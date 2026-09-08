# Release checklist

Everything between here and selling this worldwide. Updated 8 September 2026,
against the build in this directory.

Legend: `[x]` done and verified, `[ ]` ready to do, `[?]` needs the owner,
`[L]` needs a lawyer, `[-]` not applicable with the reason recorded.

**Verdict: this is legal to sell.** Nothing in the build carries copyright risk,
because it serves no publisher media. The remaining work is ordinary commercial
compliance. The one thing that cannot be made legal cheaply is publisher
screenshots and cover art, so do not add them. Reasoning in `LICENSING.md`.

The five things standing between this and a paid launch, in order of lead time:

1. Incorporate and get the trading identity on the site. Everything else waits on it.
2. Pick a merchant of record. Do not take card payments directly.
3. Submit the age rating questionnaires. Rejections cost weeks.
4. Get a lawyer over the terms and the refund policy.
5. Import the catalogue from Wikidata, because 38 games is a demo, not a product.

---

## 1. Launch Check, the 19 items

### Accessibility

- [x] **Colour contrast** — measured on rendered pages in both colour schemes
      across every route. Two failures found and fixed. Re-verified after the
      nickname and refunds changes.
- [x] **Alt text on images** — no `<img>` in the build. Inline SVG carries
      `role="img"` and an `aria-label`.
- [x] **Keyboard friendly forms** — 24 tab stops on the home screen, every one
      with a visible outline. Play is fully keyboard driven.
- [x] **Clear button labels** — no empty accessible names, no icon-only controls.
- [x] **General accessibility** — `lang`, landmarks, skip link, one `h1` per
      route, no horizontal scroll at 320px.
- [ ] **Independent accessibility review** before a paid launch. The checks above
      are automated and catch what automation catches. A screen reader user
      finding the app unplayable is a refund and a complaint, not a bug report.

### Legal pages

- [x] **Privacy policy** — real content, retention stated, regulator route given.
- [x] **Terms of use** — service, accuracy, trademarks, acceptable use, territories.
- [x] **Cookies** — no cookies set; every storage key tabulated with retention.
- [x] **Refund policy** — published at `#/refunds`, covers the 14-day withdrawal
      right, the digital content exception, method, timing and exclusions.
- [?] **Real business details** — legal entity, registered postal address, contact
      email, company or tax number. Marked in the privacy policy. Blocks launch.

### Privacy and data

- [x] **Cookie consent** — nothing non-essential loads, so no banner is owed.
- [x] **Tracking audit** — verified in the browser: zero requests to any origin
      but the app's own, across every route and a full run.
- [-] **Form consent** — no marketing, no mailing list, no email field.
- [x] **Only collect necessary data** — no free text reaches any server. Display
      names are picked from a fixed list. Retention stated per key, one button
      deletes everything.
- [-] **Third-party embeds** — none. QR codes generated in-browser, system fonts.

### Honest content

- [x] **No fake reviews** — no testimonials, ratings, customer names or logos.
- [x] **No unsupported claims** — no superlatives or user counts. Home screen
      numbers are counted from the database at render time.
- [x] **Image copyright** — `CREDITS.md` covers every asset. All generated here.
- [?] **Local laws** — worldwide minus the exclusions in section 4. Confirm where
      the entity is established and which markets are actively targeted.

## 2. Money

- [?] **Incorporate.** Everything with a name on it waits for this.
- [ ] **Merchant of record, not a payment gateway.** App stores, or Paddle or
      Lemon Squeezy for the web build. They become the seller and handle VAT and
      GST registration, collection and filing across every market. Taking card
      payments directly means owning tax registration in every country you sell
      into, which is not a reasonable burden for a quiz app.
- [ ] **Wire the checkout acknowledgement.** EU and UK buyers of digital content
      keep the 14-day withdrawal right unless they expressly consent to immediate
      supply and acknowledge losing it, at the point of purchase. The refund page
      describes it; the checkout has to actually collect it.
- [?] **Decide the model.** One-off, subscription or free with a paid tier. The
      refund page and the store listings both depend on the answer.
- [L] **Consumer contract review** of the terms and the refund policy in the home
      jurisdiction. Aggressive liability limits are struck out wholesale in
      consumer contexts in many countries.

## 3. Data and media

