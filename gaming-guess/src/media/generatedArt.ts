import type { AnyEntity, Game } from '../data/types';
import { hashString, mulberry32, type Rng } from '../engine/rng';
import { registerProvider, type MediaAsset, type MediaProvider } from './provider';

const FALLBACK_PALETTE = ['#3d5afe', '#00e5c0', '#111428'];

interface Ctx {
  rng: Rng;
  palette: string[];
  entity: AnyEntity;
  game: Game;
}

function paletteFor(entity: AnyEntity, game: Game): string[] {
  const raw = entity.art?.palette ?? game.art?.palette ?? FALLBACK_PALETTE;
  return raw.length >= 3 ? raw : [...raw, ...FALLBACK_PALETTE].slice(0, 3);
}

function r(rng: Rng, min: number, max: number): number {
  return min + rng() * (max - min);
}

function n(value: number): string {
  return value.toFixed(1);
}

/* ---------- shape vocabularies, one per entity kind ---------- */

function drawGameCover(ctx: Ctx): string {
  const { rng, palette } = ctx;
  const motif = ctx.game.art?.motif ?? 'blocks';
  const bands = Array.from({ length: 5 }, (_, i) => {
    const y = 12 + i * 34;
    return `<rect x="0" y="${y}" width="200" height="${n(r(rng, 6, 18))}" fill="${
      palette[i % palette.length]
    }" opacity="${n(r(rng, 0.18, 0.45))}"/>`;
  }).join('');

  const centre = (() => {
    switch (motif) {
      case 'ring':
      case 'portal':
      case 'orb':
        return `<circle cx="100" cy="100" r="${n(r(rng, 34, 52))}" fill="none" stroke="${palette[1]}" stroke-width="10"/>`;
      case 'triangle':
      case 'mountain':
        return `<polygon points="100,44 152,150 48,150" fill="${palette[1]}" opacity="0.85"/>`;
      case 'blade':
      case 'axe':
        return `<polygon points="96,36 108,36 112,132 100,158 92,132" fill="${palette[1]}"/>`;
      case 'maze':
        return `<g stroke="${palette[1]}" stroke-width="8" fill="none"><rect x="46" y="46" width="108" height="108" rx="10"/><rect x="74" y="74" width="52" height="52" rx="6"/></g>`;
      case 'flame':
        return `<path d="M100 40c22 30 34 44 34 66a34 34 0 0 1-68 0c0-22 12-36 34-66z" fill="${palette[1]}"/>`;
      case 'tree':
        return `<g fill="${palette[1]}"><rect x="94" y="96" width="12" height="62"/><circle cx="100" cy="82" r="40"/></g>`;
      default:
        return `<g fill="${palette[1]}"><rect x="56" y="56" width="42" height="42"/><rect x="104" y="104" width="42" height="42"/></g>`;
    }
  })();

  return `${bands}${centre}`;
}

function drawFigure(ctx: Ctx): string {
  const { rng, palette } = ctx;
  const shoulder = r(rng, 28, 44);
  const height = r(rng, 52, 74);
  const headR = r(rng, 13, 20);
  const cape = rng() > 0.5;
  return `
    ${cape ? `<path d="M${n(100 - shoulder - 8)} 96 L100 176 L${n(100 + shoulder + 8)} 96 Z" fill="${palette[2]}" opacity="0.7"/>` : ''}
    <path d="M${n(100 - shoulder)} 96 L${n(100 - shoulder * 0.6)} ${n(96 + height)} L${n(
      100 + shoulder * 0.6,
    )} ${n(96 + height)} L${n(100 + shoulder)} 96 Z" fill="${palette[1]}"/>
    <circle cx="100" cy="${n(96 - headR - 6)}" r="${n(headR)}" fill="${palette[1]}"/>
    <rect x="${n(100 - shoulder - 10)}" y="98" width="10" height="${n(height * 0.8)}" rx="5" fill="${palette[1]}"/>
    <rect x="${n(100 + shoulder)}" y="98" width="10" height="${n(height * 0.8)}" rx="5" fill="${palette[1]}"/>`;
}

