# ADR-0009: The Claim Model — the atom of the standard

**Status:** Proposed
**Date:** 2026-07-28

## Principle

**A claim has truth conditions; a concept has meaning.**

A *claim* is a bounded, category-typed statement about observable behavior whose truth
conditions are supplied by its registry category. A *concept* (Tier 2) carries intended
meaning and has no truth conditions — which is exactly why concepts never participate in
verdicts (ADR-0002/0003). The two are parallel atoms, never the same thing.

## Context

Every accepted ADR uses the word *claim*; none defines it. ADR-0001 splits claims by
document kind, ADR-0002 gives them vocabulary, ADR-0003 evaluates them, ADR-0004 signs
them, ADR-0005 addresses them, ADR-0006 distinguishes their absence from ignorance — all
without a normative definition of the thing itself. Schema v0.2 already contains a *latent*
model: every claim in the examples has a common core (ID, category, matcher attributes)
plus attribution that varies by document kind (contract: `by`/`date`/`promoted_from`;
inferred: `confidence`/`source`; observed: `evidence`/`first_seen`/`last_seen`). That
pattern is currently a convention of the example files, not a norm. Since registry entries
must define claim attributes per category (ADR-0002/0003), the claim model is a
prerequisite for the registry draft — the atom must be defined before the periodic table.

## Options

### Option A — Glossary prose only
A definitions section in the spec. Cheap, but non-normative in practice: nothing forces
schemas, registry entries, and fixtures to agree with it, so drift is guaranteed.

### Option B — One universal claim object
A single schema object where every claim carries scope, matcher, effect, mode, attributes,
and evidence fields. Over-generalized: it forces evidence fields onto declared claims and
attribution fields onto observations, recreating exactly the blur ADR-0001 removed.

### Option C — Common core + category semantics + kind-scoped attribution profiles
A claim is normatively defined as four parts:

1. **Identity** — a claim ID (grammar per ADR-0005), globally addressable as
   `subject + contract digest + claim-id`.
2. **Category** — a reference into the registry, which supplies the claim's *meaning*, its
   *matcher grammar*, and its *observation mapping* (its truth conditions). A claim without
   a registry category is not a claim.
3. **Scope** — the matcher attributes (host, path, direction, …) drawn from the category's
   grammar, bounding what the claim covers.
4. **Polarity** — *capability* (permissive: this MAY happen) or *prohibition* (`forbidden`:
   this MUST NOT happen). Prohibitions are always declared (ADR-0001).

**Attribution profiles** attach per document kind and never alter meaning:
declared → `by`, `date`, optional `promoted_from`; inferred → `confidence`, optional
`source`; observed → `evidence`, `first_seen`, `last_seen`.

**Invariants:**
- A claim's meaning is exhausted by category + scope + polarity. Attribution identifies and
  contextualizes; it MUST NOT change what the claim states.
- Claims are evaluated only by a verification engine (ADR-0003); no claim evaluates itself.
- `[]` (known empty) and `not-analyzed` (unknown) are **category-level knowledge states,
  not claims** (ADR-0006) — which is why a verdict over an empty category references the
  category, not a claim ID.

## Recommendation

**Option C.** It normativizes exactly the structure v0.2 already exhibits, keeps ADR-0001's
document separation intact at the atom level, and gives the registry draft its contract:
each category entry must define the scope vocabulary (part 3) and truth conditions (part 2)
for its claims. The spec gains a normative "Claim model" section (per ADR-0008 it belongs
to the normative set); conformance fixtures for document validity derive from it.

## Consequences

- Schema v0.3 restates its capability/forbidden structures in claim-model terms; the
  document schemas validate the core structurally and the attribution profile by kind.
- Registry entry format gains explicit slots: meaning, scope vocabulary (matcher grammar),
  observation mapping — one definition per category, as ADR-0003 already requires.
- Tier-2 concepts get the parallel definition (identity + namespaced vocabulary + meaning,
  no truth conditions), closing the loop with ADR-0002.
- Gaming vector: semantics smuggled into attribution (e.g. tooling that treats low
  `confidence` as narrowing a claim's scope). The invariant above makes that a spec
  violation, checkable in fixtures.

## Deferred to PoC

Whether observed events and observed claims need distinct shapes (raw event vs aggregated
sighting), and whether the claim core needs an extension point for future categories with
non-matcher scopes (e.g. quantitative limits).
