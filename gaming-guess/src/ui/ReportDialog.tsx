import { useEffect, useRef, useState } from 'react';
import type { Question } from '../engine/engine';
import { reportData } from '../state/stats';

const REASONS = [
  'The answer is wrong',
  'The platform information is wrong',
  'The genre or tags are wrong',
  'The release year is wrong',
  'The question is unanswerable',
  'Something else',
];

/**
 * "Report incorrect information", answering the loudest complaint about every
 * competitor. Reports are stored on this device and exported by the player from
 * the statistics page — nothing is transmitted anywhere.
 */
export default function ReportDialog({
  question,
  open,
  onClose,
}: {
  question: Question;
  open: boolean;
  onClose: () => void;
}) {
  const ref = useRef<HTMLDialogElement>(null);
  const [reason, setReason] = useState(REASONS[0]);
  const [note, setNote] = useState('');
  const [sent, setSent] = useState(false);

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    if (!open && dialog.open) dialog.close();
  }, [open]);

  const submit = (event: React.FormEvent) => {
    event.preventDefault();
    reportData({
      questionId: question.id,
      subjectId: question.subjectId,
      reason,
      note: note.trim(),
    });
    setSent(true);
    setNote('');
  };

  return (
    <dialog className="sheet" ref={ref} onClose={onClose} aria-labelledby="report-title">
      <h2 id="report-title">Report incorrect information</h2>
      <p className="small muted">
        About: {question.prompt} — answer given as “{question.options[question.answerIndex]}”.
      </p>
      {sent ? (
        <div className="stack">
          <p className="verdict ok" role="status">
            Saved on this device. You can review or export reports from the statistics page.
          </p>
          <button className="btn btn-primary" type="button" onClick={onClose}>
            Close
          </button>
        </div>
      ) : (
        <form className="stack" onSubmit={submit}>
          <div>
            <label className="field" htmlFor="report-reason">
              What is wrong?
            </label>
            <select
              id="report-reason"
              value={reason}
              onChange={(event) => setReason(event.target.value)}
            >
              {REASONS.map((r) => (
                <option key={r}>{r}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="field" htmlFor="report-note">
              Details (optional)
            </label>
            <textarea
              id="report-note"
              rows={3}
              value={note}
              onChange={(event) => setNote(event.target.value)}
              placeholder="What should it say instead?"
            />
            <p className="small muted" style={{ marginTop: '0.35rem' }}>
              Please do not include personal information. Reports stay in this browser.
            </p>
          </div>
          <div className="row">
            <button className="btn btn-primary" type="submit">
              Save report
            </button>
            <button className="btn" type="button" onClick={onClose}>
              Cancel
            </button>
          </div>
        </form>
      )}
    </dialog>
  );
}
