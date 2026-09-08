import { useCallback, useEffect, useRef, useState } from 'react';
import NicknamePicker from './NicknamePicker';
import { randomNickname } from '../../net/nicknames';
import {
  RoomConnection,
  roomServerUrl,
  type RoomPlayer,
  type RoomQuestion,
  type RoundResult,
  type ServerMessage,
  type Status,
} from '../../net/room';

type Phase = 'join' | 'lobby' | 'question' | 'reveal' | 'over';

export default function Player({ initialCode }: { initialCode: string }) {
  const [status, setStatus] = useState<Status>('closed');
  const [error, setError] = useState('');
  const [code, setCode] = useState(initialCode.toUpperCase());
  const [nickname, setNickname] = useState(randomNickname);
  const [phase, setPhase] = useState<Phase>('join');
  const [playerId, setPlayerId] = useState('');
  const [question, setQuestion] = useState<RoomQuestion | null>(null);
  const [chosen, setChosen] = useState<number | null>(null);
  const [reveal, setReveal] = useState<{
    answerIndex: number;
    explanation: string;
    mine: RoundResult | undefined;
    standings: RoomPlayer[];
  } | null>(null);
  const [standings, setStandings] = useState<RoomPlayer[]>([]);
  const [remaining, setRemaining] = useState(0);

  const connection = useRef<RoomConnection | null>(null);
  const playerIdRef = useRef('');

  const handle = useCallback((message: ServerMessage) => {
    switch (message.t) {
      case 'welcome':
        playerIdRef.current = message.playerId;
        setPlayerId(message.playerId);
        setCode(message.code);
        setPhase('lobby');
        setError('');
        break;
      case 'question':
        setQuestion(message);
        setChosen(null);
        setReveal(null);
        setPhase('question');
        break;
      case 'reveal':
        setReveal({
          answerIndex: message.answerIndex,
          explanation: message.explanation,
          mine: message.round.find((entry) => entry.id === playerIdRef.current),
          standings: message.standings,
        });
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

  const join = (event: React.FormEvent) => {
    event.preventDefault();
    const conn = new RoomConnection(handle, setStatus);
    connection.current = conn;
    conn.connect();
    conn.send({ t: 'join', code: code.trim().toUpperCase(), nickname });
  };

  const answer = (index: number) => {
    if (chosen !== null) return;
    setChosen(index);
    connection.current?.send({ t: 'answer', optionIndex: index });
  };

  if (phase === 'join') {
    return (
      <div className="stack">
        <h1 tabIndex={-1}>Join a room</h1>
        <form className="card stack" onSubmit={join}>
          <div>
            <label className="field" htmlFor="join-code">
              Room code
            </label>
            <input
              id="join-code"
              type="text"
              value={code}
              onChange={(event) => setCode(event.target.value.toUpperCase())}
              maxLength={6}
              autoCapitalize="characters"
              autoComplete="off"
              inputMode="text"
              required
              style={{ letterSpacing: '0.3em', fontSize: '1.4rem' }}
            />
          </div>
          <NicknamePicker id="join-name" value={nickname} onChange={setNickname} />
          {error && (
            <p className="verdict no" role="status">
              {error}
            </p>
          )}
          {status === 'error' && (
            <p className="verdict no" role="status">
              Could not reach the room server at {roomServerUrl()}.
            </p>
          )}
          <button className="btn btn-primary btn-block" type="submit">
            Join room
          </button>
        </form>
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
        <div className="card stack">
          <h1 tabIndex={-1}>You are in room {code}</h1>
          <p className="muted">
            Keep this page open. The questions appear here when the host starts the game.
          </p>
        </div>
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
          </div>
          <div className="card stack">
            <h2 style={{ fontSize: '1.05rem' }}>{question.prompt}</h2>
            <ul className="answers">
              {question.options.map((option, i) => (
                <li key={option}>
                  <button
                    className={`btn answer btn-block${chosen === i ? ' correct' : ''}`}
                    type="button"
                    onClick={() => answer(i)}
                    disabled={chosen !== null}
                  >
                    <span aria-hidden="true" style={{ opacity: 0.6, marginRight: '0.5rem' }}>
                      {i + 1}
                    </span>
                    {option}
                  </button>
                </li>
              ))}
            </ul>
            <p aria-live="polite" className="muted small">
              {chosen !== null ? 'Locked in. Waiting for everyone else.' : 'Pick an answer.'}
            </p>
          </div>
        </>
      )}

      {phase === 'reveal' && reveal && question && (
        <div className="card stack">
          <h1 tabIndex={-1}>{reveal.mine?.correct ? 'Correct' : 'Not this time'}</h1>
          <p className={`verdict ${reveal.mine?.correct ? 'ok' : 'no'}`}>
            <strong>{question.options[reveal.answerIndex]}</strong> — {reveal.explanation}
          </p>
          <p>
            <strong>+{reveal.mine?.delta ?? 0}</strong> points this round.
          </p>
          <ul className="leaderboard">
            {reveal.standings
              .filter((p) => !p.isHost)
              .map((player, i) => (
                <li key={player.id} className={player.id === playerId ? 'me' : ''}>
                  <span>
                    {i + 1}. {player.name}
                  </span>
                  <span>{player.score}</span>
                </li>
              ))}
          </ul>
        </div>
      )}

      {phase === 'over' && (
        <div className="card stack">
          <h1 tabIndex={-1}>Final scores</h1>
          <ul className="leaderboard">
            {standings
              .filter((p) => !p.isHost)
              .map((player, i) => (
                <li key={player.id} className={player.id === playerId ? 'me' : ''}>
                  <span>
                    {i + 1}. {player.name}
                  </span>
                  <span>{player.score.toLocaleString()}</span>
                </li>
              ))}
          </ul>
          <a className="btn btn-primary" href="#/">
            Play solo
          </a>
        </div>
      )}
    </div>
  );
}
