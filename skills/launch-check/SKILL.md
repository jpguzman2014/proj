---
name: launch-check
description: Audit a website or web app for the legal, privacy, accessibility, and honest-marketing gaps that AI-generated ("vibecoded") sites almost always ship with. Use before launching, shipping, deploying, or handing over any public-facing site, when adding a contact form, analytics, payments, embeds, or reviews, and whenever asked about privacy policy, terms and conditions, cookie consent, GDPR, WCAG, alt text, colour contrast, keyboard navigation, refund policy, or whether a site is "safe to put live".
---

# Launch Check

A public website is a legal artifact, not just code. This skill audits the 19 things that
sink AI-generated sites: missing policy pages, silent tracking, inaccessible forms, and
claims the business cannot back up.

**This is not legal advice.** It finds gaps and drafts starting points. A lawyer in the
operating jurisdiction signs off on the final wording.

## When to run

Run the full audit before first launch and before any release that adds a form, a
tracker, a payment flow, a third-party embed, or customer testimonials. Re-run the
relevant section when only one area changed.

## Procedure

Work the four sections below in order. Accessibility and Privacy findings are usually
code changes you can make directly. Legal Pages and Honest Content usually need facts
only the site owner has — collect those into one question block rather than asking
piecemeal.

For each item, record one of: **pass**, **fail**, or **needs owner input**. Never mark
an item pass because a file exists; open it and confirm it says something real. A
`privacy.html` containing lorem ipsum is a fail, and a more dangerous one than a
missing page, because it looks handled.

### 1. Accessibility

Detail: `references/accessibility.md`

- **Colour contrast** — body text at least 4.5:1 against its background, large text and
  UI borders at least 3:1. Check the real rendered pairs, including hover, placeholder,
  disabled, and text over images.
- **Alt text on images** — every `<img>` has `alt`. Decorative images get `alt=""`, not a
  missing attribute and not "image". Informative images describe the information.
- **Keyboard friendly forms** — every control reachable by Tab in a sensible order, a
  visible focus ring, no keyboard traps, and Enter submits. Labels tied to inputs with
  `for`/`id`, errors announced not just coloured red.
- **Clear button labels** — the accessible name says what happens. "Submit", "Click
  here", and icon-only buttons with no `aria-label` all fail.
- **General accessibility** — landmarks, one `h1` and no skipped heading levels, `lang`
  on `<html>`, and a page that survives 200% zoom.

### 2. Legal Pages

Detail: `references/legal-pages.md`

- **Privacy policy page** — required almost everywhere the site collects anything,
  cookies and analytics included.
- **Terms and conditions page** — the contract with the user.
- **Cookies policy** — what is set, by whom, why, and for how long.
- **Refund policy** — mandatory in most jurisdictions the moment money changes hands.
- **Real business details** — legal entity name, registered address, contact email, and
  company or tax number where the jurisdiction requires it. A bare contact form does not
  satisfy this.

Every one of these must be linked from a persistent location, normally the footer, and
reachable from every page.

### 3. Privacy and Data

Detail: `references/privacy-data.md`

- **Cookie consent** — if non-essential cookies or trackers load, consent comes first.
  Reject must be as easy as accept, and nothing non-essential fires before a choice.
- **Tracking audit** — inventory every script that phones home: analytics, pixels, heat
  maps, session recorders, ad tags. Each one needs a lawful basis and a line in the
  policy.
- **Form consent** — an unticked, separate opt-in for marketing. Pre-ticked boxes and
  consent bundled into "by submitting you agree to everything" are not consent.
- **Only collect necessary data** — delete fields the business will never use. Date of
  birth on a newsletter form is a liability with no upside.
- **Third-party embeds** — maps, fonts, videos, chat widgets, and CDNs set cookies and
  send IP addresses abroad. Prefer privacy modes, load them behind consent, and disclose.

### 4. Honest Content

Detail: `references/claims-and-content.md`

- **Remove fake reviews** — invented testimonials, stock-photo customers, and
  AI-generated praise are illegal in most consumer-protection regimes. Remove them.
  Placeholder reviews left in from a template count.
- **Remove unsupported claims** — "#1", "guaranteed", "best", percentages, and awards all
  need evidence on file. Cut or qualify anything the owner cannot document.
- **Image copyright** — confirm a licence for every image, icon, and font. Search-engine
  results and AI-generated images of real brands or people are not cleared by default.
- **Check local laws** — the applicable regime follows the customers, not the server.
  Identify the jurisdictions in scope and note the extra duties each one adds.

## Reporting

Produce a table of the 19 items with pass, fail, or needs owner input, then group the
work: fixes you already made, fixes ready to make, and the questions only the owner can
answer. Lead the report with anything that carries legal exposure right now, such as
live tracking without consent or testimonials that may be fabricated.

Copy `assets/CHECKLIST.md` into the project as a tracked file so the state of the audit
survives the session.
