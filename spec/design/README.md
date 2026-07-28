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

Grouped by role: the **constitution** records promises to the future — the decisions that
make it safe to build on C2CS at all; the **information model** defines what the documents
and their atoms are; **verification & knowledge** defines how claims are evaluated, trusted,
and combined.

### Constitution

| ADR | Title | Status |
|-----|-------|--------|
| [0002](adr-0002-vocabulary-strategy.md) | Curated Tier-1 effect taxonomy, namespaced Tier-2 concepts | Accepted |
| [0007](adr-0007-spec-governance.md) | Spec governance, versioning, and registry evolution | Accepted |
| [0008](adr-0008-normative-scope.md) | Normative scope — standard versus reference implementation | Accepted |
| [0010](adr-0010-semantic-stability.md) | Semantic stability | Accepted |

### Information model

| ADR | Title | Status |
|-----|-------|--------|
| [0001](adr-0001-document-model.md) | Split the model into contract and assessment documents | Accepted |
| [0005](adr-0005-identity-and-composition.md) | Globally addressable subject identity, composition-ready references | Accepted |
| [0009](adr-0009-claim-model.md) | The Claim Model — the atom of the standard | Accepted |
| [0013](adr-0013-semantic-relations.md) | Semantic relations — vocabulary, not logic | Proposed |

### Verification & knowledge

| ADR | Title | Status |
|-----|-------|--------|
| [0003](adr-0003-verification-grammar.md) | Typed matchers and formal verification semantics | Accepted |
| [0004](adr-0004-trust-chain.md) | Define C2CS as an in-toto attestation predicate | Accepted |
| [0006](adr-0006-extractor-conformance.md) | Three-valued semantics and extractor conformance classes | Accepted |
| [0011](adr-0011-confidence-semantics.md) | Confidence semantics — a forecast of survival under authoritative review | Accepted |
| [0012](adr-0012-assessment-aggregation.md) | Assessment aggregation and conflict resolution | Accepted |

Two non-ADRs, recorded so they are decisions rather than omissions: **implementations never
get ADRs** — MCP, specific AI models, and IDE integrations are informative-layer choices
(ADR-0008) and belong in tool documentation; and **composition/capability inheritance has no
ADR yet** — ADR-0005 reserves the door (`attributed-to:`), and writing the aggregation
semantics before the PoC and registry exist would decide it at its least-informed moment
(the ADR-0007 argument against premature structure).

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
