import { useMemo, useState } from 'react';
import Filters from './Filters';
import { EMPTY_FILTERS, countActive, type Filters as FilterState } from '../engine/filters';
import { countMatchingGames, estimateAvailable } from '../engine/engine';
import { MODE_BY_ID, typeIdsForMode } from '../modes/registry';
import { read, write } from '../state/storage';
import { statsFor } from '../state/stats';
import { navigate } from '../router';

const FILTER_KEY = 'filters';

export default function ModeSetup({ modeId }: { modeId: string }) {
  const mode = MODE_BY_ID.get(modeId);
  const [filters, setFilters] = useState<FilterState>(() =>
    read<FilterState>(FILTER_KEY, EMPTY_FILTERS),
  );
  const [showFilters, setShowFilters] = useState(false);

  const typeIds = useMemo(() => (mode ? typeIdsForMode(mode) : []), [mode]);
  const available = useMemo(
    () => estimateAvailable(filters, typeIds),
    [filters, typeIds],
  );
  const matchingGames = useMemo(() => countMatchingGames(filters), [filters]);

  if (!mode) {
    return (
      <div className="card stack">
        <h1 tabIndex={-1}>Unknown mode</h1>
        <a className="btn btn-primary" href="#/">
          Back to the modes
        </a>
      </div>
    );
  }

  const stats = statsFor(mode.id);
  const wanted = mode.rules.questionCount ?? 10;
  const tooNarrow = available < Math.min(wanted, 5);

  const start = () => {
    write(FILTER_KEY, filters);
    navigate(`/play/${mode.id}`);
  };

  return (
    <div className="stack">
      <h1 tabIndex={-1}>
        <span aria-hidden="true">{mode.icon} </span>
        {mode.name}
      </h1>
      <p className="muted">{mode.description}</p>

      <div className="card stack">
        <div className="spread">
          <div>
            <strong>{matchingGames}</strong> <span className="muted">games match</span>
            {' · '}
            <strong>{available}</strong> <span className="muted">possible questions</span>
          </div>
          <button
            className="btn"
            type="button"
            aria-expanded={showFilters}
            aria-controls="filter-panel"
            onClick={() => setShowFilters((open) => !open)}
          >
            {showFilters ? 'Hide filters' : 'Filters'}
            {countActive(filters) > 0 && ` (${countActive(filters)})`}
          </button>
        </div>

        {tooNarrow && (
          <p className="verdict no" role="status">
            These filters leave too few questions for a fair run. Clear some of them.
          </p>
        )}

        <div id="filter-panel" hidden={!showFilters}>
          <Filters value={filters} onChange={setFilters} />
          <div className="row" style={{ marginTop: '0.85rem' }}>
            <button className="btn" type="button" onClick={() => setFilters(EMPTY_FILTERS)}>
              Clear all filters
            </button>
          </div>
        </div>

        <button
          className="btn btn-primary btn-block"
          type="button"
          onClick={start}
          disabled={tooNarrow}
        >
          Start {mode.name}
        </button>
      </div>

      <section className="card stack" aria-labelledby="rules-heading">
        <h2 id="rules-heading">How this mode scores</h2>
        <ul className="small">
          <li>
            {mode.rules.questionCount
              ? `${mode.rules.questionCount} questions.`
              : mode.rules.totalSeconds
                ? `${mode.rules.totalSeconds} seconds on the clock.`
                : 'Runs until you are out of lives.'}
          </li>
          <li>Correct answer: 100 points, less for each clue you opened.</li>
          <li>Harder entries are worth up to 60 bonus points.</li>
          {mode.rules.speedBonus && <li>Speed bonus of up to 100 points for a fast answer.</li>}
          {mode.rules.wrongPenalty > 0 && (
            <li>Wrong answer: −{mode.rules.wrongPenalty} points.</li>
          )}
          {mode.rules.clueTimeCost > 0 && (
            <li>Each clue costs {mode.rules.clueTimeCost} seconds of your clock.</li>
          )}
          {mode.rules.lives !== undefined && (
            <li>
              {mode.rules.lives === 1
                ? 'One mistake ends the run.'
                : `${mode.rules.lives} mistakes allowed.`}
            </li>
          )}
        </ul>
        {stats.played > 0 && (
          <p className="small muted">
            Your best here: {stats.bestScore} points over {stats.played} runs.
          </p>
        )}
      </section>
    </div>
  );
}
