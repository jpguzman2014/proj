import { db } from '../data/db';
import { listProviders, activeProvider } from '../media/provider';

export default function About() {
  const provider = activeProvider();

  return (
    <article className="legal stack">
      <h1 tabIndex={-1}>About and data sources</h1>

      <h2>What it is</h2>
      <p>
        A gaming guessing and trivia platform: one entity graph of {db.games.length} games and{' '}
        {db.subEntities.length} connected characters, weapons, items, enemies, bosses, vehicles
        and locations, feeding every mode. No login, no daily limit on practice, and a party
        mode where everyone joins by scanning a QR code.
      </p>

      <h2>Where the images come from</h2>
      <p>
        Game screenshots, cover art and character renders are owned by their publishers, and
        being able to find one online does not make it redistributable. So this app ships none
        of them.
      </p>
      <p>
        Instead, media sits behind a provider interface that is separate from the metadata.
        The active provider is <strong>{provider.name}</strong>: {provider.licenseSummary}
      </p>
      {listProviders().length > 1 && (
        <p className="small muted">
          Registered providers: {listProviders().map((p) => p.name).join(', ')}.
        </p>
      )}
      <p>
        A licensed source can be added later by registering another provider. Any such source
        needs terms that permit the use, and per-image credit and licence text, which the
        interface already carries.
      </p>

      <h2>Where the facts come from</h2>
      <p>
        The catalogue is compiled by hand from widely published release information. Original
        release platforms are recorded separately from later ports, so a question about where
        a game launched is not answered with a port. Genres are a list rather than a single
        label, because forcing one genre per game is where competitors get accused of being
        wrong.
      </p>
      <p>
        Mistakes are expected. Every question has a report button, and reports are kept on
        your device where you can review and export them.
      </p>

      <h2>Fairness</h2>
      <p>
        Every question has a clue ladder that ends with something decisive, so no question is
        pure luck. Opening clues costs points rather than being forbidden. Wrong answers are
        drawn from the same category and, where possible, the same era, so the shape of the
        options does not give the answer away.
      </p>

      <h2>Accessibility</h2>
      <p>
        The app is keyboard playable: keys 1 to 4 answer, C opens a clue, Enter moves on.
        Every control has a visible focus ring, text meets WCAG 2.2 AA contrast, and the clue
        art carries a text description. If something here does not work with your assistive
        technology, that is a bug worth reporting.
      </p>
    </article>
  );
}
