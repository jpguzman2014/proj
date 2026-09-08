import vocabulary from '../../shared/nicknames.json';

/**
 * Party nicknames come from a fixed vocabulary and are never typed.
 *
 * A free-text field that one player types and another player reads is
 * user-generated content, which puts a service inside online safety regimes such
 * as the UK Online Safety Act and brings moderation, reporting and risk
 * assessment duties with it. A picker removes that surface completely, for the
 * price of a little personality. The server validates against the same lists,
 * because a modified client can send whatever it likes.
 */
const { adjectives, animals } = vocabulary;

export const NICKNAME_COMBINATIONS = adjectives.length * animals.length;

export function randomNickname(): string {
  const adjective = adjectives[Math.floor(Math.random() * adjectives.length)];
  const animal = animals[Math.floor(Math.random() * animals.length)];
  return `${adjective} ${animal}`;
}

/** Offers a few at once so choosing feels like a choice, not a reroll grind. */
export function nicknameChoices(count = 4): string[] {
  const chosen = new Set<string>();
  let guard = count * 20;
  while (chosen.size < count && guard > 0) {
    chosen.add(randomNickname());
    guard -= 1;
  }
  return [...chosen];
}

export function isValidNickname(candidate: string): boolean {
  const parts = candidate.trim().split(' ');
  if (parts.length !== 2) return false;
  return adjectives.includes(parts[0]) && animals.includes(parts[1]);
}
