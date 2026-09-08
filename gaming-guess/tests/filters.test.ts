import assert from 'node:assert/strict';
import { test } from 'node:test';
import { db, gameOf } from '../src/data/db';
import { EMPTY_FILTERS, matchesFilters } from '../src/engine/filters';
import { generateQuestions } from '../src/engine/engine';
import { QUESTION_TYPES } from '../src/engine/questionTypes';
import { eraOf } from '../src/data/types';

const allTypes = QUESTION_TYPES.map((t) => t.id);

test('a platform filter keeps only games released or ported to that platform', () => {
  const filters = { ...EMPTY_FILTERS, platforms: ['ps2'] };
  for (const game of db.games) {
    const has = [...game.platforms, ...(game.alsoOn ?? [])].includes('ps2');
    assert.equal(matchesFilters(game, filters), has, game.id);
  }
});

test('sub-entities inherit their game filters', () => {
  const filters = { ...EMPTY_FILTERS, genres: ['survival-horror'] };
  for (const entity of db.subEntities) {
    const game = gameOf(entity);
    const expected = Boolean(game?.genres.includes('survival-horror'));
    assert.equal(matchesFilters(entity, filters), expected, entity.id);
  }
});

test('a filtered run only contains matching questions', () => {
  const filters = { ...EMPTY_FILTERS, eras: ['1990s' as const] };
  const questions = generateQuestions({
    filters,
    typeIds: allTypes,
    count: 40,
    seed: 'nineties',
  });
  assert.ok(questions.length > 10, `only produced ${questions.length}`);
  for (const question of questions) {
    const game = db.byId.get(question.gameId);
    assert.ok(game && game.kind === 'game');
    assert.equal(eraOf((game as { releaseYear: number }).releaseYear), '1990s', question.id);
  }
});

test('a narrow filter degrades to fewer questions rather than broken ones', () => {
  const filters = { ...EMPTY_FILTERS, franchises: ['Portal'] };
  const questions = generateQuestions({
    filters,
    typeIds: allTypes,
    count: 10,
    seed: 'portal-only',
  });
  for (const question of questions) {
    assert.equal(new Set(question.options).size, question.options.length);
  }
});
