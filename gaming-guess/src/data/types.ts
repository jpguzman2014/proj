/**
 * The entity graph.
 *
 * Everything the app can ask a question about is an Entity. Games are entities
 * with extra metadata; characters, weapons, enemies, bosses, items, vehicles and
 * locations are entities that belong to a game. Adding a new category means
 * adding a value to EntityKind and seeding rows — no engine or UI rewrite.
 */

export type EntityKind =
  | 'game'
  | 'character'
  | 'weapon'
  | 'item'
  | 'enemy'
  | 'boss'
  | 'vehicle'
  | 'location';

export type Difficulty = 'easy' | 'medium' | 'hard' | 'expert' | 'impossible';

export const DIFFICULTY_ORDER: Difficulty[] = [
  'easy',
  'medium',
  'hard',
  'expert',
  'impossible',
];

export type Era = '1980s' | '1990s' | '2000s' | '2010s' | '2020s';

/** Platform and genre ids are plain strings so the catalogue can grow freely. */
export type PlatformId = string;
export type GenreId = string;

export interface ArtSeed {
  /** Two to four hex colours used by the generated clue art. */
  palette?: string[];
  /** Coarse shape family for the generated silhouette. */
  motif?: string;
}

export interface BaseEntity {
  id: string;
  kind: EntityKind;
  name: string;
  /** Accepted alternative spellings, used by free-text answer checking. */
  aliases?: string[];
  /** Owning game for everything that is not itself a game. */
  gameId?: string;
  /** Hero, villain, gun, sword, dungeon, spaceship … drives filters and prompts. */
  subtype?: string;
  difficulty: Difficulty;
  tags?: string[];
  /**
   * Clue lines ordered vague first, specific last. The clue engine reveals them
   * one at a time so no question is pure luck.
   */
  facts?: string[];
  art?: ArtSeed;
}

export interface Game extends BaseEntity {
  kind: 'game';
  franchise: string;
  series?: string;
  developer: string;
  publisher: string;
  releaseYear: number;
  /** Original release platforms. Ports are listed in `alsoOn`. */
  platforms: PlatformId[];
  alsoOn?: PlatformId[];
  /** Primary genre first, then secondaries. A game is never forced into one. */
  genres: GenreId[];
  subgenres?: GenreId[];
  perspective?: 'first-person' | 'third-person' | 'top-down' | 'side-on' | 'isometric';
  modes?: Array<'single-player' | 'multiplayer' | 'co-op' | 'online'>;
  /**
   * Where this row's facts came from, for example a Wikidata Q-id. Facts are not
   * copyrightable, but the EU database right protects substantial extraction
   * from someone else's compilation, so provenance per row is what shows the
   * catalogue was assembled rather than lifted. See LICENSING.md.
   */
  sources?: string[];
}

export type SubEntity = BaseEntity & { gameId: string };
export type AnyEntity = Game | SubEntity;

export function isGame(e: AnyEntity): e is Game {
  return e.kind === 'game';
}

export function eraOf(year: number): Era {
  if (year < 1990) return '1980s';
  if (year < 2000) return '1990s';
  if (year < 2010) return '2000s';
  if (year < 2020) return '2010s';
  return '2020s';
}

export interface Platform {
  id: PlatformId;
  name: string;
  family: string;
  year: number;
}

export interface Genre {
  id: GenreId;
  name: string;
}
