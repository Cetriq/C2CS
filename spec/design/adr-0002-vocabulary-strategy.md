# ADR-0002: Closed Tier-1 taxonomy, namespaced Tier-2 conventions

**Status:** Proposed
**Date:** 2026-07-28

## Context

The word *Common* in "Code to Common Semantics" stands or falls here. If two extractors
describe the same behavior with different words, there is no common semantics — only YAML.
Interoperability of the entire standard lives in the vocabulary, and retrofitting one onto a
format that shipped with free text is close to impossible. This is likely the single largest
piece of work in the standard.

The two tiers have opposite needs. Tier 1 (verifiable core) must be *mechanically comparable*
across producers — a finite, precisely defined set of effect categories and attributes.
Tier 2 (semantic annotations) must be *extensible* — business operations and data
classifications differ per domain and cannot wait for a standards body.

## Options

### Option A — Free text everywhere
Maximum flexibility, zero interop. Rejected on arrival: it forfeits the project's premise.

### Option B — One closed enum for everything
Interop by force, but Tier 2 cannot be enumerated centrally; the standard would either lag
reality or bloat into a universal business ontology (a known graveyard).

### Option C — Hybrid: closed registry for Tier 1, namespaced conventions for Tier 2
- **Tier 1:** a closed, versioned effect taxonomy (`filesystem`, `network`, `process`,
  `environment`, `database`, `ipc`, …) where each category has defined attributes and a
  defined observation mapping. Maintained as a registry (`spec/registry/effects/`), versioned
  independently of the schema so categories can be added without a schema release.
- **Tier 2:** an open, namespaced convention model on the OpenTelemetry semantic-conventions
  pattern: a central registry of well-known names (`c2cs.data.personal`, `c2cs.op.*`), plus
  vendor/org namespaces (`acme.*`) that interoperate syntactically today and can be promoted
  into the central registry when they prove general.

## Recommendation

**Option C.** OpenTelemetry semantic conventions are the proven precedent for exactly this
shape of problem: a core registry, namespaces, and a promotion path — extension without
fragmentation and without waiting for the standard.

Two hard rules:
1. A Tier-1 category is not admitted to the registry without a defined **observation mapping**
   (which observable signal confirms or refutes it — see ADR-0003). No unverifiable entries in
   the verifiable tier.
2. Unknown namespaced names must be **preserved, not dropped** by conforming tools, so custom
   vocabularies survive round-trips.

## Consequences

- New artifact: `spec/registry/` with its own changelog and versioning (see ADR-0007).
- Extractors map language/OS-specific behavior (a .NET `HttpClient` call, a P/Invoke) onto
  registry categories — this mapping layer is where extractor quality lives.
- Gaming vector: hiding meaningful behavior in vendor namespaces that no tool understands.
  Mitigation: Tier-1 claims may not use vendor namespaces; only Tier-2 annotations may.

## Deferred to PoC

Which Tier-1 categories are actually extractable statically from .NET, and whether the initial
registry (~6–8 categories) covers real services or needs early additions.
