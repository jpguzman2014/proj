import assert from 'node:assert/strict';
import { test } from 'node:test';
import { generateQuestions, estimateAvailable, OPTION_COUNT } from '../src/engine/engine';
import { EMPTY_FILTERS } from '../src/engine/filters';
import { MODES, typeIdsForMode } from '../src/modes/registry';
import { QUESTION_TYPES } from '../src/engine/questionTypes';

const allTypes = QUESTION_TYPES.map((t) => t.id);

test('every mode can fill a full run with no filters applied', () => {
  for (const mode of MODES) {
    const wanted = mode.rules.questionCount ?? 12;
    const questions = generateQuestions({
      filters: EMPTY_FILTERS,
      typeIds: typeIdsForMode(mode),
      count: wanted,
      seed: `test:${mode.id}`,
    });
    assert.equal(questions.length, wanted, `${mode.id} produced ${questions.length}`);
  }
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

test('estimateAvailable is non-zero for every mode', () => {
  for (const mode of MODES) {
    assert.ok(estimateAvailable(EMPTY_FILTERS, typeIdsForMode(mode)) > 0, mode.id);
  }
});
