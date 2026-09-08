import assert from 'node:assert/strict';
import { test } from 'node:test';
import { generateQuestions, estimateAvailable, OPTION_COUNT } from '../src/engine/engine';
import { EMPTY_FILTERS } from '../src/engine/filters';
import { MODES, modeNeedsMedia, typeIdsForMode } from '../src/modes/registry';
import { mediaCount } from '../src/media/commonsProvider';
import { QUESTION_TYPES } from '../src/engine/questionTypes';

const allTypes = QUESTION_TYPES.map((t) => t.id);

test('every mode fills a run, or runs as long as its pool allows', () => {
  for (const mode of MODES) {
    const wanted = mode.rules.questionCount ?? 12;
    const typeIds = typeIdsForMode(mode);
    const questions = generateQuestions({
      filters: EMPTY_FILTERS,
      typeIds,
      count: wanted,
      seed: `test:${mode.id}`,
    });
    // A screenshot mode is bounded by how many games have a freely licensed
    // picture, which is a smaller pool than the metadata modes draw on and is
    // empty before the import runs. Asking for a fixed ten would be asserting
    // the size of Wikimedia Commons rather than the behaviour of the engine.
    const ceiling = Math.min(wanted, estimateAvailable(EMPTY_FILTERS, typeIds));
    assert.equal(
      questions.length,
      ceiling,
      `${mode.id} produced ${questions.length}, expected ${ceiling}`,
    );
  }
});

test('a screenshot mode is empty before the import and populated after', () => {
  const arcade = MODES.find((mode) => modeNeedsMedia(mode));
  assert.ok(arcade, 'no media-backed mode is registered');
  const available = estimateAvailable(EMPTY_FILTERS, typeIdsForMode(arcade));
  assert.equal(available > 0, mediaCount() > 0);
});

test('every question is well formed', () => {
  const questions = generateQuestions({
    filters: EMPTY_FILTERS,
    typeIds: allTypes,
    count: 300,
    seed: 'well-formed',
  });
  assert.ok(questions.length > 200, `only produced ${questions.length}`);
  for (const question of questions) {
    assert.equal(question.options.length, OPTION_COUNT, question.id);
    assert.equal(new Set(question.options).size, OPTION_COUNT, `duplicate option in ${question.id}`);
    assert.ok(question.answerIndex >= 0 && question.answerIndex < OPTION_COUNT, question.id);
    assert.ok(question.prompt.length > 0, question.id);
    assert.ok(question.clues.length >= 3, `${question.id} has ${question.clues.length} clues`);
    assert.ok(question.explanation.length > 0, question.id);
  }
});

test('a question never leaks its answer through the prompt or the clues', () => {
  const questions = generateQuestions({
    filters: EMPTY_FILTERS,
    typeIds: allTypes,
    count: 300,
    seed: 'no-leak',
  });
  for (const question of questions) {
    const answer = question.options[question.answerIndex].toLowerCase();
    assert.ok(
      !question.prompt.toLowerCase().includes(answer),
      `${question.id} prompt contains the answer`,
    );
    for (const clue of question.clues) {
      assert.ok(
        !clue.text.toLowerCase().includes(answer),
        `${question.id} clue "${clue.label}" contains the answer`,
      );
    }
  }
});

test('the same seed produces the same run', () => {
  const a = generateQuestions({
    filters: EMPTY_FILTERS,
    typeIds: allTypes,
    count: 10,
    seed: 'daily:2026-09-08',
  });
  const b = generateQuestions({
    filters: EMPTY_FILTERS,
    typeIds: allTypes,
    count: 10,
    seed: 'daily:2026-09-08',
  });
  assert.deepEqual(
    a.map((q) => `${q.id}:${q.answerIndex}`),
    b.map((q) => `${q.id}:${q.answerIndex}`),
  );
});

test('a run does not repeat a question', () => {
  const questions = generateQuestions({
    filters: EMPTY_FILTERS,
    typeIds: allTypes,
    count: 60,
    seed: 'no-repeats',
  });
  assert.equal(new Set(questions.map((q) => q.id)).size, questions.length);
});

test('estimateAvailable is non-zero for every mode that can run', () => {
  for (const mode of MODES) {
    if (modeNeedsMedia(mode) && mediaCount() === 0) continue;
    assert.ok(estimateAvailable(EMPTY_FILTERS, typeIdsForMode(mode)) > 0, mode.id);
  }
});
