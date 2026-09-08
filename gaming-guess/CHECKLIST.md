# Launch Check

Status of the 19 pre-launch compliance items. Update as work lands; keep it in version
control so the audit state survives.

Legend: `[ ]` not done, `[x]` done and verified, `[?]` needs owner input, `[-]` not
applicable with the reason recorded.

Audited 8 September 2026 against the build in this directory.

## Accessibility

- [x] **Colour contrast** — 4.5:1 body text, 3:1 large text and UI, including hover,
      placeholder, and disabled states
      *Measured on the rendered pages in both colour schemes across all nine routes plus
      the open filter panel, a live question and an answered question. Two failures were
      found and fixed: the accent purple carrying white text sat at 4.31:1, and the
      difficulty-rating chips were rendering as black `buttontext`. `--accent` and
      `--accent-line` are now separate tokens because one value cannot be dark enough for
      white text and light enough for a border on the same dark ground.*
- [x] **Alt text on images** — every `<img>` has `alt`; decorative use `alt=""`
      *There is no `<img>` in the build. Clue art and QR codes are inline SVG in a
      wrapper with `role="img"` and an `aria-label`; the SVG itself is `aria-hidden`. The
      clue-art description says how much is still hidden, which is the information the
      image is actually carrying. The remote-image branch of `ClueArt` requires `alt`.*
- [x] **Keyboard friendly forms** — full Tab traversal, visible focus, no traps, labels
      bound to inputs
      *24 tab stops reached on the home screen, every one with a visible outline. Every
      input, select and textarea is bound to a `<label for>`. Play is fully keyboard
      driven: 1–4 answer, C opens a clue, Enter continues. Forms submit on Enter.*
- [x] **Clear button labels** — accessible name says what happens; no bare icon buttons
      *Checked on every route: no empty accessible names, no icon-only controls, and no
      "Submit" or "Click here". Every emoji is `aria-hidden` next to real text.*
- [x] **General accessibility** — landmarks, heading order, `lang`, 200% zoom
      *`lang="en"`, header/main/footer landmarks, a skip link, exactly one `h1` per route
      with no skipped levels, and focus moved to the heading on navigation. No horizontal
      scrolling at 320px, which is the 200% zoom condition; the two wide tables scroll
      inside their own containers.*

## Legal pages

- [x] **Privacy policy page** — real content, real processors named
      *Substantive and accurate for this build: what is stored, where, for how long, what
      party rooms transmit, that there is no analytics or advertising, rights and the
      route to a regulator. Two blocks are marked in the page as needing the operator.*
- [x] **Terms and conditions page**
      *Service description, database accuracy, trademark and nominative-use statement,
      acceptable use, and the payments position. Governing law and the liability clause
      are marked as needing the operator.*
- [x] **Cookies policy** — every cookie tabulated
      *No cookies are set. Every `localStorage` key is tabulated with its purpose and
      retention, with a note that the same EU and UK rules cover it, and that consent
      must come before any future analytics rather than after.*
- [-] **Refund policy** — published before payment is taken
      *Nothing is sold: no payment, subscription or in-app purchase. The terms say so and
      record what a refund policy must contain if that ever changes.*
- [?] **Real business details** — legal entity, address, email, registration number
      *Marked in the privacy policy. Needs the legal entity name, registered postal
      address, contact email, and company or tax number where required. A contact form
      alone does not satisfy the EU or UK requirement.*

## Privacy and data

- [x] **Cookie consent** — reject as easy as accept; nothing fires before consent
      *No banner, because nothing non-essential loads. Every stored key is strictly
      necessary to the feature the player asked for. Adding analytics later means adding
      consent before the script, and the cookies page says so.*
- [x] **Tracking audit** — every third-party call inventoried and disclosed
      *Verified in the browser across all routes and a full run: zero requests to any
      origin other than the app's own. No analytics, pixels, heat maps, session
      recorders, ad tags or social embeds. The service worker ignores cross-origin
      requests by design.*
- [-] **Form consent** — separate, unticked, specific opt-in for marketing
      *There is no marketing, no mailing list and no email field anywhere in the app.*
- [x] **Only collect necessary data** — surplus fields removed, retention set
      *Three inputs exist in total: a party nickname, a room code, and an optional free
      text note on a data report. No email, no name, no account. The nickname fields say
      the value is shown to other players and that a real name is not needed, and the
      report form asks players not to include personal information. Retention is stated
      per key on the cookies page, and one button on the statistics page deletes all of
      it. Room state is memory-only and swept when the host leaves or a room goes quiet.*
- [-] **Third-party embeds** — privacy modes or click-to-load, disclosed
      *None. No maps, videos, chat widgets, CDNs or hosted fonts. The QR code is
      generated in the browser rather than fetched from an image service, and the type
      stack is the reader's own system fonts.*

## Honest content

- [x] **Remove fake reviews** — including template placeholders
      *There are no testimonials, reviews, star ratings, customer names or logos anywhere
      in the app, and no template placeholders left behind. The only star rating is the
      player rating their own last question, stored on their own device.*
- [x] **Remove unsupported claims** — evidence on file for every claim
      *No superlatives, no "#1", no user counts, no guarantees. Numbers shown on the home
      screen are counted from the database at render time rather than asserted. The about
      page states plainly that the catalogue is hand-compiled and will contain mistakes.*
- [x] **Image copyright** — licence traceable for every asset and font
      *`CREDITS.md` lists every asset. No game screenshots, cover art, character renders
      or logos are stored or served; clue art and the app icons are generated by code in
      this repository, and no webfont is bundled or fetched. Media sits behind a provider
      interface carrying per-asset `credit` and `license`, so a licensed source added
      later cannot ship without them.*
- [?] **Check local laws** — jurisdictions in scope identified
      *The app is worldwide by default, so EU/EEA, UK, California and the other major
      regimes are all in scope as written. The operator needs to confirm where they are
      established and where they are targeting players, because that decides the
      trader-identification duties and which regulator hears a complaint.*

---

Open questions for the business owner:

- What legal entity operates this, at what registered address, with what contact email
  and company or tax number? This fills the marked block in the privacy policy.
- Who hosts the static site, and who hosts the party room server? Both need naming as
  processors, with what they log, for how long, and whether data leaves the region.
- What governing law and jurisdiction apply, and what liability wording has a lawyer in
  that jurisdiction approved? This fills the marked block in the terms.
- Which countries are being targeted, so the jurisdiction list above can be pinned down?

Not legal advice. A lawyer in the operating jurisdiction signs off on final wording.