function drawWeapon(ctx: Ctx): string {
  const { palette, entity } = ctx;
  const subtype = entity.subtype ?? '';
  const fill = palette[1];
  if (subtype.includes('gun'))
    return `<g fill="${fill}"><rect x="40" y="92" width="104" height="16" rx="4"/><rect x="126" y="86" width="34" height="10" rx="3"/><path d="M64 108 h20 l-6 34 h-20 z"/></g>`;
  if (subtype.includes('bow'))
    return `<g fill="none" stroke="${fill}" stroke-width="9"><path d="M74 34 A70 70 0 0 1 74 166"/><line x1="74" y1="34" x2="74" y2="166" stroke-width="3"/><line x1="74" y1="100" x2="150" y2="100" stroke-width="5"/></g>`;
  if (subtype.includes('shield'))
    return `<path d="M100 30 L156 52 V108 C156 140 130 162 100 172 C70 162 44 140 44 108 V52 Z" fill="${fill}"/>`;
  if (subtype.includes('hammer'))
    return `<g fill="${fill}"><rect x="94" y="60" width="14" height="110" rx="5"/><rect x="58" y="40" width="86" height="34" rx="8"/></g>`;
  if (subtype.includes('staff'))
    return `<g fill="${fill}"><rect x="96" y="52" width="11" height="120" rx="5"/><circle cx="101" cy="46" r="20"/></g>`;
  if (subtype.includes('knife'))
    return `<g fill="${fill}"><polygon points="94,40 110,40 106,116 98,116"/><rect x="88" y="116" width="28" height="10" rx="3"/><rect x="96" y="126" width="12" height="34" rx="5"/></g>`;
  if (subtype.includes('special'))
    return `<g fill="none" stroke="${fill}" stroke-width="9"><circle cx="100" cy="86" r="32"/><path d="M100 118 v46"/><path d="M76 164 h48"/></g>`;
  // sword and generic melee
  return `<g fill="${fill}"><polygon points="94,26 108,26 106,124 96,124"/><rect x="70" y="124" width="62" height="12" rx="5"/><rect x="95" y="136" width="12" height="38" rx="5"/></g>`;
}

function drawCreature(ctx: Ctx): string {
  const { rng, palette, entity } = ctx;
  const big = entity.kind === 'boss';
  const rx = big ? r(rng, 56, 70) : r(rng, 38, 52);
  const ry = big ? r(rng, 46, 60) : r(rng, 34, 46);
  const spikes = Math.round(r(rng, 5, 11));
  const points = Array.from({ length: spikes }, (_, i) => {
    const a = (i / spikes) * Math.PI * 2;
    const x = 100 + Math.cos(a) * (rx + 22);
    const y = 104 + Math.sin(a) * (ry + 22);
    return `<circle cx="${n(x)}" cy="${n(y)}" r="${n(r(rng, 5, 11))}" fill="${palette[2]}"/>`;
  }).join('');
  const eyes = Math.round(r(rng, 1, 4));
  const eyeMarkup = Array.from({ length: eyes }, (_, i) => {
    const x = 100 + (i - (eyes - 1) / 2) * 26;
    return `<circle cx="${n(x)}" cy="96" r="8" fill="${palette[0]}"/>`;
  }).join('');
  return `${points}<ellipse cx="100" cy="104" rx="${n(rx)}" ry="${n(ry)}" fill="${palette[1]}"/>${eyeMarkup}`;
}

function drawItem(ctx: Ctx): string {
  const { rng, palette, entity } = ctx;
  const subtype = entity.subtype ?? '';
  if (subtype.includes('consumable'))
    return `<g fill="${palette[1]}"><rect x="88" y="44" width="24" height="24" rx="4"/><path d="M74 68 h52 l10 66a26 26 0 0 1-26 30h-20a26 26 0 0 1-26-30z"/></g>`;
  if (subtype.includes('armor') || subtype.includes('equipment'))
    return `<path d="M100 40 L150 60 V112 C150 142 128 160 100 168 C72 160 50 142 50 112 V60 Z" fill="${palette[1]}"/>`;
  if (subtype.includes('collectible'))
    return `<polygon points="100,34 138,86 100,170 62,86" fill="${palette[1]}"/>`;
  return `<g fill="${palette[1]}"><rect x="56" y="60" width="88" height="88" rx="12"/><circle cx="100" cy="104" r="${n(
    r(rng, 14, 24),
  )}" fill="${palette[0]}"/></g>`;
}

function drawVehicle(ctx: Ctx): string {
  const { palette, entity } = ctx;
  const subtype = entity.subtype ?? '';
  if (subtype.includes('plane') || subtype.includes('spaceship'))
    return `<g fill="${palette[1]}"><polygon points="100,28 122,120 100,142 78,120"/><polygon points="30,118 100,96 170,118 100,132"/></g>`;
  if (subtype.includes('boat') || subtype.includes('ship'))
    return `<g fill="${palette[1]}"><path d="M36 116 h128 l-22 40 H58 Z"/><rect x="96" y="42" width="9" height="74"/><polygon points="105,48 152,84 105,104"/></g>`;
  if (subtype.includes('tank'))
    return `<g fill="${palette[1]}"><rect x="36" y="104" width="128" height="30" rx="12"/><rect x="66" y="78" width="68" height="28" rx="8"/><rect x="130" y="86" width="46" height="9" rx="4"/></g>`;
  return `<g fill="${palette[1]}"><path d="M40 116 l22-28h74l24 28z"/><rect x="34" y="116" width="132" height="20" rx="9"/><circle cx="66" cy="142" r="15" fill="${palette[2]}"/><circle cx="136" cy="142" r="15" fill="${palette[2]}"/></g>`;
}

