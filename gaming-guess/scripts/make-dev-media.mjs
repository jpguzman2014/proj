/**
 * Generates clearly-labelled placeholder screenshots so the screenshot mode can
 * be developed without network access to Wikimedia Commons.
 *
 * These are NOT screenshots and must never be shipped. They are credited in the
 * manifest as locally generated placeholders precisely so that a build made
 * with them is obvious rather than plausible. `public/media/` is gitignored, and
 * `npm run import:media` overwrites the manifest with real, licensed entries.
 */
import { deflateSync } from 'node:zlib';
import { writeFileSync, mkdirSync } from 'node:fs';
import { OPEN_GAME_IDS } from './open-game-ids.mjs';

const CRC = (() => {
  const table = new Int32Array(256);
  for (let n = 0; n < 256; n += 1) {
    let c = n;
    for (let k = 0; k < 8; k += 1) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    table[n] = c;
  }
  return table;
})();
const crc32 = (bytes) => {
  let c = -1;
  for (const byte of bytes) c = CRC[(c ^ byte) & 0xff] ^ (c >>> 8);
  return (c ^ -1) >>> 0;
};
const chunk = (type, data) => {
  const length = Buffer.alloc(4);
  length.writeUInt32BE(data.length);
  const body = Buffer.concat([Buffer.from(type, 'ascii'), data]);
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(body));
  return Buffer.concat([length, body, crc]);
};
const encodePng = (w, h, pixels) => {
  const header = Buffer.alloc(13);
  header.writeUInt32BE(w, 0);
  header.writeUInt32BE(h, 4);
  header[8] = 8;
  header[9] = 6;
  const raw = Buffer.alloc((w * 4 + 1) * h);
  for (let y = 0; y < h; y += 1) {
    pixels.copy(raw, y * (w * 4 + 1) + 1, y * w * 4, (y + 1) * w * 4);
  }
  return Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    chunk('IHDR', header),
    chunk('IDAT', deflateSync(raw, { level: 6 })),
    chunk('IEND', Buffer.alloc(0)),
  ]);
};

const hash = (text) => {
  let h = 2166136261;
  for (let i = 0; i < text.length; i += 1) {
    h ^= text.charCodeAt(i);
    h = Math.imul(h, 16777619);
  }
  return h >>> 0;
};

const W = 640;
const H = 400;

function scene(id) {
  const seed = hash(id);
  const hue = seed % 360;
  const rgb = (deg, light) => {
    const k = (n) => (n + deg / 30) % 12;
    const a = 0.4 * Math.min(light, 1 - light);
    const f = (n) => light - a * Math.max(-1, Math.min(k(n) - 3, Math.min(9 - k(n), 1)));
    return [f(0), f(8), f(4)].map((v) => Math.round(v * 255));
  };
  const sky = rgb(hue, 0.62);
  const mid = rgb((hue + 40) % 360, 0.45);
  const ground = rgb((hue + 200) % 360, 0.24);

  const px = Buffer.alloc(W * H * 4);
  const set = (x, y, colour) => {
    if (x < 0 || y < 0 || x >= W || y >= H) return;
    const i = (y * W + x) * 4;
    px[i] = colour[0];
    px[i + 1] = colour[1];
    px[i + 2] = colour[2];
    px[i + 3] = 255;
  };
  for (let y = 0; y < H; y += 1) {
    const t = y / H;
    const band = sky.map((v, i) => Math.round(v + (mid[i] - v) * t));
    for (let x = 0; x < W; x += 1) set(x, y, band);
  }
  for (let i = 0; i < 14; i += 1) {
    const bw = 20 + ((seed >> i) % 60);
    const bh = 40 + ((seed >> (i + 3)) % 180);
    const bx = i * 47;
    for (let y = H - 90 - bh; y < H - 90; y += 1) {
      for (let x = bx; x < bx + bw; x += 1) set(x, y, i % 2 ? mid : ground);
    }
  }
  for (let y = H - 90; y < H; y += 1) for (let x = 0; x < W; x += 1) set(x, y, ground);
  return encodePng(W, H, px);
}

mkdirSync('public/media', { recursive: true });
const assets = {};
for (const id of OPEN_GAME_IDS) {
  const file = `${id}-dev.png`;
  writeFileSync(`public/media/${file}`, scene(id));
  assets[id] = [
    {
      file,
      title: 'Locally generated placeholder',
      descriptionUrl: 'https://example.invalid/placeholder',
      author: 'Locally generated placeholder, not a screenshot',
      license: 'Placeholder, do not ship',
      licenseMachine: 'cc0',
      licenseUrl: 'https://example.invalid/placeholder',
      width: W,
      height: H,
    },
  ];
}

writeFileSync(
  'src/media/manifest.json',
  `${JSON.stringify(
    {
      generatedAt: new Date().toISOString(),
      source: 'Locally generated placeholders, NOT for shipping',
      note: 'Written by scripts/make-dev-media.mjs so the screenshot mode can be developed offline. Run npm run import:media to replace this with real, licensed images.',
      assets,
    },
    null,
    2,
  )}\n`,
);

console.log(
  `wrote ${OPEN_GAME_IDS.length} placeholders to public/media/ and a development manifest.\n` +
    'These are not screenshots. Run npm run import:media before shipping.',
);
