# ADR-0008: Normative scope — what the standard is, versus what we ship

**Status:** Proposed
**Date:** 2026-07-28

## Context

Spun off from the ADR-0003 review. The ADRs so far define a taxonomy (0002) and a
verification semantics (0003), which means C2CS is becoming two products at once: an
**exchange format** others implement, and a **reference engine** we build. Before any
external party implements C2CS, the boundary must be explicit — which artifacts define
conformance (normative), and which are merely ours (informative). Standards that leave this
implicit drift into "the vendor's implementation is the spec", which kills the neutral-layer
positioning, or into "the prose is the spec", which kills interoperability.

This also fixes where the commercial line can be drawn (from the product discussion: spec
open, reference tooling open source, verification/governance services commercial): nothing
normative may be commercial, or the standard is a moat in disguise and adopters will treat
it as one.

## Options

### Option A — Everything we publish is the standard
Simple, but freezes reference implementations into de-facto normativity; every bug in our
verifier becomes someone's compatibility requirement (the "bug-for-bug compatible" trap).

### Option B — Only the schema is normative
Too narrow: two conforming documents could still be verified differently (ADR-0003's whole
point), so interop would be syntactic only.

### Option C — Explicit normative set, everything else informative
**Normative** (defines conformance; open license; versioned; changes via the ADR-0007
process):
1. The document schemas (contract, assessment, verdict).
2. The registry — category meanings, matcher grammars, normalization rules, observation
   mappings (ADR-0002/0003).
3. Verdict semantics (the verdict table and evaluation rules).
4. The conformance test suites (documents, per-category fixtures, verifier fixtures) —
   the operative definition of every rule above: where prose and fixtures disagree, the
   fixtures win, and the disagreement is a spec bug to fix publicly.
5. Identity and attestation bindings (ADR-0004/0005 — predicate types, URI rules).

**Informative** (ours, replaceable, no conformance authority): the reference extractor, the
reference verifier, the MCP server, SDKs, documentation, examples. A reference
implementation passing the suite is *a* conforming implementation, never *the* definition
of conformance.

## Recommendation

**Option C.** One sentence carries the decision: **conformance is defined by artifacts, not
by our tools.** Two consequences of that sentence are worth making explicit:

- A defect found in a reference implementation is just a bug. A defect found in a fixture is
  a *spec* change and goes through governance (ADR-0007), however small it feels.
- Anyone must be able to build a certified-conforming C2CS implementation using only the
  normative set, without reading our tool source. If they cannot, the normative set is
  incomplete — that is the acceptance test for the spec itself.

## Consequences

- `spec/` gains a top-level NORMATIVE.md (or section) listing the five normative artifact
  classes and their current versions — the page an implementer starts from.
- The licensing split gets its enforcement point: the normative set is open (CC BY /
  fixture code under a permissive license); commercial products live strictly in the
  informative zone.
- The conformance suite becomes release-gating for ourselves too: our reference tools have
  no special status and must pass like anyone else's.
- Gaming vector: "extended conformance" — a vendor passing the suite while adding
  semantics that change verdicts (embrace-and-extend). Mitigation: the conformance claim is
  scoped ("conforms to C2CS x.y, registry z") and verdict documents name the registry
  version they were evaluated under, so divergence is at least visible and attributable.

## Deferred to PoC

Nothing empirical — but the *completeness* of the normative set is testable the day someone
tries to implement a verifier from the normative artifacts alone; the PoC team should
periodically attempt exactly that exercise without peeking at the reference code.
