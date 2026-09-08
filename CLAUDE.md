# Working principle: nothing ships unaudited

Any website or web app in this account is a legal artifact, not only code. Before it is
launched, deployed, or handed to a client, it passes the **Launch Check** — the 19-item
legal, privacy, accessibility, and honest-marketing audit in `skills/launch-check/`.

## When this applies

Run the audit before first launch, and re-run the affected section whenever a change
adds a form, an analytics or advertising tracker, a payment flow, a third-party embed,
or customer testimonials.

## While building, not only at the end

These are cheaper to get right than to retrofit, so hold to them as code is written:

- Every `<img>` gets an `alt` when it is written. Decorative images get `alt=""`.
- Every form input gets a bound `<label>`, and marketing opt-ins are separate and
  unticked.
- Body text meets 4.5:1 contrast. Never remove a focus outline without replacing it.
- Buttons are named for what they do, not "Click here".
- No new form field without a stated purpose. No new third-party script without asking
  what it sends and where.
- No testimonial, statistic, superlative, or guarantee in copy unless the owner has
  evidence for it. Never invent a review or a customer name, not even as a placeholder;
  fake reviews are separately illegal in the US, EU, and UK.
- No image or font without a traceable licence.

## Flag, do not silently accept

If a request would ship one of these gaps — analytics before a consent banner, a
launch with no privacy policy, placeholder testimonials left in a template — say so
once, propose the fix, and continue with the work.

## Not legal advice

The audit finds gaps and drafts starting points. Final wording of policy pages is signed
off by a lawyer in the operating jurisdiction.
