import type { RunSummary } from '../engine/scoring';
import { read, write } from './storage';

export interface ModeStats {
  played: number;
  bestScore: number;
  totalScore: number;
  totalCorrect: number;
  totalAnswered: number;
  bestStreak: number;
  fastestMs: number | null;
  lastPlayed: string | null;
}

export interface RunHistoryEntry {
  modeId: string;
  score: number;
  correct: number;
  answered: number;
  at: string;
}

const EMPTY: ModeStats = {
  played: 0,
  bestScore: 0,
  totalScore: 0,
  totalCorrect: 0,
  totalAnswered: 0,
  bestStreak: 0,
  fastestMs: null,
  lastPlayed: null,
};

export function statsFor(modeId: string): ModeStats {
  return read<ModeStats>(`stats:${modeId}`, EMPTY);
}

export function allStats(modeIds: string[]): Record<string, ModeStats> {
  return Object.fromEntries(modeIds.map((id) => [id, statsFor(id)]));
}

export function recordRun(
  modeId: string,
  summary: RunSummary,
  bestStreak: number,
): { previousBest: number; isPersonalBest: boolean } {
  const current = statsFor(modeId);
  const isPersonalBest = summary.score > current.bestScore && summary.answered > 0;
  const next: ModeStats = {
    played: current.played + 1,
    bestScore: Math.max(current.bestScore, summary.score),
    totalScore: current.totalScore + summary.score,
    totalCorrect: current.totalCorrect + summary.correct,
    totalAnswered: current.totalAnswered + summary.answered,
    bestStreak: Math.max(current.bestStreak, bestStreak),
    fastestMs:
      summary.averageMs > 0
        ? Math.min(current.fastestMs ?? Number.MAX_SAFE_INTEGER, summary.averageMs)
        : current.fastestMs,
    lastPlayed: new Date().toISOString(),
  };
  write(`stats:${modeId}`, next);

  const history = read<RunHistoryEntry[]>('history', []);
  history.unshift({
    modeId,
    score: summary.score,
    correct: summary.correct,
    answered: summary.answered,
    at: next.lastPlayed as string,
  });
  write('history', history.slice(0, 50));

  return { previousBest: current.bestScore, isPersonalBest };
}

export function history(): RunHistoryEntry[] {
  return read<RunHistoryEntry[]>('history', []);
}

/* ---- community difficulty: collected locally, ready to sync later ---- */

export interface DifficultyVote {
  questionId: string;
  stars: number;
  at: string;
}

export function rateDifficulty(questionId: string, stars: number): void {
  const votes = read<DifficultyVote[]>('difficulty-votes', []);
  votes.unshift({ questionId, stars, at: new Date().toISOString() });
  write('difficulty-votes', votes.slice(0, 500));
}

export function difficultyVotes(): DifficultyVote[] {
  return read<DifficultyVote[]>('difficulty-votes', []);
}

/** Average of this device's votes. Becomes a community score once synced. */
export function localDifficultyAverage(questionId: string): number | null {
  const votes = difficultyVotes().filter((v) => v.questionId === questionId);
  if (!votes.length) return null;
  return votes.reduce((sum, v) => sum + v.stars, 0) / votes.length;
}

/* ---- data corrections reported by players ---- */

export interface DataReport {
  questionId: string;
  subjectId: string;
  reason: string;
  note: string;
  at: string;
}

export function reportData(report: Omit<DataReport, 'at'>): void {
  const reports = read<DataReport[]>('reports', []);
  reports.unshift({ ...report, at: new Date().toISOString() });
  write('reports', reports.slice(0, 200));
}

export function dataReports(): DataReport[] {
  return read<DataReport[]>('reports', []);
}

export function daysDailyKey(dateKey: string): string {
  return `daily:${dateKey}`;
}

export function markDailyDone(dateKey: string, score: number): void {
  write(daysDailyKey(dateKey), { score, at: new Date().toISOString() });
}

export function dailyResult(dateKey: string): { score: number; at: string } | null {
  return read<{ score: number; at: string } | null>(daysDailyKey(dateKey), null);
}
