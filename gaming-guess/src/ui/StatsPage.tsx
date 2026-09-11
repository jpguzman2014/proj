import { useState } from 'react';
import { MODES } from '../modes/registry';
import { allStats, dataReports, difficultyVotes, history } from '../state/stats';
import { clearAll, storageAvailable } from '../state/storage';

function fmtDate(iso: string): string {
  return new Date(iso).toLocaleString();
}

export default function StatsPage() {
  const [version, setVersion] = useState(0);
  const stats = allStats(MODES.map((m) => m.id));
  const runs = history();
  const reports = dataReports();
  const votes = difficultyVotes();
  const available = storageAvailable();

  // Saving a file needs a real browser tab. The preview build hides the control
  // rather than offering one that silently does nothing.
  const exportReports = () => {
    const blob = new Blob([JSON.stringify(reports, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'gaming-guess-reports.json';
    link.click();
    URL.revokeObjectURL(url);
  };

  const wipe = () => {
    if (!window.confirm('Delete all scores, ratings and reports stored in this browser?')) return;
    clearAll();
    setVersion((v) => v + 1);
  };

  return (
    <div className="stack" key={version}>
      <h1 tabIndex={-1}>Your statistics</h1>
      <p className="muted">
        Everything here is stored in this browser only. Clearing your browser data or playing
        on another device starts you from zero.
      </p>

      {!available && (
        <p className="verdict no" role="status">
          This browser is blocking site storage, so scores cannot be saved. The game still plays.
        </p>
      )}

      <section className="card" aria-labelledby="per-mode">
        <h2 id="per-mode">By mode</h2>
        <div style={{ overflowX: 'auto' }}>
          <table className="stats">
            <caption className="visually-hidden">Best score and accuracy for each mode</caption>
            <thead>
              <tr>
                <th scope="col">Mode</th>
                <th scope="col">Runs</th>
                <th scope="col">Best</th>
                <th scope="col">Accuracy</th>
                <th scope="col">Best streak</th>
              </tr>
            </thead>
            <tbody>
              {MODES.map((mode) => {
                const s = stats[mode.id];
                const accuracy = s.totalAnswered
                  ? `${Math.round((s.totalCorrect / s.totalAnswered) * 100)}%`
                  : '—';
                return (
                  <tr key={mode.id}>
                    <th scope="row">
                      <span aria-hidden="true">{mode.icon} </span>
                      {mode.name}
                    </th>
                    <td>{s.played}</td>
                    <td>{s.bestScore}</td>
                    <td>{accuracy}</td>
                    <td>{s.bestStreak}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </section>

      <section className="card" aria-labelledby="recent">
        <h2 id="recent">Recent runs</h2>
        {runs.length === 0 ? (
          <p className="muted">Nothing yet. Play a round and it will show up here.</p>
        ) : (
          <ul className="leaderboard">
            {runs.slice(0, 12).map((run, i) => (
              <li key={`${run.at}-${i}`}>
                <span>
                  {run.modeId} · {fmtDate(run.at)}
                </span>
                <span>
                  {run.score} pts · {run.correct}/{run.answered}
                </span>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="card stack" aria-labelledby="feedback">
        <h2 id="feedback">Your feedback</h2>
        <p className="muted small">
          {votes.length} difficulty rating{votes.length === 1 ? '' : 's'} and {reports.length}{' '}
          data report{reports.length === 1 ? '' : 's'} saved on this device.
        </p>
        {reports.length > 0 && (
          <>
            <ul className="leaderboard">
              {reports.slice(0, 8).map((report, i) => (
                <li key={`${report.at}-${i}`}>
                  <span>
                    {report.reason}
                    {report.note ? ` — ${report.note}` : ''}
                  </span>
                  <span className="muted small">{report.subjectId}</span>
                </li>
              ))}
            </ul>
            {!import.meta.env.VITE_DEMO && (
              <div className="row">
                <button className="btn" type="button" onClick={exportReports}>
                  Export reports as a file
                </button>
              </div>
            )}
          </>
        )}
      </section>

      <section className="card stack" aria-labelledby="data-controls">
        <h2 id="data-controls">Your data</h2>
        <p className="small muted">
          Gaming Guess has no accounts and no analytics. Deleting the data below removes
          everything the app has ever stored about you.
        </p>
        <div className="row">
          <button className="btn" type="button" onClick={wipe}>
            Delete all my data
          </button>
        </div>
      </section>
    </div>
  );
}
