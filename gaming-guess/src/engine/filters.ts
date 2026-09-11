import { allPlatformsOf, db, gameOf } from '../data/db';
import type { AnyEntity, Difficulty, Era, EntityKind } from '../data/types';
import { DIFFICULTY_ORDER, eraOf } from '../data/types';

export interface Filters {
  platforms: string[];
  genres: string[];
  eras: Era[];
  difficulties: Difficulty[];
  tags: string[];
  franchises: string[];
  /** Restrict to these entity kinds. Empty means "no restriction". */
  kinds: EntityKind[];
}

export const EMPTY_FILTERS: Filters = {
  platforms: [],
  genres: [],
  eras: [],
  difficulties: [],
  tags: [],
  franchises: [],
  kinds: [],
};

export function isFilterActive(filters: Filters): boolean {
  return Object.values(filters).some((value) => value.length > 0);
}

export function countActive(filters: Filters): number {
  return Object.values(filters).reduce((n, value) => n + value.length, 0);
}

/**
 * A sub-entity inherits its game's platforms, genres, era and tags. That is what
 * lets "PlayStation only" or "horror only" work across every category at once
 * without duplicating metadata onto every weapon and boss.
 */
export function matchesFilters(entity: AnyEntity, filters: Filters): boolean {
  const game = gameOf(entity);
  if (!game) return false;

  if (filters.kinds.length && !filters.kinds.includes(entity.kind)) return false;

  if (filters.platforms.length) {
    const platforms = allPlatformsOf(game);
    if (!filters.platforms.some((p) => platforms.includes(p))) return false;
  }
  if (filters.genres.length) {
    const genres = [...game.genres, ...(game.subgenres ?? [])];
    if (!filters.genres.some((g) => genres.includes(g))) return false;
  }
  if (filters.eras.length && !filters.eras.includes(eraOf(game.releaseYear))) return false;
  if (filters.franchises.length && !filters.franchises.includes(game.franchise)) return false;
  if (filters.difficulties.length && !filters.difficulties.includes(entity.difficulty)) {
    return false;
  }
  if (filters.tags.length) {
    const tags = [...(game.tags ?? []), ...(entity.tags ?? [])];
    if (!filters.tags.some((t) => tags.includes(t))) return false;
  }
  return true;
}

export function applyFilters(entities: AnyEntity[], filters: Filters): AnyEntity[] {
  if (!isFilterActive(filters)) return entities;
  return entities.filter((entity) => matchesFilters(entity, filters));
}

/** Difficulty as a number, so scoring and sorting can reason about it. */
export function difficultyRank(difficulty: Difficulty): number {
  return DIFFICULTY_ORDER.indexOf(difficulty);
}

/** Platform ids that actually appear in the catalogue, in catalogue order. */
export function usedPlatformIds(): string[] {
  const used = new Set<string>();
  for (const game of db.games) for (const p of allPlatformsOf(game)) used.add(p);
  return [...used];
}

export function usedGenreIds(): string[] {
  const used = new Set<string>();
  for (const game of db.games) {
    for (const g of [...game.genres, ...(game.subgenres ?? [])]) used.add(g);
  }
  return [...used];
}

export function usedTags(): string[] {
  const used = new Set<string>();
  for (const game of db.games) for (const t of game.tags ?? []) used.add(t);
  return [...used].sort();
}
