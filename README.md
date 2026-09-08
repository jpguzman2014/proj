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

## Use it on your projects

Install once. It is then available in every project on that machine, current and future,
with nothing copied into any repo:

```bash
claude plugin marketplace add jpguzman2014/proj
claude plugin install launch-check@jpguzman2014
```

Then in any project, ask for an audit in plain language or run:

```
/launch-check
```

Updates reach every project later through `claude plugin update launch-check`.

## Make it stick in one project

The plugin gives Claude the procedure. A project that ships a public site should also
carry the standard in its own `CLAUDE.md`, so it applies while code is written rather
than only at launch. With the plugin installed, ask Claude in that project to install
the launch check standard; the skill has the files and will write them.

If you have this repo cloned, the same thing runs as one command:

```bash
./scripts/apply-to-project.sh /path/to/project
```

It installs the skill, a `CHECKLIST.md` to track audit state, and the principle,
creating `CLAUDE.md` or appending to it. Re-running refreshes the skill and leaves your
edits alone.

Skip this for libraries, CLIs, and internal services. Policy-page reminders in a library
repo are noise, and noise trains you to skim past the file when it matters.

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
  assets/PRINCIPLE.md       the standard applied while building
CLAUDE.md                   symlink to assets/PRINCIPLE.md
scripts/apply-to-project.sh installer for an existing project
```

## Not legal advice

The audit finds gaps and drafts starting points. Final wording of policy pages is signed
off by a lawyer in the operating jurisdiction. Regulated categories, among them health,
finance, and anything aimed at children, need advice well beyond this checklist.
