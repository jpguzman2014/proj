import { allPlatformsOf, gameOf } from '../data/db';
import { genreName, platformName } from '../data/catalogue';
import type { AnyEntity, Game } from '../data/types';
import { eraOf } from '../data/types';

export interface Clue {
  label: string;
  text: string;
}

/**
 * Clues, ordered vague first.
 *
 * The fairness rule from the brief: a player who runs out of clues should be
 * able to see why the answer was reachable. So the ladder always ends with
 * something decisive, and the engine never hides the last rung.
 */
export function cluesFor(entity: AnyEntity, revealGameTitle: boolean): Clue[] {
  const game = gameOf(entity);
  if (!game) return [];
  const clues: Clue[] = [];

  clues.push({ label: 'Era', text: `From the ${eraOf(game.releaseYear)}` });
  clues.push({ label: 'Genre', text: game.genres.map(genreName).join(', ') });

  if (entity.kind !== 'game' && entity.subtype) {
    clues.push({ label: 'Type', text: capitalise(entity.subtype) });
  }

  clues.push({
    label: 'Platform',
    text: `Originally on ${game.platforms.map(platformName).join(', ')}`,
  });

  const tags = game.tags ?? [];
  if (tags.length) {
    clues.push({ label: 'Tags', text: tags.slice(0, 3).map(prettyTag).join(', ') });
  }

  clues.push({ label: 'Developer', text: game.developer });
  clues.push({ label: 'Released', text: String(game.releaseYear) });

  for (const fact of entity.facts ?? game.facts ?? []) {
    clues.push({ label: 'Detail', text: fact });
  }

  if (revealGameTitle && entity.kind !== 'game') {
    clues.push({ label: 'Game', text: game.name });
  }
  if (revealGameTitle && entity.kind === 'game' && game.franchise !== game.name) {
    clues.push({ label: 'Franchise', text: game.franchise });
  }

  return clues;
}

/** Where a game can be played today, ports included. Used by the answer panel. */
export function playableOn(game: Game): string {
  return allPlatformsOf(game).map(platformName).join(', ');
}

function capitalise(text: string): string {
  return text.charAt(0).toUpperCase() + text.slice(1);
}

function prettyTag(tag: string): string {
  return tag
    .split('-')
    .map((part) => (part === 'aaa' ? 'AAA' : part.charAt(0).toUpperCase() + part.slice(1)))
    .join(' ');
}
