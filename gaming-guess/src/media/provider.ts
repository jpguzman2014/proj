import type { AnyEntity, Game } from '../data/types';

/**
 * Media is deliberately separated from metadata.
 *
 * Game screenshots, cover art and character renders are almost all owned by
 * their publishers. Scraping them and redistributing them is a licensing
 * problem, not a technical one, so the app ships with a provider that draws its
 * own art and leaves a seam for a licensed source later. Swapping providers
 * touches this file and nothing else.
 */
export interface MediaAsset {
  kind: 'generated-svg' | 'remote-image';
  /** Inline SVG markup, for generated assets. */
  svg?: string;
  /** Absolute URL, for licensed remote assets. */
  url?: string;
  /** Always present. Rendered as the accessible description of the image. */
  alt: string;
  /** Shown next to the image. Never empty for third-party media. */
  credit: string;
  /** Licence identifier, e.g. "CC BY-SA 4.0" or "generated". */
  license: string;
}

export interface MediaProvider {
  id: string;
  name: string;
  /** One line the UI can show so players know where art comes from. */
  licenseSummary: string;
  /**
   * @param reveal 0 = smallest crop, heaviest obscuring; higher = more visible.
   * @param maxReveal the top of the ladder for this question.
   */
  get(entity: AnyEntity, game: Game, reveal: number, maxReveal: number): MediaAsset;
}

const providers = new Map<string, MediaProvider>();
let activeId = '';

export function registerProvider(provider: MediaProvider, makeActive = false): void {
  providers.set(provider.id, provider);
  if (makeActive || !activeId) activeId = provider.id;
}

export function listProviders(): MediaProvider[] {
  return [...providers.values()];
}

export function setActiveProvider(id: string): void {
  if (providers.has(id)) activeId = id;
}

export function activeProvider(): MediaProvider {
  const provider = providers.get(activeId);
  if (!provider) throw new Error('No media provider registered');
  return provider;
}

export function getMedia(
  entity: AnyEntity,
  game: Game,
  reveal: number,
  maxReveal: number,
): MediaAsset {
  return activeProvider().get(entity, game, reveal, maxReveal);
}
