import type { AnyEntity, Game } from '../data/types';
import { hashString } from '../engine/rng';
import manifest from './manifest.json';
import { generatedArtProvider } from './generatedArt';
import { registerProvider, type MediaAsset, type MediaProvider } from './provider';

export interface ManifestEntry {
  file: string;
  title: string;
  descriptionUrl: string;
  author: string;
  license: string;
  licenseMachine?: string;
  licenseUrl: string;
  width: number;
  height: number;
}

const assets = (manifest.assets ?? {}) as Record<string, ManifestEntry[]>;

/**
 * Where the images came from, taken from the manifest rather than hardcoded.
 * A credit line that names a source the file did not come from is a false
 * attribution, which is worse than no attribution.
 */
const source = (manifest as { source?: string }).source ?? 'unknown source';

export function hasFreeMedia(gameId: string): boolean {
  return (assets[gameId]?.length ?? 0) > 0;
}

export function gamesWithFreeMedia(): string[] {
  return Object.keys(assets).filter((id) => assets[id].length > 0);
}

export function mediaCount(): number {
  return Object.values(assets).reduce((sum, list) => sum + list.length, 0);
}

/**
 * Picks the same image for the same entity every time, so a question looks
 * identical on the host screen and on a player's phone.
 */
function pickEntry(entityId: string, gameId: string): ManifestEntry | null {
  const list = assets[gameId];
  if (!list?.length) return null;
  return list[hashString(entityId) % list.length];
}

/**
 * Reveal geometry for a real photograph, rather than for drawn shapes.
 *
 * Early clues show a small, blurred, off-centre crop; the last shows the whole
 * frame sharp. The focal point is derived from the file name so it is stable,
 * and kept away from the edges so a crop never lands on empty sky.
 */
export function revealTransform(entry: ManifestEntry, reveal: number, maxReveal: number) {
  const steps = Math.max(1, maxReveal);
  const t = Math.min(1, reveal / steps);
  const seed = hashString(entry.file);
  return {
    scale: Number((3.2 - 2.2 * t).toFixed(3)),
    blurPx: Number((5.5 * (1 - t) ** 1.5).toFixed(2)),
    originX: 30 + (seed % 41),
    originY: 30 + ((seed >> 8) % 41),
    complete: reveal >= steps,
  };
}

/**
 * Serves real screenshots for the games that have freely licensed ones, and
 * hands anything else back to the generated provider. Credit and licence come
 * from the manifest, so an image can never be shown without them.
 */
export const commonsProvider: MediaProvider = {
  id: 'commons',
  name: 'Wikimedia Commons, free licences only',
  licenseSummary:
    'Screenshots of open-source games, taken from Wikimedia Commons under licences that permit commercial reuse. Every image names its author and licence. Games without a freely licensed screenshot fall back to generated clue art.',
  get(entity: AnyEntity, game: Game, reveal: number, maxReveal: number): MediaAsset {
    const entry = pickEntry(entity.id, game.id);
    if (!entry) return generatedArtProvider.get(entity, game, reveal, maxReveal);

    const { complete } = revealTransform(entry, reveal, maxReveal);
    return {
      kind: 'remote-image',
      url: `/media/${entry.file}`,
      alt: complete
        ? `Screenshot from the open-source game ${game.name}.`
        : 'A small, blurred crop of a screenshot. More of it is revealed with each clue.',
      credit: `${entry.author} via ${source}`,
      license: entry.license,
      licenseUrl: entry.licenseUrl,
      sourceUrl: entry.descriptionUrl,
    };
  },
};

registerProvider(commonsProvider, true);
