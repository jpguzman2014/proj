import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import ClueArt from './ClueArt';
import ReportDialog from './ReportDialog';
import Results from './Results';
import { generateQuestions, type Question } from '../engine/engine';
import { EMPTY_FILTERS, type Filters as FilterState } from '../engine/filters';
import { scoreAnswer, summarise, type AnswerRecord } from '../engine/scoring';
import { todayKey } from '../engine/rng';
import { MODE_BY_ID, typeIdsForMode } from '../modes/registry';
import { read } from '../state/storage';
import { markDailyDone, rateDifficulty, recordRun } from '../state/stats';
import { navigate } from '../router';

/** A fast answer is one under this many milliseconds. Used for the speed bonus. */
const SPEED_WINDOW_MS = 15000;
const BATCH = 12;

type Phase = 'playing' | 'answered' | 'finished';

export default function Run({ modeId }: { modeId: string }) {
  const mode = MODE_BY_ID.get(modeId);
  const filters = read<FilterState>('filters', EMPTY_FILTERS);

  const [runKey, setRunKey] = useState(0);
  const seed = useMemo(() => {
    if (!mode) return 'none';
    return mode.rules.dailySeed
      ? `daily:${todayKey()}`
      : `${mode.id}:${Date.now()}:${runKey}`;
  }, [mode, runKey]);

  const [questions, setQuestions] = useState<Question[]>([]);
  const [index, setIndex] = useState(0);
  const [phase, setPhase] = useState<Phase>('playing');
  const [chosen, setChosen] = useState<number | null>(null);
  const [cluesOpen, setCluesOpen] = useState(0);
  const [records, setRecords] = useState<AnswerRecord[]>([]);
  const [streak, setStreak] = useState(0);
  const [bestStreak, setBestStreak] = useState(0);
  const [mistakes, setMistakes] = useState(0);
  const [remainingMs, setRemainingMs] = useState<number | null>(null);
  const [reportOpen, setReportOpen] = useState(false);
  const [rated, setRated] = useState(false);
  const [saved, setSaved] = useState<{ previousBest: number; isPersonalBest: boolean } | null>(
    null,
  );

  const shownAt = useRef<number>(Date.now());
  const liveRef = useRef<HTMLParagraphElement>(null);

  const question = questions[index];
  const maxClues = mode?.rules.maxClues ?? 4;
  const totalClues = question ? Math.min(question.clues.length, maxClues) : 0;

  /* ---- build the run ---- */
  useEffect(() => {
    if (!mode) return;
    const count = mode.rules.questionCount ?? BATCH;
    setQuestions(
      generateQuestions({
        filters,
        typeIds: typeIdsForMode(mode),
        count,
        seed,
      }),
    );
    setIndex(0);
    setRecords([]);
    setPhase('playing');
    setChosen(null);
    setCluesOpen(0);
    setStreak(0);
    setBestStreak(0);
    setMistakes(0);
    setSaved(null);
    setRemainingMs(mode.rules.totalSeconds ? mode.rules.totalSeconds * 1000 : null);
    shownAt.current = Date.now();
    // filters are read once per run on purpose: changing them mid-run would be unfair
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [modeId, seed]);

  const finish = useCallback(() => {
    setPhase('finished');
  }, []);

  /* ---- run clock ---- */
  useEffect(() => {
    if (remainingMs === null || phase === 'finished') return;
    const id = window.setInterval(() => {
      setRemainingMs((current) => {
        if (current === null) return current;
        const next = current - 200;
        return next <= 0 ? 0 : next;
      });
    }, 200);
    return () => window.clearInterval(id);
  }, [remainingMs === null, phase]);

  useEffect(() => {
    if (remainingMs === 0 && phase !== 'finished') finish();
  }, [remainingMs, phase, finish]);

  /* ---- persist the result once ---- */
  useEffect(() => {
    if (phase !== 'finished' || saved || !mode) return;
    const summary = summarise(records);
    const result = recordRun(mode.id, summary, bestStreak);
    if (mode.rules.dailySeed) markDailyDone(todayKey(), summary.score);
    setSaved(result);
  }, [phase, saved, mode, records, bestStreak]);

  const advance = useCallback(() => {
    if (!mode) return;
    const next = index + 1;
    const fixedLength = mode.rules.questionCount;

    if (fixedLength !== undefined && next >= fixedLength) {
      finish();
      return;
    }
    if (next >= questions.length) {
      // Endless and timed modes top the pool up rather than ending abruptly.
      const more = generateQuestions({
        filters,
        typeIds: typeIdsForMode(mode),
        count: BATCH,
        seed: `${seed}:${next}`,
        exclude: new Set(questions.map((q) => q.id)),
      });
      if (!more.length) {
        finish();
        return;
      }
      setQuestions((current) => [...current, ...more]);
    }
    setIndex(next);
    setPhase('playing');
    setChosen(null);
    setCluesOpen(0);
    setRated(false);
    shownAt.current = Date.now();
  }, [index, questions, mode, seed, filters, finish]);

  const answer = useCallback(
    (optionIndex: number | null) => {
      if (!mode || !question || phase !== 'playing') return;
      const ms = Date.now() - shownAt.current;
      const skipped = optionIndex === null;
      const correct = !skipped && optionIndex === question.answerIndex;

      const breakdown = skipped
        ? { total: 0 }
        : scoreAnswer({
            correct,
            cluesUsed: cluesOpen,
            difficulty: question.difficulty,
            speedFraction: Math.max(0, 1 - ms / SPEED_WINDOW_MS),
            speedBonusEnabled: mode.rules.speedBonus,
            wrongPenalty: mode.rules.wrongPenalty,
          });

      setRecords((current) => [
        ...current,
        {
          questionId: question.id,
          correct,
          skipped,
          ms,
          cluesUsed: cluesOpen,
          points: breakdown.total,
        },
      ]);
      setChosen(optionIndex);
      setPhase('answered');

      if (correct) {
        setStreak((s) => {
          const next = s + 1;
          setBestStreak((best) => Math.max(best, next));
          return next;
        });
      } else {
        setStreak(0);
        if (!skipped) {
          setMistakes((m) => {
            const next = m + 1;
            if (mode.rules.lives !== undefined && next >= mode.rules.lives) {
              window.setTimeout(finish, 1200);
            }
            return next;
          });
        }
      }

      if (mode.rules.totalSeconds) window.setTimeout(advance, 1300);
    },
    [mode, question, phase, cluesOpen, advance, finish],
  );

  const openClue = useCallback(() => {
    if (!mode || phase !== 'playing' || cluesOpen >= totalClues) return;
    setCluesOpen((n) => n + 1);
    if (mode.rules.clueTimeCost > 0) {
      setRemainingMs((current) =>
        current === null ? current : Math.max(0, current - mode.rules.clueTimeCost * 1000),
      );
    }
  }, [mode, phase, cluesOpen, totalClues]);

  /* ---- keyboard: 1-4 to answer, C for a clue, Enter to continue ---- */
  useEffect(() => {
    const onKey = (event: KeyboardEvent) => {
      const target = event.target as HTMLElement | null;
      if (target && ['INPUT', 'TEXTAREA', 'SELECT'].includes(target.tagName)) return;
      if (phase === 'playing' && /^[1-4]$/.test(event.key)) {
        answer(Number(event.key) - 1);
      } else if (phase === 'playing' && event.key.toLowerCase() === 'c') {
        openClue();
      } else if (phase === 'answered' && event.key === 'Enter') {
        advance();
      }
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [phase, answer, openClue, advance]);

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

  if (phase === 'finished') {
    return (
      <Results
        mode={mode}
        summary={summarise(records)}
        bestStreak={bestStreak}
        previousBest={saved?.previousBest ?? 0}
        isPersonalBest={saved?.isPersonalBest ?? false}
        onPlayAgain={() => setRunKey((k) => k + 1)}
      />
    );
  }

  if (!question) {
    return (
      <div className="card stack">
        <h1 tabIndex={-1}>No questions match those filters</h1>
        <p className="muted">Widen the filters and try again.</p>
        <button className="btn btn-primary" type="button" onClick={() => navigate(`/setup/${mode.id}`)}>
          Back to filters
        </button>
      </div>
    );
  }

  const score = records.reduce((sum, r) => sum + r.points, 0);
  const total = mode.rules.questionCount;
  const timeLeft = remainingMs === null ? null : Math.ceil(remainingMs / 1000);
  const timeFraction =
    remainingMs === null || !mode.rules.totalSeconds
      ? 1
      : remainingMs / (mode.rules.totalSeconds * 1000);

  return (
    <div className="stack">
      <h1 className="visually-hidden" tabIndex={-1}>
        {mode.name}, question {index + 1}
      </h1>

      <div className="hud">
        <span className="pill">
          {total ? `Q ${index + 1} / ${total}` : `Q ${index + 1}`}
        </span>
        <span className="pill">Score {score}</span>
        {streak > 1 && <span className="pill">Streak {streak}</span>}
        {mode.rules.lives !== undefined && (
          <span className={`pill${mistakes >= mode.rules.lives - 1 ? ' danger' : ''}`}>
            Lives {Math.max(0, mode.rules.lives - mistakes)}
          </span>
        )}
        {timeLeft !== null && <span className="pill">{timeLeft}s</span>}
        <button className="btn btn-ghost" type="button" onClick={() => navigate('/')}>
          Quit
        </button>
      </div>

      {timeLeft !== null && (
        <div
          className="timer-track"
          role="progressbar"
          aria-label="Time remaining"
          aria-valuemin={0}
          aria-valuemax={mode.rules.totalSeconds ?? 0}
          aria-valuenow={timeLeft}
        >
          <div
            className={`timer-fill${timeFraction < 0.25 ? ' low' : ''}`}
            style={{ width: `${Math.max(0, timeFraction) * 100}%` }}
          />
        </div>
      )}

      <div className="card stack">
        <p className="small muted" style={{ margin: 0 }}>
          {question.typeLabel} · {question.difficulty}
        </p>
        <h2>{question.prompt}</h2>

        {question.artEntityId && (
          <ClueArt
            entityId={question.artEntityId}
            reveal={cluesOpen}
            maxReveal={totalClues}
          />
        )}

        {cluesOpen > 0 && (
          <ul className="clue-list">
            {question.clues.slice(0, cluesOpen).map((clue) => (
              <li className="clue" key={clue.label + clue.text}>
                <span className="label">{clue.label}</span>
                <span>{clue.text}</span>
              </li>
            ))}
          </ul>
        )}

        <div className="row">
          <button
            className="btn"
            type="button"
            onClick={openClue}
            disabled={phase !== 'playing' || cluesOpen >= totalClues}
          >
            Reveal a clue ({cluesOpen}/{totalClues})
            {mode.rules.clueTimeCost > 0 && ` · −${mode.rules.clueTimeCost}s`}
          </button>
          {mode.rules.allowSkip && phase === 'playing' && (
            <button className="btn btn-ghost" type="button" onClick={() => answer(null)}>
              Skip
            </button>
          )}
        </div>

        <ul className="answers">
          {question.options.map((option, i) => {
            const isAnswer = i === question.answerIndex;
            const state =
              phase === 'answered' && isAnswer
                ? ' correct'
                : phase === 'answered' && i === chosen
                  ? ' wrong'
                  : '';
            return (
              <li key={option}>
                <button
                  className={`btn answer btn-block${state}`}
                  type="button"
                  onClick={() => answer(i)}
                  disabled={phase !== 'playing'}
                >
                  <span aria-hidden="true" style={{ opacity: 0.6, marginRight: '0.5rem' }}>
                    {i + 1}
                  </span>
                  {option}
                </button>
              </li>
            );
          })}
        </ul>

        <p aria-live="polite" ref={liveRef} className="visually-hidden">
          {phase === 'answered'
            ? chosen === question.answerIndex
              ? `Correct. ${question.explanation}`
              : `Wrong. ${question.explanation}`
            : ''}
        </p>

        {phase === 'answered' && (
          <div className="stack">
            <div
              className={`verdict ${chosen === question.answerIndex ? 'ok' : 'no'}`}
            >
              <strong>
                {chosen === null
                  ? 'Skipped.'
                  : chosen === question.answerIndex
                    ? 'Correct.'
                    : 'Not this time.'}
              </strong>{' '}
              {question.explanation}
            </div>

            {!rated && (
              <fieldset className="filter-group">
                <legend>How hard was that?</legend>
                <p className="small muted" style={{ margin: '0 0 0.3rem' }}>
                  Ratings tune which questions come up. They stay on this device.
                </p>
                <div className="chips">
                  {[1, 2, 3, 4, 5].map((stars) => (
                    <button
                      className="chip"
                      key={stars}
                      type="button"
                      onClick={() => {
                        rateDifficulty(question.id, stars);
                        setRated(true);
                      }}
                    >
                      {stars} {stars === 1 ? 'star' : 'stars'}
                    </button>
                  ))}
                </div>
              </fieldset>
            )}

            <div className="row">
              <button className="btn btn-primary" type="button" onClick={advance}>
                Next question
              </button>
              <button className="btn btn-ghost" type="button" onClick={() => setReportOpen(true)}>
                Report incorrect information
              </button>
            </div>
          </div>
        )}
      </div>

      <ReportDialog
        question={question}
        open={reportOpen}
        onClose={() => setReportOpen(false)}
      />
    </div>
  );
}
