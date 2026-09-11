# Picking this up on a PC

Everything in the repository runs offline. Two jobs need outbound network access
that the cloud session did not have, and they are the two that matter next.

## Get it running

```bash
git clone <this repo>
cd gaming-guess
npm ci

npm run dev        # http://localhost:5173
npm run server     # party rooms, ws://localhost:8787, second terminal
npm test           # 30 tests: data, engine, filters, scoring, media licences
npm run build      # static site in dist/
```

Party mode needs both running. Open the app, choose Play with Friends, create a
room, then scan the QR code with a phone on the same network. The phone needs to
reach your machine, so use your LAN address rather than `localhost` if the QR
code does not resolve.

## Job one: fetch the screenshots

This is what makes the Open Source Arcade mode real. It was written and tested
here but never run, because the sandbox blocks Wikimedia.

```bash
npm run import:media                       # all 38 open-source games
node scripts/import-media.mjs --dry-run     # see what it would take, fetch nothing
node scripts/import-media.mjs --game 0-ad   # one game, to sanity-check output
```

Read the console output rather than trusting the exit code. It prints every file
it dropped and why, which is where you will find out whether a search matched
the wrong thing. It fails loudly if the network is refused, rather than
reporting a successful import of nothing.

Then `npm run dev` and play the arcade mode. Expect to prune: the searches are
broad, and a few games will pull in a logo or a box shot instead of gameplay.
Fix those by narrowing the entry in `media-sources.json` and re-running.

`public/media/` is generated and gitignored. `npm run dev:media` writes labelled
placeholders if you want to work on the mode without network; never ship a build
made with them.

## Job two: decide about RAWG before writing code

You asked to proceed on RAWG. Three constraints shape what can be built, and two
of them change the architecture rather than just adding a file.

**Attribution is exact and unavoidable.** Their terms require you to attribute
RAWG as the source of the data and images, with an active hyperlink from every
page where their data appears. Not a credits page. Every page.

**No redistribution.** Their wording is that you may use the data with your API
access only for your own projects, and not resell it or make it available to
other businesses. The practical consequence: you cannot import their catalogue
into a JSON file, commit it, and ship it in a static bundle, which is exactly
how the app works today. Their data has to be fetched at runtime.

**The API key cannot live in the client.** A static site's bundle is public, so a
key in it is a public key on your quota. That means a small proxy holding the key
server-side. A Cloudflare Worker does this on the free plan and sits alongside
the room server you are already going to deploy there.

Those three together turn a static app into a server-backed one. That is the
real cost of RAWG, not the fee.

**The free tier is tighter than it looks.** 20,000 API requests per month. If the
client asks RAWG per question, a handful of players exhausts it. A proxy with
short-lived response caching fixes that, but caching their whole catalogue to
avoid calling them is the thing the redistribution term prohibits. Keep the cache
short and per-request, not a mirror.

**Say it once more before you spend money: RAWG serving an image does not give
you copyright in it.** The screenshots in their database belong to publishers.
Their terms are a contract between you and RAWG and do not bind Nintendo. This
is the exposure the whole design removed. Using RAWG for metadata is a different
and much smaller question than using it for images, and the two decisions can be
made separately.

If you want the metadata without any of the above, Wikidata is CC0 and can be
bulk-copied, committed and shipped with no attribution owed. It is the reason
`sources` exists on the `Game` type.

### Where the code goes

- Media: register a provider in `src/media/`, next to `commonsProvider.ts`. The
  interface already carries `credit`, `license`, `licenseUrl` and `sourceUrl`,
  and `ClueArt` already renders them, so attribution cannot be forgotten.
- Metadata: an importer alongside `scripts/import-media.mjs`, filling `sources`.
- Proxy: a new worker, deployed beside the room server.

## What to check once it is running

- The arcade mode fills up and the images look like gameplay, not logos
- Party mode works phone-to-laptop on your own network
- `npm test` still passes after the import, especially the media licence tests
- The three audit scripts in the pull request discussion, re-run against your
  local build rather than the cloud preview

## The state of things

Done and verified: eleven solo modes, the question engine and its fairness
guards, filters, statistics, PWA, party multiplayer end to end, the accessibility
and contrast audits, the policy pages, and the media pipeline.

Not done: the media import has never actually run, the room server has not been
ported to a free host, and the catalogue is 38 mainstream titles plus 38
open-source ones, which is a demo rather than a product.

`CHECKLIST.md` is the full release list. `LICENSING.md` records why each media,
metadata, trademark and territory decision was made. `DEPLOY.md` has the hosting
and cost model.