- [x] **No publisher media.** Clue art and icons generated in this repository.
- [x] **Provider seam** in place, carrying per-asset credit and licence.
- [ ] **Import the catalogue from Wikidata.** CC0, so bulk commercial copying is
      permitted with no attribution owed. Fill the `sources` field per row.
      38 games is a demo. This is a product gap more than a legal one, but doing
      it via Wikidata rather than scraping a commercial catalogue is what keeps
      the EU database right out of the picture.
- [ ] **Provenance in the build.** Once the import lands, every row names where
      it came from. That is what shows the catalogue was assembled, not lifted.
- [-] **Licensed screenshots.** Deliberately not doing this. Revisit only if
      retention data justifies the fees and the residual copyright exposure, and
      read the second table in `LICENSING.md` before budgeting.

## 4. Territories

- [ ] **Configure exclusions in the payment provider and app store dashboards.**
      They maintain the sanctions lists. Do not hand-maintain this.
- [ ] **Mirror the resulting list onto the refunds and availability page**, so the
      page and the enforcement never disagree.

Excluded for sanctions, where selling can be criminal rather than civil:
Russia, Belarus, Iran, North Korea, Syria, Cuba, and the Crimea, Donetsk and
Luhansk regions of Ukraine.

Excluded for market entry cost: China, which needs an ICP filing and an NPPA
publication licence for a game, and is not achievable without a local publisher.

- [?] **Confirm against your provider's own list before launch.** Sanctions move,
      and their list is the one that gates the transaction.

## 5. Age ratings

- [ ] **IARC questionnaire.** One submission produces ESRB, PEGI, USK, ClassInd,
      ACB and GRAC ratings, covering Google Play, the Microsoft Store and
      Nintendo. Germany, Brazil, Australia and South Korea are handled by it.
- [ ] **Apple's own age rating**, filled in at App Store submission. Apple does
      not use IARC.
- [ ] **Keep the answers on file.** A rating obtained on wrong answers is void,
      and misrating is a removal reason.

Expect the lowest or near lowest band everywhere. Submit anyway. Unrated is a
removal reason on its own.

## 6. Staying out of online safety scope

- [x] **No user-generated content.** Names picked from a fixed vocabulary, never
      typed. Server replaces anything outside it, verified against a patched
      client that sent markup and a phone number.
- [x] **Report notes never leave the device.**
- [ ] **Guard the boundary.** Adding typed names, chat, reactions or server-side
      report syncing puts the product inside the UK Online Safety Act and
      equivalents, triggering a risk assessment, a reporting route and moderation
      duties. Small services are not exempt. Treat any such feature request as a
      compliance decision, not a product one.
- [?] **Children.** A gaming quiz will be judged likely to be accessed by
      children. Collecting nothing keeps the data exposure near zero, but the UK
      Children's Code imposes design duties regardless. Worth a look once the
      monetisation model is decided, because paid upsells to children are where
      that code bites hardest.

## 7. Shipping it

- [ ] **Domain and static hosting** for the web build.
- [ ] **Host the room server** with TLS, and set `VITE_ROOM_SERVER` to the `wss://`
      URL. Party mode is the only thing needing a server at all.
- [ ] **Rate limit room creation.** Rooms are cheap but not free, and nothing
      currently stops one client opening thousands.
- [?] **Name both hosts as processors** in the privacy policy, with what each logs,
      for how long, and whether data leaves the region. Marked in the page.
- [ ] **Error monitoring**, if any. Anything added here is a third-party data flow
      and needs a line in the privacy policy and a look at whether it changes the
      no-tracking claim.
- [ ] **Re-run the three audit scripts** against the deployed URL, not just the
      local preview.

## 8. Before every future release

- [ ] Re-run accessibility, contrast and reflow checks.
- [ ] Confirm the external request count is still zero.
- [ ] Confirm no free-text field reaches a server.
- [ ] Confirm no publisher media entered the build.
- [ ] Re-read section 6 if any social feature was added.

---

Open questions for the owner, consolidated:

- What legal entity, at what registered address, with what contact email and
  company or tax number?
- One-off purchase, subscription, or free with a paid tier?
- Which merchant of record?
- Who hosts the static site and who hosts the room server?
- What governing law and jurisdiction, and who reviews the terms?
- Which markets are actively targeted, as opposed to merely reachable?

Not legal advice. A lawyer in the operating jurisdiction signs off the final
wording and reviews any agreement named here.
