import { db, gameOf } from '../data/db';
import { platformName } from '../data/catalogue';
import type { AnyEntity, EntityKind, Game } from '../data/types';
import { eraOf } from '../data/types';
import type { Rng } from './rng';

export type QuestionGroup =
  | 'game'
  | 'character'
  | 'weapon'
  | 'item'
  | 'enemy'
  | 'boss'
  | 'vehicle'
  | 'location'
  | 'trivia';

export interface BuildContext {
  rng: Rng;
  /** Entities that survived the player's filters, grouped by kind. */
  candidatesByKind: Map<EntityKind, AnyEntity[]>;
}

/**
 * A question type is data plus four small functions. Adding "guess the quote"
 * or "guess the achievement" later means appending one object here and seeding
 * rows — the engine, scoring, UI and multiplayer never change.
 */
export interface QuestionType {
  id: string;
  label: string;
  group: QuestionGroup;
  /** Which entity kinds can be the subject of this question. */
  subjectKinds: EntityKind[];
  /** The question sentence shown to the player. */
  prompt(subject: AnyEntity, game: Game): string;
  /** The correct answer, as displayed. */
  answer(subject: AnyEntity, game: Game): string;
  /** Wrong-but-plausible answers. The engine trims, dedupes and shuffles. */
  distractors(subject: AnyEntity, game: Game, ctx: BuildContext): string[];
  /** Entity whose clue art is shown, or null for a text-only question. */
  artSubject(subject: AnyEntity, game: Game): AnyEntity | null;
  /**
   * Whether the clue ladder is allowed to name the owning game. False when the
   * game title would give the answer away.
   */
  revealsGameTitle: boolean;
}

function sameKind(kind: EntityKind, ctx: BuildContext): AnyEntity[] {
  return ctx.candidatesByKind.get(kind) ?? (db.byKind.get(kind) ?? []);
}

/**
 * Distractors from the same kind, preferring the same era so a 1985 platformer
 * is not sitting next to a 2022 open-world RPG. That closeness is what stops a
 * question being guessable from the shape of the options alone.
 */
function nearbyEntities(subject: AnyEntity, kind: EntityKind, ctx: BuildContext): AnyEntity[] {
  const subjectGame = gameOf(subject);
  const pool = sameKind(kind, ctx).filter((e) => e.id !== subject.id);
  const wide = pool.length ? pool : (db.byKind.get(kind) ?? []).filter((e) => e.id !== subject.id);
  if (!subjectGame) return wide;

  const era = eraOf(subjectGame.releaseYear);
  const sameEraPool = wide.filter((e) => {
    const g = gameOf(e);
    return g && g.id !== subjectGame.id && eraOf(g.releaseYear) === era;
  });
  const rest = wide.filter((e) => {
    const g = gameOf(e);
    return g && g.id !== subjectGame.id && eraOf(g.releaseYear) !== era;
  });
  return [...sameEraPool, ...rest];
}

function names(entities: AnyEntity[]): string[] {
  return entities.map((e) => e.name);
}

function otherGames(subject: AnyEntity, ctx: BuildContext): string[] {
  const game = gameOf(subject);
  const pool = nearbyEntities(subject, 'game', ctx).filter((g) => g.id !== game?.id);
  return names(pool);
}

/** "Which game is this <kind> from?" — one shape, reused for every category. */
function gameFrom(
  id: string,
  label: string,
  group: QuestionGroup,
  kind: EntityKind,
  noun: string,
): QuestionType {
  return {
    id,
    label,
    group,
    subjectKinds: [kind],
    prompt: (subject) => `Which game is this ${noun} from? (${subject.name})`,
    answer: (_subject, game) => game.name,
    distractors: (subject, _game, ctx) => otherGames(subject, ctx),
    artSubject: (subject) => subject,
    revealsGameTitle: false,
  };
}

/** "Identify this <kind>" — art plus clues, answer is the entity itself. */
function identify(
  id: string,
  label: string,
  group: QuestionGroup,
  kind: EntityKind,
  question: string,
): QuestionType {
  return {
    id,
    label,
    group,
    subjectKinds: [kind],
    prompt: () => question,
    answer: (subject) => subject.name,
    distractors: (subject, _game, ctx) => names(nearbyEntities(subject, kind, ctx)),
    artSubject: (subject) => subject,
    revealsGameTitle: false,
  };
}

