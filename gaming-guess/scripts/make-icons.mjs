/**
 * Generates the PNG app icons from scratch.
 *
 * Written by hand rather than pulled from an icon site so the app ships with no
 * image whose licence we cannot point at: these bytes are generated here.
 */
import { deflateSync } from 'node:zlib';
import { writeFileSync, mkdirSync } from 'node:fs';

const OUT = new URL('../public/icons/', import.meta.url);

const CRC_TABLE = (() => {
  const table = new Int32Array(256);
  for (let n = 0; n < 256; n += 1) {
    let c = n;
    for (let k = 0; k < 8; k += 1) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
    table[n] = c;
  }
  return table;
})();

function crc32(buffer) {
  let c = -1;
  for (const byte of buffer) c = CRC_TABLE[(c ^ byte) & 0xff] ^ (c >>> 8);
  return (c ^ -1) >>> 0;
}

function chunk(type, data) {
  const length = Buffer.alloc(4);
  length.writeUInt32BE(data.length);
  const body = Buffer.concat([Buffer.from(type, 'ascii'), data]);
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(body));
  return Buffer.concat([length, body, crc]);
}

function encodePng(size, pixels) {
  const header = Buffer.alloc(13);
  header.writeUInt32BE(size, 0);
  header.writeUInt32BE(size, 4);
  header[8] = 8; // bit depth
  header[9] = 6; // RGBA
  const raw = Buffer.alloc((size * 4 + 1) * size);
  for (let y = 0; y < size; y += 1) {
    raw[y * (size * 4 + 1)] = 0; // no filter
    pixels.copy(raw, y * (size * 4 + 1) + 1, y * size * 4, (y + 1) * size * 4);
  }
  return Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    chunk('IHDR', header),
    chunk('IDAT', deflateSync(raw, { level: 9 })),
    chunk('IEND', Buffer.alloc(0)),
  ]);
}

function draw(size) {
  const px = Buffer.alloc(size * size * 4);
  const set = (x, y, [r, g, b]) => {
    if (x < 0 || y < 0 || x >= size || y >= size) return;
    const i = (y * size + x) * 4;
    px[i] = r;
    px[i + 1] = g;
    px[i + 2] = b;
    px[i + 3] = 255;
  };

  const bgTop = [18, 14, 42];
  const bgBottom = [12, 15, 22];
  const accent = [127, 92, 255];
  const light = [242, 245, 251];

  for (let y = 0; y < size; y += 1) {
    const t = y / (size - 1);
    const colour = bgTop.map((c, i) => Math.round(c + (bgBottom[i] - c) * t));
    for (let x = 0; x < size; x += 1) set(x, y, colour);
  }

  // Rounded gamepad body.
  const bodyW = size * 0.68;
  const bodyH = size * 0.44;
  const bx = (size - bodyW) / 2;
  const by = (size - bodyH) / 2;
  const radius = bodyH / 2;
  for (let y = 0; y < size; y += 1) {
    for (let x = 0; x < size; x += 1) {
      const cx = Math.min(Math.max(x, bx + radius), bx + bodyW - radius);
      const cy = Math.min(Math.max(y, by + radius), by + bodyH - radius);
      if ((x - cx) ** 2 + (y - cy) ** 2 <= radius ** 2) set(x, y, accent);
    }
  }

  // D-pad on the left, two buttons on the right.
  const arm = size * 0.045;
  const dx = size * 0.36;
  const dy = size * 0.5;
  for (let y = 0; y < size; y += 1) {
    for (let x = 0; x < size; x += 1) {
      const inCross =
        (Math.abs(x - dx) <= arm * 2.4 && Math.abs(y - dy) <= arm) ||
        (Math.abs(x - dx) <= arm && Math.abs(y - dy) <= arm * 2.4);
      if (inCross) set(x, y, light);
      const r = size * 0.042;
      const buttons = [
        [size * 0.63, size * 0.45],
        [size * 0.7, size * 0.56],
      ];
      for (const [bxc, byc] of buttons) {
        if ((x - bxc) ** 2 + (y - byc) ** 2 <= r ** 2) set(x, y, light);
      }
    }
  }

  return px;
}

mkdirSync(OUT, { recursive: true });
for (const size of [180, 192, 512]) {
  writeFileSync(new URL(`icon-${size}.png`, OUT), encodePng(size, draw(size)));
  console.log(`wrote icon-${size}.png`);
}
