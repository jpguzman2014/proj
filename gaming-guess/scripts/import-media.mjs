/**
 * Fetches freely licensed screenshots of open-source games from Wikimedia
 * Commons and writes a manifest recording the licence of every single one.
 *
 * The rule this script exists to enforce: nothing ships whose licence we cannot
 * name. It fails closed. A file whose licence is missing, unrecognised, or
 * carries usage restrictions is dropped with a reason rather than downloaded,
 * so the way to get a bad image into the build is to change this allowlist
 * deliberately, not to forget a check.
 *
 * Run:  node scripts/import-media.mjs [--dry-run] [--game <id>]
 *
 * Needs outbound access to commons.wikimedia.org and upload.wikimedia.org.
 */
import { mkdirSync, writeFileSync, readFileSync, existsSync } from 'node:fs';
import { join } from 'node:path';

const API = 'https://commons.wikimedia.org/w/api.php';
const UA = 'GamingGuess-media-import/1.0 (open-source game screenshots; contact via repository)';
const OUT_DIR = 'public/media';
const MANIFEST = 'src/media/manifest.json';

let apiCalls = 0;
let apiFailures = 0;
let lastApiError = null;

const argv = process.argv.slice(2);
const dryRun = argv.includes('--dry-run');
const onlyGame = argv[argv.indexOf('--game') + 1];

/**
 * Licences we accept, matched against the machine-readable `License` field.
 * The list lives in shared/media-licences.json so the importer and the test
 * that guards the shipped manifest cannot drift apart.
 */
const ALLOWED_LICENCE_PREFIXES = JSON.parse(
  readFileSync(new URL('../shared/media-licences.json', import.meta.url), 'utf8'),
).allowed;

const ALLOWED_EXTENSIONS = ['.jpg', '.jpeg', '.png', '.webp'];

