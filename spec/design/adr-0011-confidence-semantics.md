# ADR-0011: Confidence semantics

**Status:** Accepted (2026-07-28)
**Date:** 2026-07-28

> Accepted with amendments from review: the definition unified across tiers (one rule,
> different authoritative reviewers — removing the Tier-1/Tier-2 asymmetry the proposed
> rule 5 introduced), the producer-property principle added, *producer pipeline* aligned to
> *producer identity + version* (ADR-0001), the can-be-wrong argument lifted into the
> principle section, and a concrete calibration example added.

## Principles

**Confidence is a forecast of survival under authoritative review.**

`confidence: 0.82` always means: *the producer estimates a 0.82 probability that this
assertion will survive its next authoritative review.* What differs between tiers is only
who the authoritative reviewer is:

- **Claims (Tier 1):** the verification engine — survival means `confirmed` against
  sufficient observation.
- **Concepts (Tier 2):** the human reviewer — survival means acceptance at promotion.

One definition, two reviewers. This makes confidence a prediction about a future,
observable event — and therefore the only kind of number that can be *wrong*, which is
precisely why it can be trusted.

Concretely: take a producer's 100 claims stated at `confidence: 0.8`. If the producer is
well calibrated, verification will confirm ≈80 of them and find drift/violation or
`not_observed`-forever in ≈20. Note what this is *not*: 0.82 is not "82% probability the
claim is true" in the abstract — it is 82% that the authoritative review, when it comes,
confirms it.

**Confidence is a property of the producer, not of the claim.** The same claim body
(ADR-0009's semantic-identity invariant) may carry 0.82 from extractor A and 0.64 from
extractor B — one claim, two producers, two forecasts. Confidence lives in attribution,
never in meaning.

## Context

Confidence appears throughout the schema (v0.2 requires it on inferred claims) but is
nowhere defined. Without a definition, every extractor will emit something different under
the same field name — raw LLM logit mass, a hand-tuned heuristic score, an ensemble
agreement ratio — and consumers will compare incomparable numbers. This is the vocabulary
problem (ADR-0002) in numeric form: a shared field name without shared semantics is not
common, only syntactic. The definition must also not leak into verification: ADR-0003 keeps
inferred content out of verdicts, and ADR-0009 forbids attribution from altering meaning.

## Options

### Option A — Tool-specific score, semantics undefined
An ordinal "higher is more sure". Honest about today's reality, but permanently blocks
cross-producer comparison and gives promotion review a number that means nothing.

### Option B — Self-reported model probability
Define it as the LLM's own stated probability. Concrete but unanchored: self-reported LLM
confidence is notoriously miscalibrated, and the definition offers no way to measure or
improve it — the number has semantics but no accountability.

### Option C — Verification forecast with measurable calibration
Adopt the principle above and attach its consequences:

- **Anchored.** The claim's truth conditions come from its registry category (ADR-0009), so
  "would be confirmed" is well-defined — confidence inherits its meaning from the same
  place the claim does.
- **Measurable.** Calibration is empirically checkable against verdict history: among a
  producer's claims stated at ~0.8, about 80% should end up confirmed. Standard proper
  scoring (e.g. Brier) applies. The spec defines the *meaning*; it does not require any
  calibration *quality* for conformance — but it makes quality measurable and comparable.
- **Scoped.** Confidences are comparable within one producer identity + version (the
  pinned producer block of ADR-0001). Cross-producer comparison is meaningful only
  alongside published calibration evidence.

**Rules:**
1. Confidence appears only on inferred content (claims and concepts). Observed claims have
   evidence, not confidence; declared claims have authority, not confidence.
2. Confidence MUST NOT affect matching or verdicts (ADR-0009's attribution invariant —
   a tool that narrows a claim's scope because confidence is low is non-conforming).
3. No conversion between confidence and the knowledge states of ADR-0006: `unknown` is not
   `confidence: 0`, and low confidence is not `unknown`. A claim at 0.3 is a hypothesis
   held weakly; `not-analyzed` is no hypothesis at all.
4. Promotion thresholds ("auto-suggest at ≥0.9") are consumer policy, never spec.
5. For Tier-2 concepts, which have no truth conditions (ADR-0009), the authoritative
   reviewer is human and the observable event is the promotion decision — the same
   definition as for claims, with a different reviewer (see Principles). No separate
   Tier-2 semantics exists.

## Recommendation

**Option C.** It composes cleanly with the existing architecture:
verdicts (ADR-0003) become the feedback signal, producer pinning (ADR-0001) defines the
comparison scope, and the epistemics stay honest (best-effort inference, now with a
falsifiable accuracy claim — the whitepaper's "calibrated uncertainty" made concrete).

## Consequences

- The spec's assessment section gains a normative definition of the confidence field; the
  registry does not change (confidence is claim-model-level, not category-level).
- A future optional `calibration:` block in the producer's self-description (or a published
  calibration report) becomes the basis for cross-producer claims — additive, post-PoC.
- The PoC gains a concrete research deliverable for the whitepaper: measured calibration
  curves for LLM-inferred claims against observed verdicts.
- Gaming vector: confidence inflation to look authoritative. Mitigation is structural —
  inflated confidence is *measurably* miscalibrated against verdict history, and chronic
  overconfidence becomes legible exactly like chronic `unknown` (ADR-0006).

## Deferred to PoC

Whether LLM-derived confidences can be calibrated well enough to be useful (the empirical
heart of the matter); what granularity calibration reporting needs (per category? per
producer?); and whether concept-confidence (rule 5) predicts human acceptance in practice.
