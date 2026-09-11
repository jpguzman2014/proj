import assert from 'node:assert/strict';
import { test } from 'node:test';
import { checkIntegrity, db, allPlatformsOf } from '../src/data/db';
import { GENRE_BY_ID, PLATFORM_BY_ID } from '../src/data/catalogue';

test('the seed database is structurally sound', () => {
  const problems = checkIntegrity();
  assert.deepEqual(problems, [], JSON.stringify(problems, null, 2));
});

test('every platform id used by a game exists in the catalogue', () => {
  const unknown: string[] = [];
  for (const game of db.games) {
    for (const id of allPlatformsOf(game)) {
      if (!PLATFORM_BY_ID.has(id)) unknown.push(`${game.id} -> ${id}`);
    }
  }
  assert.deepEqual(unknown, []);
});

test('every genre id used by a game exists in the catalogue', () => {
  const unknown: string[] = [];
  for (const game of db.games) {
    for (const id of [...game.genres, ...(game.subgenres ?? [])]) {
      if (!GENRE_BY_ID.has(id)) unknown.push(`${game.id} -> ${id}`);
    }
  }
  assert.deepEqual(unknown, []);
});

test('each entity kind has enough rows to build fair multiple choice', () => {
  for (const [kind, entities] of db.byKind) {
    assert.ok(entities.length >= 4, `${kind} has only ${entities.length} rows`);
  }
});

test('sub-entity names are unique within their own game and kind', () => {
  const seen = new Set<string>();
  const clashes: string[] = [];
  for (const entity of db.subEntities) {
    const key = `${entity.gameId}|${entity.kind}|${entity.name.toLowerCase()}`;
    if (seen.has(key)) clashes.push(key);
    seen.add(key);
  }
  assert.deepEqual(clashes, []);
});
