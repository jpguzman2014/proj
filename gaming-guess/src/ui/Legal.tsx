/**
 * Policy pages.
 *
 * The factual parts are accurate for this build: no accounts, no cookies, no
 * analytics, no third-party requests, everything in localStorage. The parts only
 * the operator can supply are marked and must be filled in before launch; a
 * lawyer in the operating jurisdiction signs off the final wording.
 */
const UPDATED = '8 September 2026';

function NeedsOperator({ children }: { children: React.ReactNode }) {
  return (
    <p className="verdict no">
      <strong>Before launch:</strong> {children}
    </p>
  );
}

export function Privacy() {
  return (
    <article className="legal stack">
      <h1 tabIndex={-1}>Privacy policy</h1>
      <p className="muted small">Last updated {UPDATED}</p>

      <h2>The short version</h2>
      <p>
        Gaming Guess does not ask who you are. There is no account, no sign-up, no email
        field and no advertising. The app makes no requests to any third-party server.
      </p>

      <h2>What is stored, and where</h2>
      <p>
        Your scores, statistics, filter choices, difficulty ratings and any data reports you
        write are saved in your own browser using <code>localStorage</code>. They stay on the
        device. They are not transmitted to us, and we cannot read them.
      </p>
      <ul>
        <li>Best scores and run history, per mode</li>
        <li>The filters you last used</li>
        <li>Difficulty ratings you give questions</li>
        <li>Data corrections you report</li>
      </ul>
      <p>
        You can delete all of it at any time from the{' '}
        <a href="#/stats">statistics page</a>, or by clearing site data in your browser.
      </p>

      <h2>Multiplayer rooms</h2>
      <p>
        If you host or join a party room, the nickname you type and your answers are sent to
        the room server so scores can be worked out. Rooms are held in memory only and are
        discarded when the room closes or the server restarts. Do not use your real name as a
        nickname if you would rather other players in the room did not see it.
      </p>

      <h2>Analytics and advertising</h2>
      <p>
        There are none. No analytics scripts, no advertising trackers, no social media
        embeds, no fonts or scripts loaded from other domains.
      </p>

      <h2>Children</h2>
      <p>
        The app collects no personal data, so it does not build profiles of anyone, including
        children.
      </p>

      <h2>How long anything is kept</h2>
      <ul>
        <li>
          Data in your browser: until you delete it, either with the button on the statistics
          page or by clearing site data.
        </li>
        <li>
          Party rooms: held in the room server's memory only, and discarded when the host
          leaves, the room goes quiet for 45 minutes, or the server restarts. Nothing about a
          room is written to disk.
        </li>
        <li>Server request logs: see the hosting section below.</li>
      </ul>

      <h2>Your rights</h2>
      <p>
        Under the UK GDPR, EU GDPR and similar laws you have rights of access, correction and
        erasure over personal data held about you. Because this app holds none on any server,
        the practical route is the delete button on the statistics page.
      </p>
      <p>
        You can also complain to a data protection regulator. In the UK that is the
        Information Commissioner's Office; in the EU it is the supervisory authority for the
        country you live in.
      </p>

      <h2>Who operates this site</h2>
      <NeedsOperator>
        Replace this block with the legal entity name, registered address, contact email and
        company or tax number where the jurisdiction requires it. A contact form alone is not
        enough in the EU or UK.
      </NeedsOperator>

      <h2>Hosting and processors</h2>
      <NeedsOperator>
        Name the static host serving this app and the host running the party room server, say
        what each logs (IP addresses and request lines are normal), for how long, and whether
        either transfers data outside the region the players are in. Naming them is required;
        "we may share data with third parties" is not.
      </NeedsOperator>
    </article>
  );
}

export function Cookies() {
  return (
    <article className="legal stack">
      <h1 tabIndex={-1}>Cookies</h1>
      <p className="muted small">Last updated {UPDATED}</p>
      <p>
        <strong>This app sets no cookies.</strong> It uses browser local storage, which is a
        different mechanism but is covered by the same rules in the EU and UK.
      </p>

      <h2>What local storage is used for</h2>
      <div style={{ overflowX: 'auto' }}>
        <table className="stats">
          <caption className="visually-hidden">Local storage keys and their purpose</caption>
          <thead>
            <tr>
              <th scope="col">Key</th>
              <th scope="col">Purpose</th>
              <th scope="col">Kept for</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <th scope="row">gg:stats:*</th>
              <td>Best score and totals per mode</td>
              <td>Until you delete it</td>
            </tr>
            <tr>
              <th scope="row">gg:history</th>
              <td>Your last 50 runs</td>
              <td>Until you delete it</td>
            </tr>
            <tr>
              <th scope="row">gg:filters</th>
              <td>The filters you last chose</td>
              <td>Until you delete it</td>
            </tr>
            <tr>
              <th scope="row">gg:difficulty-votes</th>
              <td>Difficulty ratings you gave</td>
              <td>Until you delete it</td>
            </tr>
            <tr>
              <th scope="row">gg:reports</th>
              <td>Data corrections you reported</td>
              <td>Until you delete it</td>
            </tr>
            </tbody>
        </table>
      </div>
      <p>
        All of it is strictly necessary to provide the feature you asked for, which is why no
        consent banner is shown. If analytics or advertising are ever added, consent must be
        collected before those scripts load, not after.
      </p>
    </article>
  );
}

export function Terms() {
  return (
    <article className="legal stack">
      <h1 tabIndex={-1}>Terms of use</h1>
      <p className="muted small">Last updated {UPDATED}</p>

      <h2>What this is</h2>
      <p>
        Gaming Guess is a free quiz game. It is provided as it is, with no promise that it is
        available, accurate or uninterrupted.
      </p>

      <h2>Accuracy of the database</h2>
      <p>
        The catalogue is compiled by hand and will contain mistakes. Use the report button on
        any question to flag one. Nothing here should be relied on as a factual reference.
      </p>

      <h2>Trademarks and fair use</h2>
      <p>
        Game titles, character names, studio names and other trademarks are the property of
        their respective owners. They are used here to identify the works being asked about,
        which is nominative use. Gaming Guess is not affiliated with, endorsed by or sponsored
        by any publisher or developer named in it.
      </p>
      <p>
        No screenshots, cover art or character renders are stored or served by this app. All
        images are drawn by the app itself from its own metadata. See{' '}
        <a href="#/about">about and data sources</a>.
      </p>

      <h2>Acceptable use</h2>
      <ul>
        <li>Do not use nicknames in party rooms that harass or impersonate other people.</li>
        <li>Do not attempt to disrupt the room server for other players.</li>
      </ul>

      <h2>Payments and refunds</h2>
      <p>
        Nothing is sold here. There is no payment, no subscription and no in-app purchase, so
        there is nothing to refund and no refund policy to publish.
      </p>
      <p>
        If anything is ever sold, a refund policy has to be published before the first sale,
        stating the window, the conditions, the refund method and timing, and the exclusions.
        In the EU and UK that includes the 14-day right to withdraw and the acknowledgement a
        buyer of digital content has to give before download.
      </p>

      <h2>Liability and governing law</h2>
      <NeedsOperator>
        Replace this block with the operating entity, the governing law and jurisdiction, and
        a liability clause reviewed by a lawyer in that jurisdiction.
      </NeedsOperator>
    </article>
  );
}
