# Privacy and Data

## Cookie consent

Under the EU ePrivacy Directive and the UK PECR, consent comes **before** any
non-essential cookie or tracker is set. Strictly necessary cookies, such as session,
authentication, and load balancing, are exempt.

A compliant banner: reject is as prominent and as few clicks as accept; nothing
non-essential fires before a choice; granular categories, not one all-or-nothing switch;
no pre-ticked boxes; no cookie wall; the choice is stored and easy to change later.

Common failures found in generated sites: Google Analytics in `<head>` firing on load
with the banner purely cosmetic; an "Accept" button with a "Learn more" link and no way
to refuse; a banner that sets its own consent cookie recording acceptance the user never
gave.

Verify rather than trust the widget. Load the site in a clean private window, open the
Application panel, and confirm the cookie jar and network requests are empty of
third-party calls until consent is given.

## Tracking inventory

Build the list from the network tab, not from memory. For each entry record the vendor,
the purpose, the data sent, the lawful basis, and the consent category. Watch for
analytics, advertising pixels, session recorders that capture keystrokes and therefore
often capture form contents, chat widgets, A/B testing tools, and error monitors that
can capture personal data inside stack traces.

Session recording deserves particular care. Recording a user filling in a form captures
whatever they type, sometimes including payment or health information, and needs
explicit consent plus input masking.

## Form consent

```html
<!-- correct: separate, unticked, specific -->
<input type="checkbox" id="marketing" name="marketing">
<label for="marketing">Email me occasional product news. Unsubscribe anytime.</label>

<!-- fail: pre-ticked -->
<input type="checkbox" checked>

<!-- fail: bundled -->
<label>By submitting you agree to our terms and to receive marketing.</label>
```

Consent is freely given, specific, informed, and unambiguous. Bundling marketing consent
into the act of submitting a contact form fails all four. Service email, such as an
order confirmation, does not need marketing consent; a newsletter does.

Record what was consented to and when. Honour unsubscribe requests promptly and include
the link in every marketing email.

## Data minimisation

Collect only what the purpose requires. A contact form needs a name, an email, and a
message. It does not need a phone number, a date of birth, a company size, or an
address. Every extra field is a field to secure, to disclose, to retain, and to breach.

Set a retention period per data type and actually delete on schedule. "We keep data as
long as necessary" without a period is not a retention policy.

## Third-party embeds

| Embed | Risk | Mitigation |
|---|---|---|
| Google Fonts | Sends IP to Google; ruled a GDPR breach in Germany | Self-host the font files |
| YouTube | Sets tracking cookies on load | `youtube-nocookie.com`, or click-to-load |
| Google Maps | Cookies and IP | Static image, or load behind consent |
| Chat widgets | Often full session tracking | Behind consent, disclose the vendor |
| Social buttons | Track across sites even unclicked | Plain links to the profile |

Click-to-load is the general answer: render a placeholder, load the real embed only
after the user opts in. It fixes the legal problem and the performance problem at once.

Add a Content Security Policy once the third-party inventory is settled. It is also the
control that stops an embed from quietly adding trackers later.
