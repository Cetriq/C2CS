# ADR-0012: Assessment aggregation and conflict resolution

**Status:** Proposed
**Date:** 2026-07-28

## Principle

**Observations accumulate; ignorance never overrides knowledge.**

An observation is an existential statement — *this event was seen*. Existential statements
cannot contradict each other, so at the observed level there are no conflicts to resolve:
what looks like a conflict is always a difference in *coverage*, and ADR-0006's knowledge
states already have the vocabulary for that.

## Context

A verdict is produced from one contract plus *a set of* assessments (ADR-0003), but nothing
yet defines how the set combines. The gap becomes real immediately in practice: extractor A
analyzed filesystem and found nothing, harness B observed a filesystem write, extractor C
didn't look. Which category status wins? Do observations from different producers merge?
What about two inferred assessments that disagree? Left undefined, every verification
engine will aggregate differently — breaking ADR-0003's determinism promise precisely
where multiple tools are involved, which is the ecosystem's whole point.

## Options

### Option A — One assessment per verdict
Punt: verdicts take a single assessment; merging is someone else's problem. This just moves
aggregation into unspecified pre-processing tools — the divergence happens anyway, now
invisibly and unauditably.

### Option B — Weighted merging
Combine assessments with weights, confidence math, or majority votes. Imports probabilistic
judgment into the verdict path, which ADR-0003 defines as a pure, deterministic function —
and gives implementations knobs that guarantee disagreement.

### Option C — Deterministic set semantics
**Admissibility first.** An assessment participates in a verdict only if it references the
same contract digest, its subject artifact digests match the contract's subject, and its
registry version is compatible. Inadmissible assessments are excluded and listed in the
verdict as excluded, with reasons — silent exclusion would fake coverage.

**Observed claims: union.** The observed event set for a category is the union of observed
claims across all admissible observed assessments whose windows overlap `evaluated_over`.
A sighting in any assessment is a sighting.

**Coverage: knowledge-maximal.** Per category, aggregated status is:
- `analyzed` if at least one admissible observed assessment analyzed the category;
- known-empty only if *all* assessments that analyzed the category report empty **and** the
  union contains no events — one producer's `[]` means "I saw nothing", never "nothing
  happened", so a single observed event anywhere outranks any number of empty reports;
- `unknown` only if no admissible assessment analyzed the category.

These three rules are the principle restated: positive knowledge accumulates monotonically,
absence-of-sighting is weak, absence-of-analysis is nothing.

**Inferred assessments: never aggregated.** They do not participate in verdicts (ADR-0003),
and the spec defines no ensemble math over confidences (ADR-0011 scopes confidence to one
pipeline). Disagreeing inferred assessments are surfaced *side by side* in promotion review
— disagreement between producers is information for the human, not noise to average away.
A producer that internally ensembles multiple models is one pinned pipeline (ADR-0001) and
owns its own combination logic.

**Attribution.** The verdict records, per category, which assessments contributed — so a
verdict built on one harness and a verdict built on five are distinguishable at a glance.

## Recommendation

**Option C.** It is the only option that keeps verdicts a deterministic function of their
inputs while letting the input set grow. The conformance suite (ADR-0003) gains multi-
assessment fixtures: overlapping observations, empty-vs-sighting, unknown-vs-analyzed, and
inadmissibility cases — aggregation disputes get settled by fixtures like everything else.

## Consequences

- Schema v0.3: the verdict's `evaluated_over.assessments` gains excluded entries with
  reasons; `overall.coverage` is defined as the aggregate above; per-category contribution
  lists are added.
- Window semantics get one clarification: an observed assessment contributes only the part
  of its window overlapping `evaluated_over` (partial overlap is fine; the evidence
  pointers carry timestamps).
- Gaming vector: coverage laundering — spinning up a trivial harness that marks every
  category `analyzed` with `[]` to upgrade aggregate coverage and unlock a `conformant`
  outcome. Mitigation: the per-category contribution list makes single-source coverage
  legible, and conformance class rules (ADR-0006) apply to harnesses too — an observed
  assessment's `analyzed` must be backed by the category's observation mapping actually
  being instrumented, which fixtures can probe.

## Deferred to PoC

Whether window overlap should require a minimum observation duration before `analyzed`
counts (a five-second trace technically analyzes the network category); and whether
excluded-assessment reporting needs machine-readable reason codes from day one.
