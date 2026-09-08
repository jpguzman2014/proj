/**
 * Builds a single self-contained HTML file from the Vite output.
 *
 * Everything is inlined, so the result opens from a file path, an email
 * attachment or a hosted page with no server and no second request. Used for
 * previews and for handing the app to someone who just wants to try it.
 *
 * Run `VITE_DEMO=1 npx vite build` first, so the service worker registration and
 * anything else needing a server is compiled out.
 */
import { readFileSync, writeFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const DIST = 'dist';
const OUT = process.argv[2] ?? 'dist/gaming-guess.html';

const assets = readdirSync(join(DIST, 'assets'));
const js = assets.filter((f) => f.endsWith('.js'));
const css = assets.filter((f) => f.endsWith('.css'));
if (js.length !== 1 || css.length !== 1) {
  throw new Error(`Expected one JS and one CSS asset, found ${js.length} and ${css.length}`);
}

const read = (file) => readFileSync(join(DIST, 'assets', file), 'utf8');
const html = readFileSync(join(DIST, 'index.html'), 'utf8');

// The site title is written for search results and carries an explainer. A
// preview is identified by its name alone, so use the product name here.
const title = 'Gaming Guess';
const description = html.match(/<meta\s+name="description"\s+content="([^"]*)"/)?.[1] ?? '';

// The host wraps this in its own document skeleton, so emit body content only:
// a title, the stylesheet, the mount point and the bundle.
const page = `<title>${title}</title>
<meta name="description" content="${description}" />
<style>
${read(css[0])}
</style>
<div id="root"></div>
<script type="module">
${read(js[0])}
</script>
`;

writeFileSync(OUT, page);
const kb = (Buffer.byteLength(page) / 1024).toFixed(0);
console.log(`wrote ${OUT} (${kb} KB, one file, no external requests)`);
