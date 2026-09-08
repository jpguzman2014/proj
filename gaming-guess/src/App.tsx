import Home from './ui/Home';
import ModeSetup from './ui/ModeSetup';
import Run from './ui/Run';
import StatsPage from './ui/StatsPage';
import About from './ui/About';
import { Cookies, Privacy, Refunds, Terms } from './ui/Legal';
import Host from './ui/party/Host';
import Player from './ui/party/Player';
import { navigate, segments, useRoute } from './router';

function Footer() {
  return (
    <footer className="site">
      <nav aria-label="Site information">
        <a href="#/about">About and data sources</a>
        <a href="#/privacy">Privacy</a>
        <a href="#/cookies">Cookies</a>
        <a href="#/refunds">Refunds and availability</a>
        <a href="#/terms">Terms</a>
      </nav>
      <p>
        Gaming Guess is an independent fan-made quiz. Game titles, characters and trademarks
        belong to their respective owners and are named here for identification only.
      </p>
    </footer>
  );
}

function NotFound() {
  return (
    <div className="card stack">
      <h1 tabIndex={-1}>Page not found</h1>
      <p className="muted">That link does not go anywhere in this app.</p>
      <button className="btn btn-primary" type="button" onClick={() => navigate('/')}>
        Back to the modes
      </button>
    </div>
  );
}

function screenFor(path: string) {
  const parts = segments(path);
  if (parts.length === 0) return <Home />;
  switch (parts[0]) {
    case 'setup':
      return parts[1] ? <ModeSetup modeId={parts[1]} /> : <NotFound />;
    case 'play':
      return parts[1] ? <Run modeId={parts[1]} /> : <NotFound />;
    case 'stats':
      return <StatsPage />;
    case 'host':
      return <Host />;
    case 'join':
      return <Player initialCode={parts[1] ?? ''} />;
    case 'about':
      return <About />;
    case 'privacy':
      return <Privacy />;
    case 'cookies':
      return <Cookies />;
    case 'terms':
      return <Terms />;
    case 'refunds':
      return <Refunds />;
    default:
      return <NotFound />;
  }
}

export default function App() {
  const path = useRoute();
  const inRun = path.startsWith('/play');

  return (
    <>
      <a className="skip-link" href="#main">
        Skip to main content
      </a>
      <header className="topbar">
        <a className="brand" href="#/">
          <span aria-hidden="true">🎮</span> GAMING GUESS
        </a>
        {!inRun && (
          <nav className="row" aria-label="Main">
            <a className="btn btn-ghost" href="#/stats">
              <span className="wide-only">Statistics</span>
              <span className="narrow-only">Stats</span>
            </a>
            <a className="btn btn-ghost" href="#/host">
              <span className="wide-only">Play with friends</span>
              <span className="narrow-only">Friends</span>
            </a>
          </nav>
        )}
      </header>
      <main className="shell" id="main">
        {screenFor(path)}
      </main>
      {!inRun && <Footer />}
    </>
  );
}
