import type { Genre, Platform } from './types';

/** Hardware the seed database references. Ordered roughly by release. */
export const PLATFORMS: Platform[] = [
  { id: 'arcade', name: 'Arcade', family: 'Arcade', year: 1971 },
  { id: 'nes', name: 'NES', family: 'Nintendo', year: 1983 },
  { id: 'gameboy', name: 'Game Boy', family: 'Nintendo', year: 1989 },
  { id: 'genesis', name: 'Mega Drive / Genesis', family: 'Sega', year: 1988 },
  { id: 'snes', name: 'SNES', family: 'Nintendo', year: 1990 },
  { id: 'ps1', name: 'PlayStation', family: 'PlayStation', year: 1994 },
  { id: 'saturn', name: 'Sega Saturn', family: 'Sega', year: 1994 },
  { id: 'n64', name: 'Nintendo 64', family: 'Nintendo', year: 1996 },
  { id: 'dreamcast', name: 'Dreamcast', family: 'Sega', year: 1998 },
  { id: 'ps2', name: 'PlayStation 2', family: 'PlayStation', year: 2000 },
  { id: 'gamecube', name: 'GameCube', family: 'Nintendo', year: 2001 },
  { id: 'xbox', name: 'Xbox', family: 'Xbox', year: 2001 },
  { id: 'gba', name: 'Game Boy Advance', family: 'Nintendo', year: 2001 },
  { id: 'nds', name: 'Nintendo DS', family: 'Nintendo', year: 2004 },
  { id: 'psp', name: 'PSP', family: 'PlayStation', year: 2004 },
  { id: 'x360', name: 'Xbox 360', family: 'Xbox', year: 2005 },
  { id: 'ps3', name: 'PlayStation 3', family: 'PlayStation', year: 2006 },
  { id: 'wii', name: 'Wii', family: 'Nintendo', year: 2006 },
  { id: 'psvita', name: 'PS Vita', family: 'PlayStation', year: 2011 },
  { id: 'wiiu', name: 'Wii U', family: 'Nintendo', year: 2012 },
  { id: 'ps4', name: 'PlayStation 4', family: 'PlayStation', year: 2013 },
  { id: 'xone', name: 'Xbox One', family: 'Xbox', year: 2013 },
  { id: 'switch', name: 'Nintendo Switch', family: 'Nintendo', year: 2017 },
  { id: 'ps5', name: 'PlayStation 5', family: 'PlayStation', year: 2020 },
  { id: 'xseries', name: 'Xbox Series X|S', family: 'Xbox', year: 2020 },
  { id: 'pc', name: 'PC', family: 'PC', year: 1981 },
  { id: 'mobile', name: 'Mobile', family: 'Mobile', year: 2008 },
];

export const PLATFORM_BY_ID = new Map(PLATFORMS.map((p) => [p.id, p]));

export function platformName(id: string): string {
  return PLATFORM_BY_ID.get(id)?.name ?? id;
}

/**
 * Genres are a flat list applied as an array per game: a primary genre first,
 * then any number of secondaries. Nothing is forced into a single bucket.
 */
export const GENRES: Genre[] = [
  { id: 'action', name: 'Action' },
  { id: 'adventure', name: 'Adventure' },
  { id: 'action-adventure', name: 'Action-Adventure' },
  { id: 'rpg', name: 'RPG' },
  { id: 'jrpg', name: 'JRPG' },
  { id: 'action-rpg', name: 'Action RPG' },
  { id: 'fps', name: 'First-Person Shooter' },
  { id: 'tps', name: 'Third-Person Shooter' },
  { id: 'horror', name: 'Horror' },
  { id: 'survival-horror', name: 'Survival Horror' },
  { id: 'racing', name: 'Racing' },
  { id: 'sports', name: 'Sports' },
  { id: 'fighting', name: 'Fighting' },
  { id: 'platformer', name: 'Platformer' },
  { id: 'metroidvania', name: 'Metroidvania' },
  { id: 'strategy', name: 'Strategy' },
  { id: 'simulation', name: 'Simulation' },
  { id: 'puzzle', name: 'Puzzle' },
  { id: 'stealth', name: 'Stealth' },
  { id: 'roguelike', name: 'Roguelike' },
  { id: 'sandbox', name: 'Sandbox' },
  { id: 'survival', name: 'Survival' },
  { id: 'party', name: 'Party' },
  { id: 'maze', name: 'Maze' },
  { id: 'shooter', name: 'Shooter' },
];

export const GENRE_BY_ID = new Map(GENRES.map((g) => [g.id, g]));

export function genreName(id: string): string {
  return GENRE_BY_ID.get(id)?.name ?? id;
}

/** Cross-cutting tags. A game carries as many as apply. */
export const TAGS = [
  'open-world',
  'single-player',
  'multiplayer',
  'co-op',
  'first-person',
  'third-person',
  'fantasy',
  'sci-fi',
  'post-apocalyptic',
  'western',
  'survival',
  'retro',
  'indie',
  'aaa',
  'linear',
  'souls-like',
  'pixel-art',
  'story-driven',
] as const;

export type Tag = (typeof TAGS)[number];
