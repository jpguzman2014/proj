# Accessibility

Target: WCAG 2.2 Level AA. This is the level written into the European Accessibility
Act, the UK public sector regulations, and the standard US courts have effectively
converged on under the Americans with Disabilities Act.

## Colour contrast

| Content | Minimum ratio |
|---|---|
| Body text under 18pt (24px) | 4.5:1 |
| Large text, 18pt+ or 14pt+ bold | 3:1 |
| Icons, form borders, focus rings | 3:1 |

Check the pairs that get forgotten: placeholder text, disabled buttons, hover and
visited link states, text over a photograph or gradient, and light grey captions.

Grey-on-white is the single most common failure in generated designs. `#999999` on
white is 2.85:1 and fails. `#767676` is 4.54:1 and passes.

Colour must never be the only signal. A required field marked only by a red border is
invisible to a colourblind user; add text or an icon.

## Alt text

```html
<img src="team.jpg" alt="">                          <!-- decorative, correct -->
<img src="chart.png" alt="Revenue doubled in 2025">  <!-- informative, correct -->
<img src="logo.png">                                 <!-- fail: no alt -->
<img src="cat.jpg" alt="image">                      <!-- fail: says nothing -->
```

Find missing attributes quickly, then read each result in context:

```bash
grep -rnP '<img(?![^>]*\balt=)' --include=*.html --include=*.jsx --include=*.tsx .
```

Rules: describe function, not appearance. A logo that links home is
`alt="Acme, home"`. Skip "image of" and "photo of", the screen reader says that already.
Text baked into an image goes in the alt verbatim. Background images carrying meaning
need a text equivalent.

## Keyboard and forms

Tab through the entire page without touching the mouse. Watch for:

- A focus indicator that is always visible. `outline: none` with no replacement is a
  failure, and it is in most CSS resets.
- Tab order that follows the visual order. Positive `tabindex` values break this.
- No traps. Modals and date pickers must be escapable, and focus should return to the
  trigger on close.
- Custom controls built from `<div>` need `role`, `tabindex="0"`, and Enter and Space
  handlers. A real `<button>` is almost always the better answer.

Forms specifically:

```html
<label for="email">Email address</label>
<input id="email" name="email" type="email" autocomplete="email"
       aria-describedby="email-error" aria-invalid="true">
<p id="email-error" role="alert">Enter an email address with an @ symbol.</p>
```

Placeholders are not labels; they vanish on typing and usually fail contrast. Error
messages say how to fix the problem, sit next to the field, and are announced. On
submit, move focus to the first error or to a summary.

## Structure

One `<h1>` per page, no skipped levels, and headings used for hierarchy rather than
font size. Wrap regions in `<header>`, `<nav>`, `<main>`, `<footer>`. Set `<html lang>`.
Give every page a unique, front-loaded `<title>`. Add a skip-to-content link. Confirm
the page reflows without horizontal scrolling at 200% zoom and at 320px wide.

## Tooling

Automated tools catch roughly a third of real issues. Run them, then test by hand.

```bash
npx @axe-core/cli http://localhost:3000
npx lighthouse http://localhost:3000 --only-categories=accessibility --view
npx pa11y http://localhost:3000
```

The manual pass that matters: unplug the mouse and complete the primary user journey.
