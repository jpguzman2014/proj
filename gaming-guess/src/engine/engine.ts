import { db, gameOf } from '../data/db';
import { hasFreeMedia } from '../media/commonsProvider';
import type { AnyEntity, Difficulty, EntityKind, Game } from '../data/types';
import { cluesFor, type Clue } from './clues';
import { applyFilters, difficultyRank, matchesFilters, type Filters } from './filters';
import { QUESTION_TYPE_BY_ID, type BuildContext, type QuestionType } from './questionTypes';
import { pick, rngFrom, shuffle, type Rng } from './rng';

export const OPTION_COUNT = 4;

export interface Question {
  id: string;
  typeId: string;
  typeLabel: string;
  prompt: string;
  options: string[];
  answerIndex: number;
  clues: Clue[];
  /** Entity id whose generated clue art is shown, or null for text-only. */
  artEntityId: string | null;
  difficulty: Difficulty;
  subjectId: string;
  gameId: string;
  /** Shown after answering, so the player can see why it was answerable. */
  explanation: string;
}

export interface GenerateOptions {
  filters: Filters;
  typeIds: string[];
  count: number;
  seed: string;
  /** Skip questions harder than this. Undefined means no ceiling. */
  maxDifficulty?: Difficulty;
  /** Question ids already used in this session, to avoid immediate repeats. */
  exclude?: Set<string>;
}

function buildContext(rng: Rng, filters: Filters): BuildContext {
  const candidatesByKind = new Map<EntityKind, AnyEntity[]>();
  for (const [kind, entities] of db.byKind) {
    candidatesByKind.set(kind, applyFilters(entities, filters));
  }
  return { rng, candidatesByKind };
}

function subjectsFor(type: QuestionType, ctx: BuildContext, max?: Difficulty): AnyEntity[] {
  const ceiling = max ? difficultyRank(max) : Infinity;
  return type.subjectKinds
    .flatMap((kind) => ctx.candidatesByKind.get(kind) ?? [])
    .filter((entity) => difficultyRank(entity.difficulty) <= ceiling)
    .filter((entity) => {
      // A screenshot question with no screenshot is unanswerable, so the pool
      // is empty rather than the question being wrong. Before the media import
      // has run this simply means those modes have nothing to draw on.
      if (!type.requiresMedia) return true;
      const game = gameOf(entity);
      return Boolean(game && hasFreeMedia(game.id));
    });
}

function buildQuestion(
  type: QuestionType,
  subject: AnyEntity,
  ctx: BuildContext,
): Question | null {
  const game = gameOf(subject);
  if (!game) return null;

  const answer = type.answer(subject, game);
  // Some questions only apply to part of the catalogue, such as a software
  // licence. An empty answer means this subject cannot carry this question.
  if (!answer) return null;
  const distractors: string[] = [];
  const seen = new Set([answer.toLowerCase()]);
  for (const candidate of type.distractors(subject, game, ctx)) {
    const key = candidate.toLowerCase();
    if (seen.has(key)) continue;
    seen.add(key);
    distractors.push(candidate);
    if (distractors.length >= OPTION_COUNT * 3) break;
  }
  // A question with too few plausible wrong answers is unfair, not easy.
  if (distractors.length < OPTION_COUNT - 1) return null;

  const chosen = shuffle(ctx.rng, distractors).slice(0, OPTION_COUNT - 1);
  const options = shuffle(ctx.rng, [answer, ...chosen]);

  // A question that states its own answer is worse than a hard one. Drop any
  // clue that contains the answer, and reject the question outright if the
  // prompt does. This holds for every question type, including ones added later.
  const needle = answer.toLowerCase();
  if (type.prompt(subject, game).toLowerCase().includes(needle)) return null;
  const clues = cluesFor(subject, type.revealsGameTitle).filter(
    (clue) => !clue.text.toLowerCase().includes(needle),
  );
  if (clues.length < 3) return null;

  return {
    id: `${type.id}:${subject.id}`,
    typeId: type.id,
    typeLabel: type.label,
    prompt: type.prompt(subject, game),
    options,
    answerIndex: options.indexOf(answer),
    clues,
    artEntityId: type.artSubject(subject, game)?.id ?? null,
    difficulty: subject.difficulty,
    subjectId: subject.id,
    gameId: game.id,
    explanation: explain(subject, game, answer),
  };
}

/** Subtypes are stored lowercase; a couple read badly that way in a sentence. */
const SUBTYPE_LABELS: Record<string, string> = {
  npc: 'NPC',
};

function explain(subject: AnyEntity, game: Game, answer: string): string {
  if (subject.kind === 'game') {
    return `${answer} — ${game.developer}, ${game.releaseYear}.`;
  }
  const raw = subject.subtype ?? subject.kind;
  const role = SUBTYPE_LABELS[raw] ?? raw;
  // Lead with the subject, not the answer: for "which game is this vehicle
  // from", the answer is the game and repeating it reads as a tautology.
  return `${subject.name} — ${role} in ${game.name} (${game.developer}, ${game.releaseYear}).`;
}

/**
 * Builds a run of questions. One database, many question types: the same
 * entity graph feeds "which game is this", "who is this" and "what year",
 * so a new mode is a different list of type ids, not new content.
 */
export function generateQuestions(options: GenerateOptions): Question[] {
  const rng = rngFrom(options.seed);
  const ctx = buildContext(rng, options.filters);
  const types = options.typeIds
    .map((id) => QUESTION_TYPE_BY_ID.get(id))
    .filter((t): t is QuestionType => Boolean(t));
  if (!types.length) return [];

  const used = new Set(options.exclude ?? []);
  const questions: Question[] = [];
  const pools = new Map<string, AnyEntity[]>();
  for (const type of types) {
    pools.set(type.id, shuffle(rng, subjectsFor(type, ctx, options.maxDifficulty)));
  }

  let guard = options.count * 40;
  while (questions.length < options.count && guard > 0) {
    guard -= 1;
    const type = pick(rng, types);
    const pool = pools.get(type.id);
    if (!pool || !pool.length) continue;
    const subject = pool.pop() as AnyEntity;
    const question = buildQuestion(type, subject, ctx);
    if (!question || used.has(question.id)) continue;
    used.add(question.id);
    questions.push(question);
  }
  return questions;
}

/** How many questions the current filter and type selection can actually make. */
export function estimateAvailable(filters: Filters, typeIds: string[]): number {
  const rng = rngFrom('estimate');
  const ctx = buildContext(rng, filters);
  let total = 0;
  for (const id of typeIds) {
    const type = QUESTION_TYPE_BY_ID.get(id);
    if (!type) continue;
    total += subjectsFor(type, ctx).length;
  }
  return total;
}

/** Entities a set of filters keeps, for the "N games match" readout. */
export function countMatchingGames(filters: Filters): number {
  return db.games.filter((game) => matchesFilters(game, filters)).length;
}
