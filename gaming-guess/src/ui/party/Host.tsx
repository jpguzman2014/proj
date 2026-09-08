import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import ClueArt from '../ClueArt';
import QrCode from '../QrCode';
import Filters from '../Filters';
import NicknamePicker from './NicknamePicker';
import { generateQuestions } from '../../engine/engine';
import { EMPTY_FILTERS, countActive, type Filters as FilterState } from '../../engine/filters';
import { PARTY_MODE, typeIdsForMode } from '../../modes/registry';
import {
  RoomConnection,
  joinUrl,
  roomServerUrl,
  type RoomPlayer,
  type RoomQuestion,
  type RoundResult,
  type ServerMessage,
  type Status,
} from '../../net/room';
import { randomNickname } from '../../net/nicknames';
import { read, write } from '../../state/storage';

type Phase = 'setup' | 'lobby' | 'question' | 'reveal' | 'over';

export default function Host() {
  const [status, setStatus] = useState<Status>('closed');
  const [error, setError] = useState('');
  const [code, setCode] = useState('');
  const [phase, setPhase] = useState<Phase>('setup');
  const [players, setPlayers] = useState<RoomPlayer[]>([]);
  const [question, setQuestion] = useState<RoomQuestion | null>(null);
  const [answered, setAnswered] = useState(0);
  const [reveal, setReveal] = useState<{
    answerIndex: number;
    explanation: string;
    round: RoundResult[];
    last: boolean;
  } | null>(null);
  const [standings, setStandings] = useState<RoomPlayer[]>([]);
  const [remaining, setRemaining] = useState(0);

  const [nickname, setNickname] = useState(randomNickname);
  const [questionCount, setQuestionCount] = useState(15);
  const [seconds, setSeconds] = useState(20);
  const [filters, setFilters] = useState<FilterState>(() =>
    read<FilterState>('filters', EMPTY_FILTERS),
  );
  const [showFilters, setShowFilters] = useState(false);

  const connection = useRef<RoomConnection | null>(null);

  const handle = useCallback((message: ServerMessage) => {
    switch (message.t) {
      case 'welcome':
        setCode(message.code);
        setPhase('lobby');
        setError('');
        break;
      case 'room':
        setPlayers(message.players);
        break;
      case 'question':
        setQuestion(message);
        setReveal(null);
        setAnswered(0);
        setPhase('question');
        break;
      case 'answered':
        setAnswered(message.answered);
        break;
      case 'reveal':
        setReveal({
          answerIndex: message.answerIndex,
          explanation: message.explanation,
          round: message.round,
          last: message.last,
        });
        setStandings(message.standings);
        setPhase('reveal');
        break;
      case 'over':
        setStandings(message.standings);
        setPhase('over');
        break;
      case 'error':
        setError(message.message);
        break;
      default:
        break;
    }
  }, []);

  useEffect(() => () => connection.current?.close(), []);

  useEffect(() => {
    if (phase !== 'question' || !question) return;
    const tick = () => setRemaining(Math.max(0, Math.ceil((question.endsAt - Date.now()) / 1000)));
    tick();
    const id = window.setInterval(tick, 250);
    return () => window.clearInterval(id);
  }, [phase, question]);

  const createRoom = () => {
    const conn = new RoomConnection(handle, setStatus);
    connection.current = conn;
    conn.connect();
    conn.send({ t: 'create', nickname });
  };

  const questions = useMemo(
    () =>
      generateQuestions({
        filters,
        typeIds: typeIdsForMode(PARTY_MODE),
        count: questionCount,
        seed: `party:${code || 'preview'}:${questionCount}`,
      }),
    [filters, questionCount, code],
  );

  const startGame = () => {
    write('filters', filters);
    connection.current?.send({
      t: 'start',
      perQuestionSeconds: seconds,
      questions: questions.map((q) => ({
        prompt: q.prompt,
        options: q.options,
        answerIndex: q.answerIndex,
        typeLabel: q.typeLabel,
        difficulty: q.difficulty,
        artEntityId: q.artEntityId,
        clues: q.clues.slice(0, PARTY_MODE.rules.maxClues),
        explanation: q.explanation,
      })),
    });
  };

  const connected = players.filter((p) => p.connected && !p.isHost);

  if (phase === 'setup') {
    return (
      <div className="stack">
        <h1 tabIndex={-1}>Play with friends</h1>
        <p className="muted">
          Put this screen on a TV or laptop. Everyone else scans the QR code with their phone
          and plays from there. No app, no account.
        </p>

        <div className="card stack">
          <NicknamePicker id="host-name" value={nickname} onChange={setNickname} />

          <div className="row">
            <div style={{ flex: '1 1 160px' }}>
              <label className="field" htmlFor="host-count">
                Questions
              </label>
              <input
                id="host-count"
                type="number"
                min={5}
                max={50}
                value={questionCount}
                onChange={(event) => setQuestionCount(Number(event.target.value) || 15)}
              />
            </div>
            <div style={{ flex: '1 1 160px' }}>
              <label className="field" htmlFor="host-seconds">
                Seconds per question
              </label>
              <input
                id="host-seconds"
                type="number"
                min={5}
                max={60}
                value={seconds}
                onChange={(event) => setSeconds(Number(event.target.value) || 20)}
              />
            </div>
          </div>

          <button
            className="btn"
            type="button"
            aria-expanded={showFilters}
            aria-controls="party-filters"
            onClick={() => setShowFilters((open) => !open)}
          >
            {showFilters ? 'Hide categories' : 'Categories and filters'}
            {countActive(filters) > 0 && ` (${countActive(filters)})`}
          </button>
          <div id="party-filters" hidden={!showFilters}>
            <Filters value={filters} onChange={setFilters} />
          </div>

          <p className="small muted">
            {questions.length} questions ready from the current filters.
          </p>

          {import.meta.env.VITE_DEMO ? (
            <p className="verdict no">
              <strong>Not available in this preview.</strong> Party mode needs the room
              server running, and a single-file preview has nowhere to run it. Everything
              else on this page works, and the solo modes are fully playable.
            </p>
          ) : (
            <button className="btn btn-primary btn-block" type="button" onClick={createRoom}>
              Create room
            </button>
          )}
          {status === 'error' && (
            <p className="verdict no" role="status">
              Could not reach the room server at {roomServerUrl()}. Start it with{' '}
              <code>npm run server</code>, or set <code>VITE_ROOM_SERVER</code>.
            </p>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="stack">
      {error && (
        <p className="verdict no" role="status">
          {error}
        </p>
      )}

      {phase === 'lobby' && (
        <>
          <h1 tabIndex={-1}>Room {code}</h1>
          <div className="card qr-panel">
            <QrCode value={joinUrl(code)} label={`QR code to join room ${code}`} />
            <div className="stack">
              <p className="room-code" aria-label={`Room code ${code.split('').join(' ')}`}>
                {code}
              </p>
              <p className="muted">
                Scan the code, or open this address and enter the code:
                <br />
                <code>{joinUrl(code)}</code>
              </p>
              <h2>Players ({connected.length})</h2>
              {connected.length === 0 ? (
                <p className="muted">Waiting for the first player to join.</p>
              ) : (
                <ul className="leaderboard">
                  {connected.map((player) => (
                    <li key={player.id}>
                      <span>{player.name}</span>
                      <span className="muted small">ready</span>
                    </li>
                  ))}
                </ul>
              )}
              <button
                className="btn btn-primary"
                type="button"
                onClick={startGame}
                disabled={connected.length === 0 || questions.length === 0}
              >
                Start game
              </button>
            </div>
          </div>
        </>
      )}

      {phase === 'question' && question && (
        <>
          <h1 className="visually-hidden" tabIndex={-1}>
            Question {question.index + 1}
          </h1>
          <div className="hud">
            <span className="pill">
              Q {question.index + 1} / {question.total}
            </span>
            <span className="pill">{remaining}s</span>
            <span className="pill">
              {answered} of {connected.length} answered
            </span>
            <span className="pill">Room {code}</span>
          </div>
          <div className="card stack">
            <h2>{question.prompt}</h2>
            {question.artEntityId && (
              <ClueArt entityId={question.artEntityId} reveal={1} maxReveal={2} />
            )}
            <ul className="answers">
              {question.options.map((option, i) => (
                <li key={option}>
                  <span className="btn answer btn-block" aria-hidden="false">
                    <strong style={{ marginRight: '0.5rem' }}>{i + 1}</strong>
                    {option}
                  </span>
                </li>
              ))}
            </ul>
            <p className="muted small">Answer on your phone.</p>
          </div>
        </>
      )}

      {phase === 'reveal' && reveal && question && (
        <>
          <h1 tabIndex={-1}>Round {question.index + 1}</h1>
          <div className="card stack">
            <p className="verdict ok">
              <strong>{question.options[reveal.answerIndex]}</strong> — {reveal.explanation}
            </p>
            <ul className="leaderboard">
              {reveal.round.map((entry) => (
                <li key={entry.id}>
                  <span>
                    {entry.name} {entry.correct ? '✓' : entry.answered ? '✗' : '—'}
                  </span>
                  <span>+{entry.delta}</span>
                </li>
              ))}
            </ul>
            <h2>Standings</h2>
            <ul className="leaderboard">
              {standings
                .filter((p) => !p.isHost)
                .map((player, i) => (
                  <li key={player.id}>
                    <span>
                      {i + 1}. {player.name}
                    </span>
                    <span>{player.score}</span>
                  </li>
                ))}
            </ul>
            <button
              className="btn btn-primary"
              type="button"
              onClick={() => connection.current?.send({ t: 'next' })}
            >
              {reveal.last ? 'Show final results' : 'Next question'}
            </button>
          </div>
        </>
      )}

      {phase === 'over' && (
        <>
          <h1 tabIndex={-1}>🏆 Gaming champion</h1>
          <div className="card stack">
            <ul className="leaderboard">
              {standings
                .filter((p) => !p.isHost)
                .map((player, i) => (
                  <li key={player.id} className={i === 0 ? 'me' : ''}>
                    <span>
                      {i + 1}. {player.name}
                    </span>
                    <span>{player.score.toLocaleString()}</span>
                  </li>
                ))}
            </ul>
            <a className="btn btn-primary" href="#/">
              Back to the modes
            </a>
          </div>
        </>
      )}
    </div>
  );
}
