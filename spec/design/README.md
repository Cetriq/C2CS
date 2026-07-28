# C2CS design decisions (ADRs)

Architecture Decision Records for the C2CS standard. Each record captures one load-bearing
design decision: the context, the options considered, a recommendation, and its consequences.

**Status meanings:** `Proposed` (recommendation on the table, not yet decided) →
`Accepted` / `Rejected` → `Superseded by ADR-XXXX`.

These are the decisions that must be settled *before* a PoC, because they are effectively
one-way doors once the format has users. Questions that only implementation can answer
(operation granularity, static extractability from .NET, confidence calibration, agent
usefulness) are deliberately **not** ADRs — the PoC is the experiment instrument for those,
and each ADR notes what it defers to it.

## Index

| ADR | Title | Status |
|-----|-------|--------|
| [0001](adr-0001-document-model.md) | Split the model into contract and assessment documents | Accepted |
| [0002](adr-0002-vocabulary-strategy.md) | Curated Tier-1 effect taxonomy, namespaced Tier-2 concepts | Accepted |
| [0003](adr-0003-verification-grammar.md) | Typed matchers and formal verification semantics | Proposed |
| [0004](adr-0004-trust-chain.md) | Define C2CS as an in-toto attestation predicate | Proposed |
| [0005](adr-0005-identity-and-composition.md) | URI-based identity, composition-ready references | Proposed |
| [0006](adr-0006-extractor-conformance.md) | Extractor conformance classes and the `unknown` marker | Proposed |
| [0007](adr-0007-spec-governance.md) | Spec governance, versioning, and registry evolution | Proposed |

## Method

The spec is written use-case first. Every field must be traceable to at least one of these
consumers; a field no consumer needs is cut:

1. **AI agent via MCP** — "what does this system do?" without reading the codebase.
2. **`c2cs verify` in CI** — does the implementation conform to the declared contract?
3. **GRC / compliance report** — what data is processed, what is forbidden, who approved it?
4. **Architecture view** — operations, data flows, and dependencies across a system.

Each ADR should also survive an adversarial pass: *how would someone cheat this design?*
(e.g. `closed` mode with a `*` scope on everything — technically conformant, semantically
empty). Known gaming vectors belong in the ADR's Consequences section.

New records use [adr-template.md](adr-template.md).
