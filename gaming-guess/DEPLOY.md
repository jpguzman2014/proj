# Deploying at zero cost

The whole product can run for nothing, and can even take money for nothing in
fixed cost. This file records the free path and, more importantly, the free tiers
that look free and are not.

Figures checked 8 September 2026. Free tiers move. Re-check before committing.

## The single-player app: genuinely free

`npm run build` produces a static `dist/`. No server, no database, no runtime.

**Use Cloudflare Pages.** Free tier, unlimited bandwidth, free TLS, and a
`*.pages.dev` subdomain so a domain name is optional rather than required.
Commercial use is allowed on the free plan, which matters later.

### Free tiers that will bite you

| Host | Problem |
|---|---|
| Vercel Hobby | Free, but commercial use is prohibited by the plan terms |
| GitHub Pages | Free, but the terms exclude running a business or e-commerce on it |
| Netlify free | Fine to start, but bandwidth is metered and overages are billable |

Cloudflare Pages avoids all three. Start there and you never have to migrate the
day you decide to charge.

## Party mode: the only part that needs a server

A WebSocket server has to stay awake, which is where free tiers usually fail.
Two options that actually hold up:

**Cloudflare Durable Objects, on the Workers free plan.** This is the option the
original brief pointed at, and it is now free to start:

| Limit | Free plan |
|---|---|
| Requests | About 3 million per month |
| Compute | About 390,000 GB-seconds per month |
| SQLite storage | 1 GB |
| WebSocket message size | 32 MiB |

Incoming WebSocket messages bill at a 20:1 ratio, so a million received messages
counts as fifty thousand requests. A party quiz sends a handful of small messages
per player per question. You will not approach these limits with real players.

**Deno Deploy** is the lower-effort alternative. Its free tier supports
WebSockets and the port is mostly swapping the transport layer, since `Room` in
`server/index.mjs` holds all the logic and knows nothing about `ws`.

### What to avoid

- **Render free tier** spins down after inactivity and cold-starts in the tens of
  seconds. A room that takes 30 seconds to answer a QR scan is a dead feature.
- **Fly.io and Railway** now run on trial credits rather than a standing free
  tier. Free until it is not.

### Free but not zero effort

Neither option runs the current `server/index.mjs` unmodified. Budget the port.
Nothing about the client changes: point `VITE_ROOM_SERVER` at the `wss://` URL
and the UI does not know the difference.

## Taking money later, still at zero fixed cost

This is the part people get wrong. Selling does not require paying anyone up
front.

**Merchant of record, on the web, not through an app store.** Paddle and Lemon
Squeezy both charge 5% plus 50 cents per transaction, with no monthly fee. You
pay only when you earn. Watch the add-ons: international cards and PayPal each
add 1.5%, subscriptions add 0.5%, and payouts to a non-US bank account cost 1%.

Three things you would otherwise pay for come bundled:

- **Tax.** They are the seller of record and handle VAT and GST registration,
  collection and filing worldwide. Doing this yourself means registering in
  dozens of jurisdictions.
- **Sanctions and territory gating.** They maintain the excluded list and enforce
  it at checkout. See `LICENSING.md` for what that list has to contain.
- **Consumer-facing contract.** They are the trader on the invoice, which
  materially reduces your own consumer-contract exposure and the lawyer time it
  would otherwise take.

**What app stores cost, and why to skip them for now.** Apple charges 99 US
dollars a year for a developer account, and Google charges 25 dollars once. The
web build needs neither, installs as a PWA on Android, iPhone, Windows and Mac,
and skips store review entirely. Age ratings also stop being required, because
those are a storefront obligation rather than a legal one for a website.

## The costs you cannot make zero

Two, and both are deferrable while the app is free.

**A contact address.** Trader identification in the EU and UK needs a real
address at which you can be served, and publishing a home address is a poor
trade. A service address costs a small annual fee. A merchant of record softens
this by being the trader on the invoice, but your privacy policy still needs a
controller identity.

**A lawyer, eventually.** Not before a free launch. Before taking money, someone
should read the terms and the refund policy, because aggressive liability limits
are struck out wholesale in consumer contexts in many countries.

## Recommended order

1. **Now, at zero.** Cloudflare Pages, `*.pages.dev` subdomain, free app, no
   sale. Party mode ported to Durable Objects or Deno Deploy, also zero.
2. **When there are players.** Buy a domain, roughly 10 dollars a year, and get a
   service address if you want the contact details off your home.
3. **When you decide to charge.** Add a merchant of record. Still no fixed cost.
   Get the terms reviewed before the first sale, not after.
4. **Only if the data says so.** App stores, and the ratings and fees that come
   with them.

Sources for the figures above are listed in the pull request discussion. Verify
current terms before relying on any of them.
