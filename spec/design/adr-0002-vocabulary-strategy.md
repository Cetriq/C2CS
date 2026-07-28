# ADR-0002: Curated Tier-1 effect taxonomy, namespaced Tier-2 concepts

**Status:** Accepted (2026-07-28)
**Date:** 2026-07-28

> Accepted with amendments from review: a summarizing principle added, *semantic annotations*
> renamed **semantic concepts** (Tier 2 describes operations, domain objects, classifications,
> and intent — more than metadata on top of something else), *closed* registry reworded
> **curated** (quality without "we decide everything"), Tier-1 identifier immutability made a
> hard rule, an abstraction guideline added (categories name effects, not technologies), and
> the cross-language interop consequence made explicit.

## Principle

**Tier 1 defines observable behavior. Tier 2 defines intended meaning.**

## Context

The word *Common* in "Code to Common Semantics" stands or falls here. If two extractors
describe the same behavior with different words, there is no common semantics — only YAML.
Interoperability of the entire standard lives in the vocabulary, and retrofitting one onto a
format that shipped with free text is close to impossible. This is likely the single largest
piece of work in the standard.

The two tiers have opposite needs. Tier 1 (verifiable core) must be *mechanically comparable*
across producers — a finite, precisely defined set of effect categories and attributes.
Tier 2 (semantic concepts) must be *extensible* — business operations and data
classifications differ per domain and cannot wait for a standards body.

## Options

### Option A — Free text everywhere
Maximum flexibility, zero interop. Rejected on arrival: it forfeits the project's premise.

### Option B — One closed enum for everything
Interop by force, but Tier 2 cannot be enumerated centrally; the standard would either lag
reality or bloat into a universal business ontology (a known graveyard).

### Option C — Hybrid: curated registry for Tier 1, namespaced conventions for Tier 2
- **Tier 1:** a curated, versioned effect taxonomy where each category has defined attributes
  and a defined observation mapping. Maintained as a registry (`spec/registry/effects/`),
  versioned independently of the schema so categories can be added without a schema release.
- **Tier 2:** an open, namespaced convention model on the OpenTelemetry semantic-conventions
  pattern: a central registry of well-known names (`c2cs.data.personal`, `c2cs.op.*`), plus
  vendor/org namespaces (`acme.*`) that interoperate syntactically today and can be promoted
  into the central registry when they prove general — the same path by which OpenTelemetry's
  conventions grew.

## Recommendation

**Option C.** OpenTelemetry semantic conventions are the proven precedent for exactly this
shape of problem: a core registry, namespaces, and a promotion path — extension without
fragmentation and without waiting for the standard. The same small-hard-core /
large-extensible-layer shape recurs in OpenAPI, Kubernetes CRDs, SPDX, and OCI.

Hard rules:

1. **Observation mapping required.** A Tier-1 category is not admitted to the registry
   without a defined observation mapping (which observable signal confirms or refutes it —
   see ADR-0003). The registry cannot invent semantics; every category must answer *"how do
   we know this exists?"* No unverifiable entries in the verifiable tier.
2. **Tier-1 identifiers are immutable.** A category can be added, and it can be deprecated
   with a pointer to its replacement — but its meaning is never redefined. Tools depend on
   identifiers keeping their meaning forever.
3. **No vendor namespaces in Tier 1.** Only Tier-2 concepts may use them; meaningful
   observable behavior must not be expressible in words only one vendor's tools understand.
4. **Unknown namespaced names are preserved, not dropped**, by conforming tools, so custom
   vocabularies survive round-trips.

Guideline for registry work: **Tier-1 categories name effects, not technologies or APIs.**
The category test is "what happened to the world?", not "which library was called". This
pushes toward abstractions like *persistent storage* (covering SQL Server, SQLite, Redis AOF,
RocksDB) rather than *database*, and it forces imprecise buckets like *ipc* (pipe? localhost
HTTP? shared memory? signals?) to be either decomposed into observable mechanisms or defined
at a level where one observation mapping genuinely covers the category. The tension to manage:
too concrete leaks technology names into the standard; too abstract weakens the observation
mapping. Each admission resolves this per category.

## Consequences

- New artifact: `spec/registry/` with its own changelog and versioning (see ADR-0007).
- **The interoperability this buys is between languages, not just between tools.** A Rust
  service and a .NET service map onto the same Tier-1 categories, so their behavior is
  comparable even though their code, runtimes, and extractors share nothing. This is where
  "common" materializes — the registry is the rendezvous point.
- Extractors map language/OS-specific behavior (a .NET `HttpClient` call, a P/Invoke) onto
  registry categories — this mapping layer is where extractor quality lives.
- Gaming vector: hiding meaningful behavior in vendor namespaces that no tool understands.
  Mitigation: hard rule 3 — Tier-1 claims may not use vendor namespaces.

## Deferred to PoC

Which Tier-1 categories are actually extractable statically from .NET; whether the initial
registry (~6–8 categories) covers real services or needs early additions; and where the
effect-vs-technology abstraction line lands for the storage and IPC categories.
