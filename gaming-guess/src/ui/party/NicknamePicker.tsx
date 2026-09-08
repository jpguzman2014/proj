import { useState } from 'react';
import { NICKNAME_COMBINATIONS, nicknameChoices } from '../../net/nicknames';

/**
 * Players choose a name, they never type one. See `src/net/nicknames.ts` for
 * why: a typed name is user-generated content shown to strangers, and that one
 * field would put the whole product inside online safety regimes.
 */
export default function NicknamePicker({
  value,
  onChange,
  id,
}: {
  value: string;
  onChange: (next: string) => void;
  id: string;
}) {
  const [choices, setChoices] = useState<string[]>(() => {
    const initial = nicknameChoices(4);
    return initial.includes(value) ? initial : [value, ...initial.slice(1)];
  });

  const reroll = () => {
    const next = nicknameChoices(4);
    setChoices(next);
    onChange(next[0]);
  };

  return (
    <fieldset className="filter-group" id={id}>
      <legend>Your name in the game</legend>
      <p className="small muted" style={{ margin: '0 0 0.35rem' }}>
        Pick one of these. Names are chosen, not typed, so nothing you write ever
        appears in front of another player. There are {NICKNAME_COMBINATIONS} to go round.
      </p>
      <div className="chips">
        {choices.map((name) => (
          <label className="chip" key={name}>
            <input
              type="radio"
              name={`${id}-nickname`}
              value={name}
              checked={value === name}
              onChange={() => onChange(name)}
            />
            <span>{name}</span>
          </label>
        ))}
      </div>
      <button className="btn" type="button" onClick={reroll} style={{ marginTop: '0.6rem' }}>
        Show different names
      </button>
    </fieldset>
  );
}