export const QUESTION_TYPES: QuestionType[] = [
  {
    id: 'GAME_FROM_ART',
    label: 'Game from clue art',
    group: 'game',
    subjectKinds: ['game'],
    prompt: () => 'Which game is this?',
    answer: (_subject, game) => game.name,
    distractors: (subject, _game, ctx) => otherGames(subject, ctx),
    artSubject: (subject) => subject,
    revealsGameTitle: false,
  },
  gameFrom('GAME_FROM_CHARACTER', 'Game from character', 'game', 'character', 'character'),
  gameFrom('GAME_FROM_WEAPON', 'Game from weapon', 'game', 'weapon', 'weapon'),
  gameFrom('GAME_FROM_BOSS', 'Game from boss', 'game', 'boss', 'boss'),
  gameFrom('GAME_FROM_LOCATION', 'Game from location', 'game', 'location', 'location'),
  gameFrom('GAME_FROM_ITEM', 'Game from item', 'game', 'item', 'item'),
  gameFrom('GAME_FROM_VEHICLE', 'Game from vehicle', 'game', 'vehicle', 'vehicle'),

  identify('CHARACTER_FROM_ART', 'Identify the character', 'character', 'character', 'Who is this?'),
  identify('WEAPON_FROM_ART', 'Identify the weapon', 'weapon', 'weapon', 'Which weapon is this?'),
  identify('ITEM_FROM_ART', 'Identify the item', 'item', 'item', 'Which item is this?'),
  identify('ENEMY_FROM_ART', 'Identify the enemy', 'enemy', 'enemy', 'Which enemy is this?'),
  identify('BOSS_FROM_ART', 'Identify the boss', 'boss', 'boss', 'Which boss is this?'),
  identify('VEHICLE_FROM_ART', 'Identify the vehicle', 'vehicle', 'vehicle', 'Which vehicle is this?'),
  identify('LOCATION_FROM_ART', 'Identify the location', 'location', 'location', 'Which location is this?'),

  {
    id: 'CHARACTER_IN_GAME',
    label: 'Who appears in this game',
    group: 'character',
    subjectKinds: ['character'],
    prompt: (_subject, game) => `Which of these characters appears in ${game.name}?`,
    answer: (subject) => subject.name,
    distractors: (subject, _game, ctx) => names(nearbyEntities(subject, 'character', ctx)),
    artSubject: () => null,
    revealsGameTitle: true,
  },
  {
    id: 'BOSS_IN_GAME',
    label: 'Which boss is in this game',
    group: 'boss',
    subjectKinds: ['boss'],
    prompt: (_subject, game) => `Which of these is a boss in ${game.name}?`,
    answer: (subject) => subject.name,
    distractors: (subject, _game, ctx) => names(nearbyEntities(subject, 'boss', ctx)),
    artSubject: () => null,
    revealsGameTitle: true,
  },
  {
    id: 'YEAR_OF_GAME',
    label: 'Release year',
    group: 'trivia',
    subjectKinds: ['game'],
    prompt: (_subject, game) => `What year was ${game.name} first released?`,
    answer: (_subject, game) => String(game.releaseYear),
    distractors: (_subject, game, ctx) => {
      const offsets = [-6, -4, -3, -2, 2, 3, 4, 6];
      const shuffled = offsets.sort(() => ctx.rng() - 0.5);
      return shuffled.map((o) => String(game.releaseYear + o));
    },
    artSubject: () => null,
    revealsGameTitle: true,
  },
  {
    id: 'DEVELOPER_OF_GAME',
    label: 'Developer',
    group: 'trivia',
    subjectKinds: ['game'],
    prompt: (_subject, game) => `Who developed ${game.name}?`,
    answer: (_subject, game) => game.developer,
    distractors: (_subject, game, ctx) =>
      nearbyEntities(game, 'game', ctx)
        .map((g) => (g as Game).developer)
        .filter((developer) => developer !== game.developer),
    artSubject: () => null,
    revealsGameTitle: true,
  },
  {
    id: 'PUBLISHER_OF_GAME',
    label: 'Publisher',
    group: 'trivia',
    subjectKinds: ['game'],
    prompt: (_subject, game) => `Who published ${game.name}?`,
    answer: (_subject, game) => game.publisher,
    distractors: (_subject, game, ctx) =>
      nearbyEntities(game, 'game', ctx)
        .map((g) => (g as Game).publisher)
        .filter((publisher) => publisher !== game.publisher),
    artSubject: () => null,
    revealsGameTitle: true,
  },
  {
    id: 'PLATFORM_OF_GAME',
    label: 'Original platform',
    group: 'trivia',
    subjectKinds: ['game'],
    prompt: (_subject, game) => `Which platform did ${game.name} originally launch on?`,
    answer: (_subject, game) => platformName(game.platforms[0]),
    distractors: (_subject, game, ctx) => {
      const owned = new Set([...game.platforms, ...(game.alsoOn ?? [])]);
      return nearbyEntities(game, 'game', ctx)
        .flatMap((g) => (g as Game).platforms)
        .filter((id) => !owned.has(id))
        .map(platformName);
    },
    artSubject: () => null,
    revealsGameTitle: true,
  },
  {
    id: 'FRANCHISE_OF_CHARACTER',
    label: 'Franchise from character',
    group: 'trivia',
    subjectKinds: ['character'],
    prompt: (subject) => `Which franchise does ${subject.name} belong to?`,
    answer: (_subject, game) => game.franchise,
    distractors: (subject, game, ctx) =>
      nearbyEntities(subject, 'game', ctx)
        .map((g) => (g as Game).franchise)
        .filter((franchise) => franchise !== game.franchise),
    artSubject: () => null,
    revealsGameTitle: false,
  },
];

export const QUESTION_TYPE_BY_ID = new Map(QUESTION_TYPES.map((t) => [t.id, t]));

export function typeIdsForGroups(groups: QuestionGroup[]): string[] {
  return QUESTION_TYPES.filter((t) => groups.includes(t.group)).map((t) => t.id);
}
