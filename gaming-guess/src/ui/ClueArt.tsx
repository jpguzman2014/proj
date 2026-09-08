import { useMemo } from 'react';
import { db, gameOf } from '../data/db';
import { getMedia } from '../media/provider';

/**
 * Renders the clue image for a question.
 *
 * The SVG is produced by this app's own generator, so injecting it as markup is
 * safe: no third-party string ever reaches this component. The wrapper carries
 * the accessible name because the inline SVG itself is hidden from assistive
 * technology.
 */
export default function ClueArt({
  entityId,
  reveal,
  maxReveal,
}: {
  entityId: string;
  reveal: number;
  maxReveal: number;
}) {
  const asset = useMemo(() => {
    const entity = db.byId.get(entityId);
    const game = entity ? gameOf(entity) : undefined;
    if (!entity || !game) return null;
    return getMedia(entity, game, reveal, maxReveal);
  }, [entityId, reveal, maxReveal]);

  if (!asset) return null;

  if (asset.kind === 'remote-image' && asset.url) {
    return (
      <figure style={{ margin: 0 }}>
        <div className="art-frame">
          <img src={asset.url} alt={asset.alt} width={600} height={600} />
        </div>
        <figcaption className="small muted">
          {asset.credit} · {asset.license}
        </figcaption>
      </figure>
    );
  }

  return (
    <figure style={{ margin: 0 }}>
      <div
        className="art-frame"
        role="img"
        aria-label={asset.alt}
        dangerouslySetInnerHTML={{ __html: asset.svg ?? '' }}
      />
      <figcaption className="small muted">
        Stylised clue art generated from this entry’s data. Not official artwork.
      </figcaption>
    </figure>
  );
}
