# Launch Check

A pre-launch audit for public-facing websites, packaged so it applies to every project
rather than being remembered once.

AI-generated sites ship the same gaps every time: no privacy policy, analytics firing
before consent, images with no `alt`, forms nobody can Tab through, and template
testimonials from customers who do not exist. Those are legal exposure, not polish.
This repo turns the 19-item checklist into something Claude runs on its own.

## The 19 items

**Accessibility** — colour contrast, alt text on images, keyboard friendly forms, clear
button labels, general accessibility.

**Legal pages** — privacy policy, terms and conditions, cookies policy, refund policy,
real business details.

**Privacy and data** — cookie consent, tracking audit, form consent, collect only
necessary data, third-party embeds.

**Honest content** — remove fake reviews, remove unsupported claims, image copyright,
check local laws.

## Use it in this repo

The skill is already active here. Ask for an audit in plain language, or invoke it:

```
/launch-check
```

## Apply it to an existing project

```bash
./scripts/apply-to-project.sh /path/to/project
```

That copies the skill into `.claude/skills/launch-check`, drops a `CHECKLIST.md` to
track audit state, and adds the working principle to the project's `CLAUDE.md`, creating
it or appending to it. Re-running refreshes the skill and leaves your edits alone.

## Apply it to every future project

Install it once as a plugin and it is available everywhere, with no per-project copying:

```bash
claude plugin marketplace add jpguzman2014/proj
claude plugin install launch-check@jpguzman2014
```

Updates then reach every project through `claude plugin update launch-check`.

Use the plugin for the audit skill itself, and `apply-to-project.sh` when a repo should
also carry the checklist and the principle in its own `CLAUDE.md`. The two are
complementary: the plugin gives Claude the procedure, and `CLAUDE.md` makes the
standard hold while code is being written rather than only at launch.

## Layout

```
.claude-plugin/
  plugin.json               plugin manifest
  marketplace.json          marketplace manifest, so the repo installs as above
skills/launch-check/
  SKILL.md                  the audit procedure
  references/               detail per area, loaded as needed
    accessibility.md        WCAG 2.2 AA thresholds, alt text, keyboard, tooling
    legal-pages.md          what each policy page must actually contain
    privacy-data.md         consent, tracking inventory, embeds, minimisation
    claims-and-content.md   fake reviews, substantiation, licensing, jurisdictions
  assets/CHECKLIST.md       trackable copy of the 19 items
CLAUDE.md                   the standard applied while building
scripts/apply-to-project.sh installer for an existing project
```

## Not legal advice

The audit finds gaps and drafts starting points. Final wording of policy pages is signed
off by a lawyer in the operating jurisdiction. Regulated categories, among them health,
finance, and anything aimed at children, need advice well beyond this checklist.
