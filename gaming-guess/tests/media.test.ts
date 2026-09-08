import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { test } from 'node:test';
import manifest from '../src/media/manifest.json';
import { hasFreeMedia, mediaCount, gamesWithFreeMedia } from '../src/media/commonsProvider';
import { generateQuestions } from '../src/engine/engine';
import { EMPTY_FILTERS } from '../src/engine/filters';
import { db, gameOf } from '../src/data/db';
import { OPEN_GAMES } from '../src/data/openGames';

const allowed: string[] = JSON.parse(
  readFileSync(new URL('../shared/media-licences.json', import.meta.url), 'utf8'),
).allowed;

type Entry = {
  file: string;
  author: string;
  license: string;
  licenseMachine?: string;
  licenseUrl: string;
  descriptionUrl: string;
};

const assets = (manifest.assets ?? {}) as Record<string, Entry[]>;

test('every shipped image names an author, a licence and a source', () => {
  for (const [gameId, entries] of Object.entries(assets)) {
    for (const entry of entries) {
      assert.ok(entry.file, `${gameId} entry has no file`);
      assert.ok(entry.author?.trim(), `${entry.file} has no author`);
      assert.ok(entry.license?.trim(), `${entry.file} has no licence`);
      assert.ok(entry.licenseUrl?.trim(), `${entry.file} has no licence URL`);
      assert.ok(entry.descriptionUrl?.trim(), `${entry.file} has no source page`);
    }
  }
});

test('no shipped image uses a licence outside the allowlist', () => {
  // The importer enforces this at fetch time. This asserts it again on what is
  // actually committed, so hand-editing the manifest cannot slip an image past.
  for (const [gameId, entries] of Object.entries(assets)) {
    for (const entry of entries) {
      const machine = (entry.licenseMachine ?? '').toLowerCase();
      assert.ok(machine, `${entry.file} in ${gameId} records no machine-readable licence`);
      assert.ok(
        allowed.some((prefix) => machine.startsWith(prefix)),
        `${entry.file} in ${gameId} uses "${machine}", which is not on the allowlist`,
      );
    }
  }
});

test('images are only claimed for open-source games', () => {
  for (const gameId of Object.keys(assets)) {
    const game = db.byId.get(gameId);
    assert.ok(game, `manifest names unknown game ${gameId}`);
    assert.equal(
      (game as { corpus?: string }).corpus,
      'open-source',
      `${gameId} is not in the open-source corpus but has media`,
    );
  }
});

test('every open-source game is curated for the importer', () => {
  const sources = JSON.parse(readFileSync('media-sources.json', 'utf8'));
  const missing = OPEN_GAMES.filter((game) => !sources.games[game.id]).map((g) => g.id);
  assert.deepEqual(missing, [], 'open-source games with no import entry');
});

test('the importer curation only names games that exist', () => {
  const sources = JSON.parse(readFileSync('media-sources.json', 'utf8'));
  const unknown = Object.keys(sources.games).filter((id) => !db.byId.has(id));
  assert.deepEqual(unknown, []);
});

test('screenshot questions never appear for a game without a screenshot', () => {
  const questions = generateQuestions({
    filters: EMPTY_FILTERS,
    typeIds: ['GAME_FROM_SCREENSHOT'],
    count: 40,
    seed: 'screenshots',
  });
  // With no media imported this is empty, which is the point of the assertion.
  assert.equal(questions.length, mediaCount() === 0 ? 0 : questions.length);
  for (const question of questions) {
    assert.ok(hasFreeMedia(question.gameId), `${question.id} has no free media`);
  }
});

test('open-source games are not used as distractors for mainstream ones', () => {
  const questions = generateQuestions({
    filters: EMPTY_FILTERS,
    typeIds: ['GAME_FROM_CHARACTER'],
    count: 60,
    seed: 'corpus-separation',
  });
  assert.ok(questions.length > 20);
  let crossCorpus = 0;
  for (const question of questions) {
    const subject = db.byId.get(question.subjectId);
    const subjectCorpus = (gameOf(subject!) as { corpus?: string }).corpus ?? 'mainstream';
    for (const option of question.options) {
      const match = db.games.find((g) => g.name === option);
      if (match && (match.corpus ?? 'mainstream') !== subjectCorpus) crossCorpus += 1;
    }
  }
  // Distractors are ranked, not hard-filtered, so a few crossings are allowed
  // when the same-corpus pool runs dry. They must stay rare.
  assert.ok(crossCorpus <= questions.length * 0.1, `${crossCorpus} cross-corpus options`);
});

test('gamesWithFreeMedia agrees with hasFreeMedia', () => {
  for (const gameId of gamesWithFreeMedia()) assert.ok(hasFreeMedia(gameId));
});
