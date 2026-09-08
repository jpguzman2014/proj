import type { RunSummary } from '../engine/scoring';
import type { GameMode } from '../modes/registry';
import { navigate } from '../router';

function pct(value: number): string {
  return `${Math.round(value * 100)}%`;
}

export default function Results({
  mode,
  summary,
  bestStreak,
  previousBest,
  isPersonalBest,
  onPlayAgain,
}: {
  mode: GameMode;
  summary: RunSummary;
  bestStreak: number;
  previousBest: number;
  isPersonalBest: boolean;
  onPlayAgain: () => void;
}) {
  const rows: Array<[string, string]> = [
    ['Score', String(summary.score)],
    ['Correct', `${summary.correct} of ${summary.answered}`],
    ['Accuracy', summary.answered ? pct(summary.accuracy) : '—'],
    ['Average answer time', summary.averageMs ? `${(summary.averageMs / 1000).toFixed(1)}s` : '—'],
    ['Best streak', String(bestStreak)],
    ['Clues opened', String(summary.cluesUsed)],
    ['Skipped', String(summary.skipped)],
    ['Personal best', String(Math.max(previousBest, summary.score))],
  ];

  return (
    <div className="stack">
      <h1 tabIndex={-1}>{isPersonalBest ? 'New personal best' : 'Run complete'}</h1>
      <p className="muted">
        {mode.icon} {mode.name}
      </p>

      <div className="card stack">
        <p className="big-score" aria-label={`Final score ${summary.score} points`}>
          {summary.score}
        </p>
        <table className="stats">
          <caption className="visually-hidden">Run statistics</caption>
          <tbody>
            {rows.map(([label, value]) => (
              <tr key={label}>
                <th scope="row">{label}</th>
                <td>{value}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="row">
        <button className="btn btn-primary" type="button" onClick={onPlayAgain}>
          Play again
        </button>
        <button className="btn" type="button" onClick={() => navigate(`/setup/${mode.id}`)}>
          Change filters
        </button>
        <button className="btn" type="button" onClick={() => navigate('/stats')}>
          Statistics
        </button>
        <button className="btn" type="button" onClick={() => navigate('/')}>
          All modes
        </button>
      </div>
    </div>
  );
}
