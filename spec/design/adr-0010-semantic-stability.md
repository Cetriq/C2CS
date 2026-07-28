# ADR-0010: Semantic stability

**Status:** Proposed
**Date:** 2026-07-28

## Principle

**The meaning of an existing semantic identifier MUST never change.**

Vocabularies may *add*, *deprecate*, and *replace*. They may never *redefine*. If a meaning
must change, that is a new identifier — `customer` may not mean something different in
registry 1.4 than it did in 1.0, ever.

## Context

Two accepted ADRs already carry fragments of this rule: ADR-0002 makes Tier-1 identifiers
immutable (hard rule 2), and ADR-0007 makes deprecated registry entries valid indefinitely.
But the rule is scattered and Tier-1-scoped, while the exposure is widest exactly where
nothing yet states it: the central Tier-2 concept vocabulary (`c2cs.*`), which will grow
fastest, attract the most contribution pressure, and — being "just semantics" — will tempt
well-meaning "clarifications" that silently shift meaning. Every long-lived registry
ecosystem (HTTP methods, MIME types, IANA registries, OpenTelemetry semantic conventions)
survives on this one rule; registries that allowed redefinition have produced decades of
ambiguity. With documents that are signed and immutable (ADR-0004) and interpretable
forever (ADR-0007), a meaning shift would retroactively falsify existing attestations —
the worst possible failure for a standard whose product is dependability.

## Options

### Option A — Status quo: per-part rules
Keep the two scattered rules and extend piecemeal. Invites exactly the gap this ADR closes:
each new vocabulary (Tier-2 central concepts, future relation vocabulary, verdict values)
needs its own rule, and one will be forgotten.

### Option B — Versioned meanings (`customer@2`)
Allow redefinition under version qualifiers. This is a new identifier wearing the old one's
name — it re-imports the ambiguity (which `customer` did the author mean?) while adding
syntax. Rejected.

### Option C — One stability rule across all normative vocabularies
The principle above applies uniformly to **every identifier in the normative set**
(ADR-0008): Tier-1 categories and their matcher attributes, central Tier-2 concepts,
relation vocabularies when they arrive, verdict values, conformance class names, predicate
type URIs. Mechanics:

- Every registry entry carries `status: active | deprecated`; deprecation requires a
  `replacement:` pointer (or an explicit `replacement: none` with rationale).
- Deprecated entries remain valid indefinitely (ADR-0007) — deprecation steers new
  documents, it never invalidates old ones.
- Editorial clarification of an entry's prose is allowed only where it does not alter what
  conforming implementations accept or produce; where the conformance fixtures would
  change, it is a redefinition, and redefinition means a new identifier.
- Vendor namespaces (`acme.*`) are outside the standard's authority, but promotion into the
  central registry (ADR-0002) is a one-way stability commitment: from promotion onward the
  identifier is frozen like any other.

## Recommendation

**Option C.** It is mostly consolidation — two existing rules generalized into one
principle with one enforcement mechanism — bought cheap now, impossible to retrofit after
the registry grows. The fixture test ("would existing conformance fixtures change?") gives
the rule teeth: stability disputes are settled by artifacts, not by argument (ADR-0008).

## Consequences

- The registry entry format (ADR-0002's draft, informed by ADR-0009's claim model) gains
  the `status`/`replacement` fields from day one, so no migration is ever needed.
- ADR-0002 hard rule 2 and ADR-0007's deprecation rule become instances of this principle;
  their texts stand, this ADR is the general form.
- Contribution guidelines get an unambiguous test for reviewers of registry PRs: additive,
  deprecating, or redefining? Only the first two merge.
- Gaming vector: meaning drift via prose "clarifications" that fixtures don't cover.
  Mitigation: a registry entry is not accepted without fixtures exercising its meaning
  (ADR-0002/0003), so the clarification test above has something to bite on.

## Deferred to PoC

Nothing empirical — like ADR-0007, this is process armor that should be in place before the
registry's first external contribution.
