# Idea note — Common Semantics as a machine-native intermediate layer

*Date: 2026-06-20*
*Trigger: Zhu et al., "Large Language Models Do Not Always Need Readable Language" (BabelTele), arXiv:2606.19857.*

## The breakthrough

C2CS's central pipeline is **Code → semantic inference → policy proposal**. The middle term —
the "Common Semantics", the inferred representation of a program's probable intent — has so far
been treated implicitly as something structured and human-inspectable (prose-like intent,
readable resource/data-flow descriptions).

The BabelTele paper removes that assumption. It shows empirically that **human readability,
natural-language typicality, and model-side semantic recoverability can be decoupled**: LLMs can
both *produce* and *consume* compact, opaque, non-human-readable representations that still carry
the semantics. Reported result: **99.5% semantic fidelity at 27.9% of the original text length**,
and — critically — **zero-shot cross-model transfer** (text compressed by one model is decodable
by a different model family without fine-tuning).

The shift for C2CS: **the Common Semantics layer does not need to be human-readable to be
useful.** It can be a *machine-native, high-density representation* of inferred intent — optimized
for model decodability and semantic density rather than for a human reader.

## Why this matters for C2CS

1. **Efficiency / inline feasibility.** If the intent representation is dense and model-native,
   the inference layer (ReadAhead) becomes cheaper to run and to carry inline before/during
   execution — the cost objection to running semantic inference in the hot path weakens.

2. **Direct intent exchange between security components.** Cross-model decodability means one
   model's inferred intent-hypothesis can be consumed by another model (a different EPO policy
   generator, a different verifier) without a human-readable lingua franca in between. The
   "Common" in Common Semantics can be a *shared machine representation*, not English prose.

3. **A grounded replacement for a speculative claim.** Section Y of the whitepaper currently
   carries an "Emerging AI communication models" paragraph flagged as speculative and unverified.
   BabelTele is the concrete, empirical citation that paragraph was waiting for — it can move from
   speculation toward grounded forward-looking work.

## The double-edged security implication (for the threat model)

The same decoupling is an **adversarial surface**, not only an opportunity. If intent can be
encoded in forms opaque to humans but decodable by models, then:

- **Obfuscation gains a new axis.** An adversary can express or hide intent in machine-native
  forms that human reviewers cannot audit but that some models still act on — sharpening the
  obfuscation and adversarial-software boundaries already named in the Limitations section.
- BabelTele's own *Risks* note makes the analogous point: transforming text into a compact
  non-standard representation "may alter the behavior of the original text in unexpected ways"
  and could compromise safety in safety-critical domains.

So the dependency runs both ways: a machine-native Common Semantics layer is what could make
C2CS efficient and composable, **and** it is a capability adversaries can turn against the
analyzer. Both belong in the paper — the opportunity in Section Y, the risk in Limitations.

## Open questions to pursue

- Is a BabelTele-style representation *faithful enough for security decisions*? 99.5% fidelity on
  QA is not the same bar as policy correctness — a small semantic loss can be a large
  privilege-boundary error. Needs its own evaluation (see `experiments/`).
- Calibrated uncertainty: can the dense representation carry confidence, or does opacity hide it?
- Cross-model transfer is "systematic but not universal" in the paper (compressor–reader pair
  matters). For C2CS that means the intent representation may not be safely portable across
  arbitrary security models — portability must be measured, not assumed.

## Status

Captured as a note only. Not yet woven into `c2cs-whitepaper.md`. Citation added to
`references.bib`.