function stripHtml(value) {
  return String(value ?? '')
    .replace(/<[^>]*>/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

async function api(params) {
  const url = new URL(API);
  url.search = new URLSearchParams({ format: 'json', formatversion: '2', ...params }).toString();
  apiCalls += 1;
  try {
    const response = await fetch(url, { headers: { 'User-Agent': UA } });
    if (!response.ok) throw new Error(`${response.status} ${response.statusText}`);
    return await response.json();
  } catch (error) {
    apiFailures += 1;
    lastApiError = error;
    throw error;
  }
}

/** Candidate file titles for one game, from a category first, then a search. */
async function candidates(spec) {
  const titles = new Set();
  for (const category of spec.categories ?? []) {
    try {
      const data = await api({
        action: 'query',
        list: 'categorymembers',
        cmtitle: `Category:${category}`,
        cmtype: 'file',
        cmlimit: '30',
      });
      for (const member of data.query?.categorymembers ?? []) titles.add(member.title);
    } catch {
      /* a missing category is normal; the search below still runs. A genuine
         network failure is caught by the check at the end of main(). */
    }
  }
  if (spec.search) {
    try {
      const data = await api({
        action: 'query',
        list: 'search',
        srsearch: spec.search,
        srnamespace: '6',
        srlimit: '30',
      });
      for (const hit of data.query?.search ?? []) titles.add(hit.title);
    } catch {
      /* ignore */
    }
  }
  return [...titles].filter((title) =>
    ALLOWED_EXTENSIONS.some((ext) => title.toLowerCase().endsWith(ext)),
  );
}

async function describe(titles, thumbWidth) {
  if (!titles.length) return [];
  const data = await api({
    action: 'query',
    titles: titles.join('|'),
    prop: 'imageinfo',
    iiprop: 'url|size|extmetadata|mime',
    iiurlwidth: String(thumbWidth),
  });
  return (data.query?.pages ?? []).flatMap((page) => {
    const info = page.imageinfo?.[0];
    if (!info) return [];
    return [{ title: page.title, info }];
  });
}

/** The gate. Returns a reason string when the file must not be used. */
function rejectReason({ title, info }) {
  const meta = info.extmetadata ?? {};
  const machine = String(meta.License?.value ?? '').toLowerCase();
  const shortName = stripHtml(meta.LicenseShortName?.value);
  const restrictions = stripHtml(meta.Restrictions?.value);

  if (!machine) return `no machine-readable licence (short name: ${shortName || 'none'})`;
  if (!ALLOWED_LICENCE_PREFIXES.some((prefix) => machine.startsWith(prefix))) {
    return `licence not on the allowlist: ${machine}`;
  }
  if (restrictions) return `carries usage restrictions: ${restrictions}`;
  if (!info.thumburl) return 'no thumbnail available';
  if (!/^image\//.test(info.mime ?? '')) return `not an image: ${info.mime}`;
  if ((info.width ?? 0) < 480) return `too small: ${info.width}px wide`;
  if (!title.toLowerCase().startsWith('file:')) return 'unexpected title namespace';
  return null;
}

function record({ title, info }, gameId, index, extension) {
  const meta = info.extmetadata ?? {};
  return {
    file: `${gameId}-${index + 1}${extension}`,
    title,
    descriptionUrl: info.descriptionurl,
    author: stripHtml(meta.Artist?.value) || 'Unknown',
    license: stripHtml(meta.LicenseShortName?.value) || String(meta.License?.value ?? ''),
    licenseMachine: String(meta.License?.value ?? '').toLowerCase(),
    licenseUrl: stripHtml(meta.LicenseUrl?.value) || info.descriptionurl,
    width: info.thumbwidth ?? info.width,
    height: info.thumbheight ?? info.height,
  };
}

async function main() {
  const sources = JSON.parse(readFileSync('media-sources.json', 'utf8'));
  const thumbWidth = sources.thumbWidth ?? 960;
  const perGame = sources.perGame ?? 3;
  const entries = Object.entries(sources.games).filter(([id]) => !onlyGame || id === onlyGame);

  if (!dryRun) mkdirSync(OUT_DIR, { recursive: true });
  const manifest = {
    generatedAt: new Date().toISOString(),
    source: 'Wikimedia Commons',
    note: 'Generated by scripts/import-media.mjs. Do not edit by hand.',
    assets: {},
  };
  let kept = 0;
  let rejected = 0;

  for (const [gameId, spec] of entries) {
    const titles = await candidates(spec);
    const described = await describe(titles.slice(0, 40), thumbWidth);
    const usable = [];

    for (const item of described) {
      if (usable.length >= perGame) break;
      const reason = rejectReason(item);
      if (reason) {
        rejected += 1;
        console.log(`  drop ${item.title}: ${reason}`);
        continue;
      }
      const extension = (item.info.thumburl.match(/\.(jpe?g|png|webp)$/i) ?? ['.jpg'])[0];
      const entry = record(item, gameId, usable.length, extension.toLowerCase());
      if (!dryRun) {
        const response = await fetch(item.info.thumburl, { headers: { 'User-Agent': UA } });
        if (!response.ok) {
          console.log(`  drop ${item.title}: download failed ${response.status}`);
          rejected += 1;
          continue;
        }
        writeFileSync(join(OUT_DIR, entry.file), Buffer.from(await response.arrayBuffer()));
      }
      usable.push(entry);
      kept += 1;
    }

    if (usable.length) manifest.assets[gameId] = usable;
    console.log(`${gameId}: kept ${usable.length} of ${described.length} candidates`);
  }

  // Fail loudly on a network problem. Reporting "0 images, all fine" when
  // every request was refused is worse than crashing, because it looks done.
  if (apiCalls > 0 && apiFailures === apiCalls) {
    throw new Error(
      `Every one of the ${apiCalls} requests to Wikimedia failed, so nothing could be ` +
        `imported. Last error: ${lastApiError?.message ?? 'unknown'}. ` +
        'This needs outbound access to commons.wikimedia.org and upload.wikimedia.org.',
    );
  }
  if (kept === 0) {
    throw new Error(
      `No images passed the licence gate across ${entries.length} game(s). ` +
        'Either the searches matched nothing or every match was rejected above.',
    );
  }

  if (!dryRun) writeFileSync(MANIFEST, `${JSON.stringify(manifest, null, 2)}\n`);
  console.log(
    `\n${dryRun ? 'Dry run. ' : ''}Kept ${kept} images, dropped ${rejected}. ` +
      `${Object.keys(manifest.assets).length} of ${entries.length} games have media.`,
  );
  if (!dryRun && !existsSync(MANIFEST)) throw new Error('manifest was not written');
}

main().catch((error) => {
  console.error(error.message);
  process.exit(1);
});
