import { db } from '../data/db';
import { MODES } from '../modes/registry';
import { todayKey } from '../engine/rng';
import { dailyResult } from '../state/stats';
import { navigate } from '../router';

function ModeButton({
  icon,
  name,
  tagline,
  onClick,
}: {
  icon: string;
  name: string;
  tagline: string;
  onClick: () => void;
}) {
  return (
    <li>
      <button className="mode-card" type="button" onClick={onClick}>
        <span className="icon" aria-hidden="true">
          {icon}
        </span>
        <span className="name">{name}</span>
        <span className="tagline">{tagline}</span>
      </button>
    </li>
  );
}

export default function Home() {
  const primary = MODES.filter((mode) => mode.primary);
  const more = MODES.filter((mode) => !mode.primary);
  const today = todayKey();
  const daily = dailyResult(today);

  return (
    <div className="stack">
      <div>
        <h1 tabIndex={-1}>How well do you really know video games?</h1>
        <p className="muted">
          Pick a mode and play. No account, no email, nothing to install. Your scores stay on
          this device.
        </p>
      </div>

      <h2 id="modes-heading">Modes</h2>
      <ul className="mode-grid" aria-labelledby="modes-heading">
        {primary.map((mode) => (
          <ModeButton
            key={mode.id}
            icon={mode.icon}
            name={mode.name}
            tagline={mode.tagline}
            onClick={() => navigate(`/setup/${mode.id}`)}
          />
        ))}
        <ModeButton
          icon="👥"
          name="Play with Friends"
          tagline="Scan a QR code, everyone joins"
          onClick={() => navigate('/host')}
        />
      </ul>

      <h2 id="more-heading">More ways to play</h2>
      <ul className="mode-grid" aria-labelledby="more-heading">
        {more.map((mode) => (
          <ModeButton
            key={mode.id}
            icon={mode.icon}
            name={mode.name}
            tagline={
              mode.id === 'daily' && daily
                ? `Done today — ${daily.score} points`
                : mode.tagline
            }
            onClick={() => navigate(`/setup/${mode.id}`)}
          />
        ))}
      </ul>

      <section className="card stack" aria-labelledby="db-heading">
        <h2 id="db-heading">What is in the database</h2>
        <p className="muted small">
          One entity graph feeds every mode. A game connects to its characters, weapons, items,
          enemies, bosses, vehicles and locations, so a single entry can produce many different
          questions.
        </p>
        <table className="stats">
          <caption className="visually-hidden">Entity counts by category</caption>
          <thead>
            <tr>
              <th scope="col">Category</th>
              <th scope="col">Entries</th>
            </tr>
          </thead>
          <tbody>
            {[...db.byKind.entries()]
              .sort((a, b) => b[1].length - a[1].length)
              .map(([kind, entities]) => (
                <tr key={kind}>
                  <th scope="row">{kind.charAt(0).toUpperCase() + kind.slice(1)}</th>
                  <td>{entities.length}</td>
                </tr>
              ))}
          </tbody>
        </table>
        <p className="small muted">
          Spotted something wrong? Every question has a “Report incorrect information” button.
        </p>
      </section>
    </div>
  );
}
