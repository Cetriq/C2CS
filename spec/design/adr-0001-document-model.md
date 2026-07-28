# ADR-0001: Split the model into contract and assessment documents

**Status:** Accepted (2026-07-28)
**Date:** 2026-07-28

> Accepted with amendments from review: document type renamed *Evidence* → *Assessment*
> (inferred content is hypothesis, not proof — the stronger word would lend it status it has
> not earned), a normative/descriptive principle added, two invariants added, and the producer
> homogeneity rule strengthened to require a pinned pipeline version.

## Principle

**Contracts describe what shall hold. Assessments describe what was inferred or observed.**

Contracts are normative; assessments are descriptive. Everything else in this ADR is
machinery around that sentence.

## Context

Schema v0.1 puts three kinds of content in one document, and they have different lifecycles,
owners, and storage locations:

| Content | Produced by | Cadence | Natural home |
|---------|------------|---------|--------------|
| Declared claims + `forbidden` + `contract.mode` | Humans | Changes with intent | Source repo (like an OpenAPI spec) |
| Inferred claims | Extractor / LLM | Per analysis run | Build pipeline output |
| Observed claims | Runtime harness | Per deployment / time window | Observability stack |

Mixing them forces one file to be simultaneously human-owned and machine-regenerated, which
breaks review workflows (a regenerated file churns the human-approved parts) and storage
(runtime observations do not belong in a git repo). The SBOM ecosystem distinguishes
source/build/runtime SBOMs for exactly this reason. This is the schema's ground structure —
everything else builds on it, so it must be settled first.

## Options

### Option A — Single document (v0.1 status quo)
One file per subject containing all claims, distinguished only by provenance tags.
Simple to explain; falls apart operationally as soon as analysis output is generated
continuously.

### Option B — Contract + assessment documents
Two document types:
- **Contract** (`*.c2cs-contract.yaml`) — declared claims, `forbidden`, `contract.mode`.
  Human-owned, lives in the repo, versioned with source, reviewed like code.
- **Assessment** (`*.c2cs-assessment.yaml`) — produced by exactly one producer running one
  pinned pipeline version, with exactly one `kind` (`inferred` or `observed`), bound to an
  artifact digest, referencing the contract it speaks to. Append-only in spirit: a new
  analysis or observation window produces a new assessment rather than mutating an old one.

Verification consumes one contract plus a set of assessments. Observed assessments carry
evidential weight (they point at reproducible traces); inferred assessments are hypotheses
with confidence — the shared document type must not blur that difference, which is why the
neutral name *assessment* was chosen over *evidence*.

### Option C — Three fixed document types (contract / inference report / observation report)
Like B but with inference and observation as distinct schemas. Sharper, but the two
assessment kinds share almost all structure; the split doubles schema surface for little gain.

## Recommendation

**Option B.** The homogeneity rule ("one assessment = one producer, one pipeline version,
one kind") gives Option C's clarity without a third schema. The provenance model from v0.1 is
unchanged — it just stops being the only thing separating content with different owners.

The `inferred → declared` promotion becomes a concrete, reviewable act: a human lifts a claim
from an assessment into the contract (e.g. via a generated PR), rather than a field flipping
inside one file — auditable and traceable, which is what the security audience expects.

### Invariants

1. **A contract MUST NOT contain inferred or observed claims.**
2. **An assessment MUST NOT modify contractual intent.** A verifier MUST treat any normative
   content in an assessment (`forbidden`, `contract.mode`, declared claims) as a spec
   violation, not as input.

No gray zone: the document type alone tells a consumer whether content is normative.

### Producer pinning

"One producer" is not enough for reproducibility — results change across tool, model, and
vocabulary versions. An assessment MUST pin its full pipeline:

```yaml
producer:
  extractor: c2cs-dotnet/1.2.0
  model: claude-fable-5          # inferred assessments only
  registry: 0.8.1                # vocabulary version (ADR-0002)
  schema: "0.2"
```

Two assessments are comparable only if their producer blocks match.

## Consequences

- Schema v0.2 splits into two schemas sharing common definitions (subject, claims, provenance).
- The contract gets a stable home in the repo and a normal review workflow; assessments get
  digest-bound artifact storage (see ADR-0004 — assessments are natural attestations).
- Cross-document references require stable identity — drives ADR-0005.
- Internal representations (e.g. a semantic graph inside the extractor) are implementation
  detail, deliberately **not** a document type; only contracts, assessments, and verdicts
  cross tool boundaries.
- Gaming vector: a stale contract with fresh assessments looks healthy if tools only check
  the assessments. Verification must always report *against which contract revision* a
  verdict holds.

## Deferred to PoC

Whether inferred assessments should embed pointers into source (file/symbol anchors) to make
promotion-review ergonomic, and how large assessment documents get in practice.
