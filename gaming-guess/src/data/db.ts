import { GAMES as MAINSTREAM_GAMES } from './games';
import { OPEN_GAMES } from './openGames';
import { SUB_ENTITIES } from './entities';
import type { AnyEntity, EntityKind, Era, Game, SubEntity } from './types';
import { eraOf } from './types';

export interface Database {
  games: Game[];
  subEntities: SubEntity[];
  all: AnyEntity[];
  byId: Map<string, AnyEntity>;
  byKind: Map<EntityKind, AnyEntity[]>;
  byGame: Map<string, SubEntity[]>;
  franchises: string[];
  developers: string[];
  publishers: string[];
}

function group<T, K>(rows: T[], key: (row: T) => K): Map<K, T[]> {
  const out = new Map<K, T[]>();
  for (const row of rows) {
    const k = key(row);
    const bucket = out.get(k);
    if (bucket) bucket.push(row);
    else out.set(k, [row]);
  }
  return out;
}

/** One catalogue, two corpora. See `Corpus` in types.ts for why they are split. */
const GAMES: Game[] = [...MAINSTREAM_GAMES, ...OPEN_GAMES];
const GAME_BY_ID = new Map(GAMES.map((g) => [g.id, g]));

function buildDatabase(): Database {
  const all: AnyEntity[] = [...GAMES, ...SUB_ENTITIES];
  return {
    games: GAMES,
    subEntities: SUB_ENTITIES,
    all,
    byId: new Map(all.map((entity) => [entity.id, entity])),
    byKind: group(all, (entity) => entity.kind),
    byGame: group(SUB_ENTITIES, (entity) => entity.gameId),
    franchises: [...new Set(GAMES.map((g) => g.franchise))].sort(),
    developers: [...new Set(GAMES.map((g) => g.developer))].sort(),
    publishers: [...new Set(GAMES.map((g) => g.publisher))].sort(),
  };
}

export const db = buildDatabase();
export { GAMES, GAME_BY_ID };

export function gameOf(entity: AnyEntity): Game | undefined {
  return entity.kind === 'game'
    ? (entity as Game)
    : GAME_BY_ID.get((entity as SubEntity).gameId);
}

export function eraOfEntity(entity: AnyEntity): Era | undefined {
  const game = gameOf(entity);
  return game ? eraOf(game.releaseYear) : undefined;
}

/** Original platforms plus later ports — what "playable on" honestly means. */
export function allPlatformsOf(game: Game): string[] {
  return [...new Set([...game.platforms, ...(game.alsoOn ?? [])])];
}

export interface IntegrityProblem {
  id: string;
  problem: string;
}

/**
 * Cheap structural checks over the seed data. Run by the test suite so a bad
 * paste never reaches players as an unanswerable question.
 */
export function checkIntegrity(database: Database = db): IntegrityProblem[] {
  const problems: IntegrityProblem[] = [];
  const seen = new Set<string>();
  for (const entity of database.all) {
    if (seen.has(entity.id)) problems.push({ id: entity.id, problem: 'duplicate id' });
    seen.add(entity.id);
    if (!entity.name.trim()) problems.push({ id: entity.id, problem: 'empty name' });
    if (entity.kind !== 'game') {
      const sub = entity as SubEntity;
      if (!GAME_BY_ID.has(sub.gameId)) {
        problems.push({ id: entity.id, problem: `unknown gameId ${sub.gameId}` });
      }
    }
  }
  for (const game of database.games) {
    if (!game.platforms.length) problems.push({ id: game.id, problem: 'no platforms' });
    if (!game.genres.length) problems.push({ id: game.id, problem: 'no genres' });
    if (game.releaseYear < 1970 || game.releaseYear > 2030) {
      problems.push({ id: game.id, problem: `implausible year ${game.releaseYear}` });
    }
    const overlap = (game.alsoOn ?? []).filter((p) => game.platforms.includes(p));
    if (overlap.length) {
      problems.push({ id: game.id, problem: `alsoOn repeats platforms: ${overlap.join(', ')}` });
    }
  }
  return problems;
}
