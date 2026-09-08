import type { Difficulty } from '../data/types';
import { difficultyRank } from './filters';

export const BASE_POINTS = 100;

/** Points kept after using N clues. Using clues is allowed, not punished hard. */
const CLUE_MULTIPLIERS = [1, 0.8, 0.62, 0.48, 0.38, 0.3];

export interface ScoreInput {
  correct: boolean;
  cluesUsed: number;
  difficulty: Difficulty;
  /** 0..1, share of the allowed time still left when the answer was locked in. */
  speedFraction: number;
  speedBonusEnabled: boolean;
  wrongPenalty: number;
}

export interface ScoreBreakdown {
  base: number;
  difficultyBonus: number;
  speedBonus: number;
  cluePenalty: number;
  penalty: number;
  total: number;
}

export function scoreAnswer(input: ScoreInput): ScoreBreakdown {
  if (!input.correct) {
    const penalty = input.wrongPenalty;
    return {
      base: 0,
      difficultyBonus: 0,
      speedBonus: 0,
      cluePenalty: 0,
      penalty,
      total: -penalty,
    };
  }

  const multiplier =
    CLUE_MULTIPLIERS[Math.min(input.cluesUsed, CLUE_MULTIPLIERS.length - 1)];
  const base = Math.round(BASE_POINTS * multiplier);
  const cluePenalty = BASE_POINTS - base;
  const difficultyBonus = difficultyRank(input.difficulty) * 15;
  const speedBonus = input.speedBonusEnabled
    ? Math.round(BASE_POINTS * clamp01(input.speedFraction))
    : 0;

  return {
    base,
    difficultyBonus,
    speedBonus,
    cluePenalty,
    penalty: 0,
    total: base + difficultyBonus + speedBonus,
  };
}

function clamp01(n: number): number {
  return Math.max(0, Math.min(1, n));
}

export interface RunSummary {
  score: number;
  answered: number;
  correct: number;
  skipped: number;
  accuracy: number;
  averageMs: number;
  cluesUsed: number;
}

export interface AnswerRecord {
  questionId: string;
  correct: boolean;
  skipped: boolean;
  ms: number;
  cluesUsed: number;
  points: number;
}

export function summarise(records: AnswerRecord[]): RunSummary {
  const answered = records.filter((r) => !r.skipped);
  const correct = answered.filter((r) => r.correct);
  const totalMs = answered.reduce((sum, r) => sum + r.ms, 0);
  return {
    score: records.reduce((sum, r) => sum + r.points, 0),
    answered: answered.length,
    correct: correct.length,
    skipped: records.length - answered.length,
    accuracy: answered.length ? correct.length / answered.length : 0,
    averageMs: answered.length ? Math.round(totalMs / answered.length) : 0,
    cluesUsed: records.reduce((sum, r) => sum + r.cluesUsed, 0),
  };
}
