import { useMemo } from 'react';
import { db, gameOf } from '../data/db';
import { getMedia } from '../media/provider';

/**
 * Renders the clue image for a question.
 *
 * Two shapes come out of the media layer. A real screenshot is cropped and
 * blurred back to a full frame as clues are opened, the way a picture quiz
 * should work. Generated art is inline SVG produced by this app, so injecting
 * it as markup is safe: no third-party string reaches this component.
 *
 * Credit and licence are rendered from the asset, not from anything the caller
 * passes in, so an image cannot appear without them.
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
    const steps = Math.max(1, maxReveal);
    const t = Math.min(1, reveal / steps);
    const scale = 3.2 - 2.2 * t;
    const blur = 5.5 * (1 - t) ** 1.5;
    return (
      <figure style={{ margin: 0 }}>
        <div className="art-frame art-frame--photo">
          <img
            src={asset.url}
            alt={asset.alt}
            loading="eager"
            decoding="async"
            style={{
              transform: `scale(${scale})`,
              filter: blur > 0.05 ? `blur(${blur}px)` : 'none',
            }}
          />
        </div>
        <figcaption className="small muted media-credit">
          {asset.sourceUrl ? (
            <a href={asset.sourceUrl} target="_blank" rel="noreferrer noopener">
              {asset.credit}
            </a>
          ) : (
            asset.credit
          )}
          {' · '}
          {asset.licenseUrl ? (
            <a href={asset.licenseUrl} target="_blank" rel="noreferrer noopener">
              {asset.license}
            </a>
          ) : (
            asset.license
          )}
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
