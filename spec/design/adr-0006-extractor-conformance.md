# ADR-0006: Three-valued semantics and extractor conformance classes

**Status:** Accepted (2026-07-28)
**Date:** 2026-07-28

> Accepted with amendments from review: *three-valued completeness* renamed **three-valued
> semantics** (it is about knowledge, not completeness), `unknown` elevated from mechanism to
> design principle, two verdict rules added (unknown never satisfies a contract; unknown
> prevents a positive closed-mode verdict), an explicit SQL-NULL warning added, and the third
> conformance class renamed *Attested* → **Signed** (see naming note).

## Principle

**Unknown is first-class information. The absence of analysis is represented explicitly.**

C2CS distinguishes three states of knowledge, not two states of presence:

```
known positive    — claims exist
known empty       — analyzed, none found
unknown           — not analyzed
```

This is epistemic honesty as a format feature — a standard that can say *"we don't know"* —
and it belongs among the spec's top-level principles, not buried in a conformance section.
It is load-bearing for the entire verification model: without it, `closed` mode is a bluff.

## Context

"Supports C2CS" must mean something, or the ecosystem fragments on day one: tools will emit
whatever subset is convenient and interop dies quietly. The SBOM world learned this — the
NTIA "minimum elements" exist because an SBOM with arbitrary gaps is worse than none, since it
*looks* complete. C2CS has a sharper version of the problem: under `contract.mode: closed`,
an absent capability is a **claim** ("spawns no subprocesses"). An extractor that simply
didn't analyze process behavior must not be indistinguishable from one that verified its
absence — `process: []` must never carry both meanings.

## Options

### Option A — No conformance levels
Anything goes. The label "C2CS compliant" becomes marketing noise; consumers cannot rely on
any field being present.

### Option B — Single monolithic bar
One high bar (full Tier 1 + Tier 2 + attestation). Honest but excludes early and partial
implementations — exactly the community the standard needs first.

### Option C — Three-valued semantics + tiered conformance classes
The load-bearing rule first, then levels on top:

**Rule: three-valued semantics.** For every Tier-1 registry category, a producer must emit
exactly one of: a set of claims, an explicit empty set (`[]`, meaning "analyzed, none
found"), or `unknown` ("not analyzed"). Silence is a spec violation.

**Verdict rules** (normative, ADR-0003's verdict semantics incorporate them):
1. **Unknown MUST NOT satisfy a contract.** Unknown is neither true nor false; it can never
   be the basis of a `confirmed`.
2. **Unknown prevents a positive closed-mode verdict.** A `closed` contract cannot be judged
   conformant while any in-scope category is unknown — the verdict must surface the gap
   rather than average over it.

**SQL-NULL warning:** three-valued logic is exactly where SQL created decades of confusion.
The spec must define `unknown` propagation exhaustively in the verdict semantics — in
particular `unknown ≠ none`: `[]` participates in verdicts (it is a claim of absence);
`unknown` participates in none. No implicit coercion in either direction, ever.

**Conformance classes:**
- **C2CS Core** — emits all Tier-1 registry categories under the three-valued rule, with
  valid provenance, for at least one subject type.
- **C2CS Semantic** — Core + Tier-2 concepts (operations, data entities) with
  confidence on inferred claims.
- **C2CS Signed** — Semantic + output delivered as signed attestations (ADR-0004).

Verifiers have their own single bar: pass the conformance test suite (ADR-0003). Extractors
get classes, verifiers get pass/fail — two different roles, two different shapes of
conformance.

> **Naming note.** Review found *Attested* technically correct but mute to a working
> developer, suggesting Trusted/Verified/Enterprise. *Verified* collides with ADR-0003's
> definition of verification, and *Trusted* claims more than the layer delivers — ADR-0004's
> own principle says attestation binds identity, not correctness. **Signed** is the honest
> plain word: it says exactly what is added, a developer understands it instantly, and it
> overclaims nothing. Revisitable before the v0.2 freeze if a better name appears.

## Recommendation

**Option C.** Three-valued semantics is the honesty mechanism that makes closed-mode
verification sound in the presence of imperfect tools — it belongs in the schema and the
verdict rules, not in documentation. The classes give early implementations a legitimate
entry point (Core) while keeping the trust-dependent label (Signed) strict.

## Consequences

- Schema change in v0.2: Tier-1 categories become three-valued. This supersedes v0.1's
  "empty list is a claim" note by making the distinction explicit. Whether `unknown` is
  literally a value (`process: unknown`) or a coverage block (`coverage: {process: analyzed}`)
  is schema design, decided in the v0.2 work — the ADR fixes the semantics, not the syntax.
- The principle above joins the spec's top-level design principles alongside the tier
  principle (ADR-0002) and the normative/descriptive principle (ADR-0001).
- Each conformance class needs a machine-checkable test fixture set, not prose requirements.
- Gaming vector: an extractor marking hard categories `unknown` forever while advertising
  "Core". Mitigation: conformance output must be per-category visible in reports, so chronic
  `unknown`s are legible to the consumer choosing tools.

## Deferred to PoC

Where .NET static analysis genuinely must say `unknown` (reflection, dynamic loading,
P/Invoke), which calibrates whether Core is achievable statically or requires observation —
and the `unknown`-as-value versus coverage-block syntax question above.
