import assert from 'node:assert/strict';
import { test } from 'node:test';
import { scoreAnswer, summarise, type AnswerRecord } from '../src/engine/scoring';

const base = {
  cluesUsed: 0,
  difficulty: 'easy' as const,
  speedFraction: 0,
  speedBonusEnabled: false,
  wrongPenalty: 0,
};

test('a clean correct answer scores the base points', () => {
  assert.equal(scoreAnswer({ ...base, correct: true }).total, 100);
});

test('each clue opened costs points but never all of them', () => {
  const scores = [0, 1, 2, 3, 4, 5].map(
    (cluesUsed) => scoreAnswer({ ...base, correct: true, cluesUsed }).total,
  );
  for (let i = 1; i < scores.length; i += 1) {
    assert.ok(scores[i] < scores[i - 1], `clue ${i} did not reduce the score`);
    assert.ok(scores[i] > 0, `clue ${i} wiped the score out`);
  }
});

test('harder entries are worth more', () => {
  const easy = scoreAnswer({ ...base, correct: true, difficulty: 'easy' }).total;
  const impossible = scoreAnswer({ ...base, correct: true, difficulty: 'impossible' }).total;
  assert.ok(impossible > easy);
});

test('the speed bonus only applies when the mode enables it', () => {
  const off = scoreAnswer({ ...base, correct: true, speedFraction: 1 }).total;
  const on = scoreAnswer({
    ...base,
    correct: true,
    speedFraction: 1,
    speedBonusEnabled: true,
  }).total;
  assert.equal(off, 100);
  assert.equal(on, 200);
});

test('a wrong answer applies the mode penalty and nothing else', () => {
  const result = scoreAnswer({ ...base, correct: false, wrongPenalty: 25 });
  assert.equal(result.total, -25);
  assert.equal(result.speedBonus, 0);
});

test('summarise reports accuracy over answered questions only', () => {
  const records: AnswerRecord[] = [
    { questionId: 'a', correct: true, skipped: false, ms: 2000, cluesUsed: 0, points: 100 },
    { questionId: 'b', correct: false, skipped: false, ms: 4000, cluesUsed: 1, points: 0 },
    { questionId: 'c', correct: false, skipped: true, ms: 0, cluesUsed: 0, points: 0 },
  ];
  const summary = summarise(records);
  assert.equal(summary.answered, 2);
  assert.equal(summary.correct, 1);
  assert.equal(summary.skipped, 1);
  assert.equal(summary.accuracy, 0.5);
  assert.equal(summary.averageMs, 3000);
  assert.equal(summary.score, 100);
});
