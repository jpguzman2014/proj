import { genreName, platformName } from '../data/catalogue';
import { db } from '../data/db';
import {
  usedGenreIds,
  usedPlatformIds,
  usedTags,
  type Filters as FilterState,
} from '../engine/filters';
import { DIFFICULTY_ORDER, type Difficulty, type Era } from '../data/types';

const ERAS: Era[] = ['1980s', '1990s', '2000s', '2010s', '2020s'];

interface ChipGroupProps<T extends string> {
  legend: string;
  hint?: string;
  values: readonly T[];
  labelFor: (value: T) => string;
  selected: T[];
  onChange: (next: T[]) => void;
}

function ChipGroup<T extends string>({
  legend,
  hint,
  values,
  labelFor,
  selected,
  onChange,
}: ChipGroupProps<T>) {
  const toggle = (value: T) => {
    onChange(
      selected.includes(value) ? selected.filter((v) => v !== value) : [...selected, value],
    );
  };
  return (
    <fieldset className="filter-group">
      <legend>{legend}</legend>
      {hint && <p className="small muted" style={{ margin: '0 0 0.2rem' }}>{hint}</p>}
      <div className="chips">
        {values.map((value) => (
          <label className="chip" key={value}>
            <input
              type="checkbox"
              checked={selected.includes(value)}
              onChange={() => toggle(value)}
            />
            <span>{labelFor(value)}</span>
          </label>
        ))}
      </div>
    </fieldset>
  );
}

export default function Filters({
  value,
  onChange,
}: {
  value: FilterState;
  onChange: (next: FilterState) => void;
}) {
  const platforms = usedPlatformIds();
  const genres = usedGenreIds();

  return (
    <div className="stack">
      <ChipGroup
        legend="Platform"
        hint="Includes later ports, so a PS2 filter still finds games first released elsewhere."
        values={platforms}
        labelFor={platformName}
        selected={value.platforms}
        onChange={(platforms_) => onChange({ ...value, platforms: platforms_ })}
      />
      <ChipGroup
        legend="Genre"
        hint="A game keeps every genre that applies to it, not one forced label."
        values={genres}
        labelFor={genreName}
        selected={value.genres}
        onChange={(genres_) => onChange({ ...value, genres: genres_ })}
      />
      <ChipGroup
        legend="Era"
        values={ERAS}
        labelFor={(era) => era}
        selected={value.eras}
        onChange={(eras) => onChange({ ...value, eras })}
      />
      <ChipGroup
        legend="Difficulty"
        values={DIFFICULTY_ORDER}
        labelFor={(d: Difficulty) => d.charAt(0).toUpperCase() + d.slice(1)}
        selected={value.difficulties}
        onChange={(difficulties) => onChange({ ...value, difficulties })}
      />
      <ChipGroup
        legend="Tags"
        values={usedTags()}
        labelFor={(tag) => tag.split('-').join(' ')}
        selected={value.tags}
        onChange={(tags) => onChange({ ...value, tags })}
      />
      <ChipGroup
        legend="Franchise"
        values={db.franchises}
        labelFor={(f) => f}
        selected={value.franchises}
        onChange={(franchises) => onChange({ ...value, franchises })}
      />
    </div>
  );
}