function drawLocation(ctx: Ctx): string {
  const { rng, palette } = ctx;
  const towers = Array.from({ length: 7 }, (_, i) => {
    const w = r(rng, 14, 30);
    const h = r(rng, 30, 108);
    const x = 12 + i * 26;
    return `<rect x="${n(x)}" y="${n(160 - h)}" width="${n(w)}" height="${n(h)}" fill="${
      palette[i % 2 ? 1 : 2]
    }"/>`;
  }).join('');
  return `<circle cx="150" cy="48" r="20" fill="${palette[1]}" opacity="0.65"/>${towers}<rect x="0" y="160" width="200" height="40" fill="${palette[2]}"/>`;
}

function body(ctx: Ctx): string {
  switch (ctx.entity.kind) {
    case 'game':
      return drawGameCover(ctx);
    case 'character':
      return drawFigure(ctx);
    case 'weapon':
      return drawWeapon(ctx);
    case 'item':
      return drawItem(ctx);
    case 'enemy':
    case 'boss':
      return drawCreature(ctx);
    case 'vehicle':
      return drawVehicle(ctx);
    case 'location':
      return drawLocation(ctx);
    default:
      return drawGameCover(ctx);
  }
}

/** Crop and blur schedule. Reveal 0 is a tight, soft crop; the last is clean. */
function revealGeometry(reveal: number, maxReveal: number) {
  const steps = Math.max(1, maxReveal);
  const t = Math.min(1, reveal / steps);
  const scale = 0.46 + 0.54 * t;
  const size = 200 * scale;
  const offset = (200 - size) / 2;
  return { size, offset, blur: (1 - t) * 6.5, pixel: Math.round(3 + (1 - t) * 13) };
}

export const generatedArtProvider: MediaProvider = {
  id: 'generated',
  name: 'Generated clue art',
  licenseSummary:
    'Clue art is drawn by this app from each entry’s own metadata. No third-party screenshots or artwork are stored or served.',
  get(entity: AnyEntity, game: Game, reveal: number, maxReveal: number): MediaAsset {
    const rng = mulberry32(hashString(`${entity.id}|${entity.kind}`));
    const palette = paletteFor(entity, game);
    const ctx: Ctx = { rng, palette, entity, game };
    const shapes = body(ctx);
    const { size, offset, blur, pixel } = revealGeometry(reveal, maxReveal);
    const uid = `a${hashString(entity.id).toString(36)}`;

    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="${n(offset)} ${n(
      offset,
    )} ${n(size)} ${n(size)}" width="100%" height="100%" role="img" aria-hidden="true" preserveAspectRatio="xMidYMid slice">
  <defs>
    <linearGradient id="${uid}bg" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0%" stop-color="${palette[2]}"/>
      <stop offset="100%" stop-color="${palette[0]}"/>
    </linearGradient>
    <filter id="${uid}f" x="-20%" y="-20%" width="140%" height="140%">
      <feGaussianBlur stdDeviation="${n(blur)}"/>
      <feComponentTransfer><feFuncA type="discrete" tableValues="0 1"/></feComponentTransfer>
    </filter>
    <pattern id="${uid}p" width="${pixel}" height="${pixel}" patternUnits="userSpaceOnUse">
      <rect width="${pixel}" height="${pixel}" fill="none" stroke="${palette[2]}" stroke-opacity="0.16" stroke-width="1"/>
    </pattern>
  </defs>
  <rect x="0" y="0" width="200" height="200" fill="url(#${uid}bg)"/>
  <g filter="url(#${uid}f)">${shapes}</g>
  <rect x="0" y="0" width="200" height="200" fill="url(#${uid}p)"/>
</svg>`;

    const revealed = reveal >= maxReveal;
    return {
      kind: 'generated-svg',
      svg,
      alt: revealed
        ? `Stylised clue art generated for a ${entity.kind}. It is abstract shapes in the entry’s colours, not official artwork.`
        : `Partly hidden stylised clue art for a ${entity.kind}. More of it is revealed with each clue.`,
      credit: 'Generated by Gaming Guess',
      license: 'generated',
    };
  },
};

registerProvider(generatedArtProvider, true);
