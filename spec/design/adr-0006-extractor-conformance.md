# ADR-0006: Extractor conformance classes and the `unknown` marker

**Status:** Proposed
**Date:** 2026-07-28

## Context

"Supports C2CS" must mean something, or the ecosystem fragments on day one: tools will emit
whatever subset is convenient and interop dies quietly. The SBOM world learned this — the
NTIA "minimum elements" exist because an SBOM with arbitrary gaps is worse than none, since it
*looks* complete. C2CS has a sharper version of the problem: under `contract.mode: closed`,
an absent capability is a **claim** ("spawns no subprocesses"). An extractor that simply
didn't analyze process behavior must not be indistinguishable from one that verified its
absence.

## Options

### Option A — No conformance levels
Anything goes. The label "C2CS compliant" becomes marketing noise; consumers cannot rely on
any field being present.

### Option B — Single monolithic bar
One high bar (full Tier 1 + Tier 2 + attestation). Honest but excludes early and partial
implementations — exactly the community the standard needs first.

### Option C — Explicit `unknown` marker + tiered conformance classes
The load-bearing rule first, then levels on top:

**Rule: three-valued completeness.** For every Tier-1 registry category, a producer must
emit exactly one of: a set of claims, an explicit empty set (`[]`, meaning "analyzed, none
found"), or `unknown` ("not analyzed"). Silence is a spec violation, and `unknown` categories
are excluded from closed-mode verdicts — they cannot silently strengthen a contract.

**Conformance classes:**
- **C2CS Core** — emits all Tier-1 registry categories under the three-valued rule, with
  valid provenance, for at least one subject type.
- **C2CS Semantic** — Core + Tier-2 annotations (operations, data entities) with
  confidence on inferred claims.
- **C2CS Attested** — Semantic + output delivered as signed attestations (ADR-0004).

Verifiers have their own single bar: pass the conformance test suite (ADR-0003).

## Recommendation

**Option C.** The `unknown` marker is the honesty mechanism that makes closed-mode
verification sound in the presence of imperfect tools — it belongs in the schema, not in
documentation. The classes give early implementations a legitimate entry point (Core) while
keeping the trust-dependent label (Attested) strict.

## Consequences

- Schema change in v0.2: Tier-1 categories become three-valued (claims / empty / `unknown`).
  This supersedes v0.1's "empty list is a claim" note by making the distinction explicit.
- Each conformance class needs a machine-checkable test fixture set, not prose requirements.
- Gaming vector: an extractor marking hard categories `unknown` forever while advertising
  "Core". Mitigation: conformance output must be per-category visible in reports, so chronic
  `unknown`s are legible to the consumer choosing tools.

## Deferred to PoC

Where .NET static analysis genuinely must say `unknown` (reflection, dynamic loading,
P/Invoke), which calibrates whether Core is achievable statically or requires observation.
