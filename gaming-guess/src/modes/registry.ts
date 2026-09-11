import { QUESTION_TYPE_BY_ID, typeIdsForGroups, type QuestionGroup } from '../engine/questionTypes';

export interface ModeRules {
  /** Whole-run countdown in seconds. Undefined means untimed. */
  totalSeconds?: number;
  /** Per-question countdown. Undefined means no per-question limit. */
  perQuestionSeconds?: number;
  /** Fixed run length. Undefined means the run continues until it ends itself. */
  questionCount?: number;
  /** Mistakes allowed before the run ends. Undefined means unlimited. */
  lives?: number;
  speedBonus: boolean;
  wrongPenalty: number;
  /** Seconds taken off the clock for each clue. Only meaningful when timed. */
  clueTimeCost: number;
  maxClues: number;
  allowSkip: boolean;
  /** Everyone gets the same run on a given date. */
  dailySeed?: boolean;
}

export interface GameMode {
  id: string;
  name: string;
  icon: string;
  tagline: string;
  description: string;
  groups: QuestionGroup[];
  /** Exact question types, when a group is too broad. Overrides `groups`. */
  typeIds?: string[];
  rules: ModeRules;
  /** Shown on the home screen above the fold. */
  primary: boolean;
}

const standard = {
  speedBonus: false,
  wrongPenalty: 0,
  clueTimeCost: 0,
  maxClues: 4,
  allowSkip: true,
} satisfies Partial<ModeRules>;

/**
 * Modes are configuration, not code paths. A new mode is one entry here: the
 * run screen, scoring, stats and party mode all read these rules.
 */
export const MODES: GameMode[] = [
  {
    id: 'guess-the-game',
    name: 'Guess the Game',
    icon: '🎮',
    tagline: 'Clue art and progressive hints',
    description:
      'Identify the game from stylised clue art. Each clue you open reveals more of the picture and costs points.',
    groups: ['game'],
    rules: { ...standard, questionCount: 10 },
    primary: true,
  },
  {
    id: 'guess-the-character',
    name: 'Guess the Character',
    icon: '👤',
    tagline: 'Heroes, villains and companions',
    description: 'Name the character, or work out which game they came from.',
    groups: ['character'],
    rules: { ...standard, questionCount: 10 },
    primary: true,
  },
  {
    id: 'guess-the-weapon',
    name: 'Guess the Weapon',
    icon: '🔫',
    tagline: 'Swords, guns and stranger things',
    description: 'Weapons and items, from the iconic to the obscure.',
    groups: ['weapon', 'item'],
    rules: { ...standard, questionCount: 10 },
    primary: true,
  },
  {
    id: 'guess-the-enemy',
    name: 'Guess the Enemy',
    icon: '👾',
    tagline: 'Bosses, grunts and horrors',
    description: 'Enemies and bosses across four decades of games.',
    groups: ['enemy', 'boss'],
    rules: { ...standard, questionCount: 10 },
    primary: true,
  },
  {
    id: 'time-attack',
    name: 'Time Attack',
    icon: '⚡',
    tagline: '60 seconds, as many as you can',
    description:
      'Rapid questions on a 60 second clock. Fast answers score a speed bonus, wrong answers cost 25, clues cost 2 seconds each.',
    groups: ['game', 'character', 'weapon', 'item', 'enemy', 'boss', 'vehicle', 'location', 'trivia'],
    rules: {
      totalSeconds: 60,
      speedBonus: true,
      wrongPenalty: 25,
      clueTimeCost: 2,
      maxClues: 3,
      allowSkip: true,
    },
    primary: true,
  },
  {
    id: 'open-source-arcade',
    name: 'Open Source Arcade',
    icon: '🐧',
    tagline: 'Real screenshots, free to show',
    description:
      'Real screenshots of open-source games, revealed a crop at a time. These are the only games the app can legally show you a picture of, because their communities publish them under free licences. Every image credits its author.',
    groups: ['game'],
    typeIds: ['GAME_FROM_SCREENSHOT'],
    rules: { ...standard, questionCount: 10 },
    primary: true,
  },
  {
    id: 'quick-play',
    name: 'Quick Play',
    icon: '🎲',
    tagline: '10 random questions',
    description: 'Ten questions drawn from every category in the database.',
    groups: ['game', 'character', 'weapon', 'item', 'enemy', 'boss', 'vehicle', 'location', 'trivia'],
    rules: { ...standard, questionCount: 10 },
    primary: false,
  },
  {
    id: 'daily',
    name: 'Daily Challenge',
    icon: '📅',
    tagline: 'Same 10 for everyone, every day',
    description:
      'Ten questions seeded from today’s date. Everyone playing today gets the same run. Practice modes stay unlimited.',
    groups: ['game', 'character', 'weapon', 'item', 'enemy', 'boss', 'vehicle', 'location', 'trivia'],
    rules: { ...standard, questionCount: 10, dailySeed: true },
    primary: false,
  },
  {
    id: 'endless',
    name: 'Endless',
    icon: '♾️',
    tagline: 'Three lives, no finish line',
    description: 'Keep answering until you have used three wrong answers.',
    groups: ['game', 'character', 'weapon', 'item', 'enemy', 'boss', 'vehicle', 'location', 'trivia'],
    rules: { ...standard, lives: 3, allowSkip: false },
    primary: false,
  },
  {
    id: 'perfect-run',
    name: 'Perfect Run',
    icon: '💎',
    tagline: 'One mistake ends it',
    description: 'No second chances. How far can you get without a single wrong answer?',
    groups: ['game', 'character', 'weapon', 'item', 'enemy', 'boss', 'vehicle', 'location', 'trivia'],
    rules: { ...standard, lives: 1, allowSkip: false },
    primary: false,
  },
  {
    id: 'trivia',
    name: 'Gaming Trivia',
    icon: '🧠',
    tagline: 'Years, studios and publishers',
    description: 'Release years, developers, publishers and launch platforms.',
    groups: ['trivia'],
    rules: { ...standard, questionCount: 10 },
    primary: false,
  },
];

export const MODE_BY_ID = new Map(MODES.map((mode) => [mode.id, mode]));

export function typeIdsForMode(mode: GameMode): string[] {
  return mode.typeIds ?? typeIdsForGroups(mode.groups);
}

/** True when every question type in the mode needs a screenshot to exist. */
export function modeNeedsMedia(mode: GameMode): boolean {
  const types = typeIdsForMode(mode).map((id) => QUESTION_TYPE_BY_ID.get(id));
  return types.length > 0 && types.every((type) => type?.requiresMedia);
}

/** Party rounds reuse the solo mode config, with speed scoring switched on. */
export const PARTY_MODE: GameMode = {
  id: 'party',
  name: 'Party Mode',
  icon: '👥',
  tagline: 'Everyone on their own phone',
  description:
    'The host screen shows the question, phones show the answers. Fastest correct answer scores most.',
  groups: ['game', 'character', 'weapon', 'item', 'enemy', 'boss', 'vehicle', 'location', 'trivia'],
  rules: {
    perQuestionSeconds: 20,
    questionCount: 15,
    speedBonus: true,
    wrongPenalty: 0,
    clueTimeCost: 0,
    maxClues: 2,
    allowSkip: false,
  },
  primary: false,
};
