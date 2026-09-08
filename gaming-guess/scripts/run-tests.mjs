/**
 * Bundles the TypeScript tests with esbuild (already present as a Vite
 * dependency) and hands them to Node's own test runner. Keeps the test setup to
 * zero extra dependencies.
 */
import { build } from 'esbuild';
import { spawnSync } from 'node:child_process';
import { globSync } from 'node:fs';
import { rmSync } from 'node:fs';

const OUT = '.tmp-tests';
rmSync(OUT, { recursive: true, force: true });

const entries = globSync('tests/*.test.ts');
if (!entries.length) {
  console.error('No tests found.');
  process.exit(1);
}

await build({
  entryPoints: entries,
  outdir: OUT,
  bundle: true,
  platform: 'node',
  format: 'esm',
  target: 'node20',
  outExtension: { '.js': '.mjs' },
  external: ['node:*'],
  logLevel: 'error',
});

const bundled = entries.map(
  (entry) => `${OUT}/${entry.replace(/^tests\//, '').replace(/\.ts$/, '.mjs')}`,
);
const result = spawnSync(process.execPath, ['--test', ...bundled], { stdio: 'inherit' });
rmSync(OUT, { recursive: true, force: true });
process.exit(result.status ?? 1);
