# ADR-0001: Split the model into contract and evidence documents

**Status:** Proposed
**Date:** 2026-07-28

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
(runtime evidence does not belong in a git repo). The SBOM ecosystem distinguishes
source/build/runtime SBOMs for exactly this reason. This is the schema's ground structure —
everything else builds on it, so it must be settled first.

## Options

### Option A — Single document (v0.1 status quo)
One file per subject containing all claims, distinguished only by provenance tags.
Simple to explain; falls apart operationally as soon as evidence is generated continuously.

### Option B — Contract + evidence documents
Two document types:
- **Contract** (`*.c2cs-contract.yaml`) — declared claims, `forbidden`, `contract.mode`.
  Human-owned, lives in the repo, versioned with source, reviewed like code.
- **Evidence** (`*.c2cs-evidence.yaml`) — produced by exactly one producer with exactly one
  provenance source (`inferred` or `observed`), bound to an artifact digest, referencing the
  contract it speaks to. Append-only in spirit: a new analysis or observation window produces
  a new evidence document rather than mutating an old one.

Verification consumes one contract plus a set of evidence documents.

### Option C — Three fixed document types (contract / inference report / observation report)
Like B but with inference and observation as distinct schemas. Sharper, but the two evidence
kinds share almost all structure; the split doubles schema surface for little gain.

## Recommendation

**Option B.** The homogeneity rule ("one evidence document = one producer, one provenance
source") gives Option C's clarity without a third schema. The provenance model from v0.1 is
unchanged — it just stops being the only thing separating content with different owners.

The `inferred → declared` promotion becomes a concrete, reviewable act: a human lifts a claim
from an evidence document into the contract (e.g. via a generated PR), rather than a field
flipping inside one file.

## Consequences

- Schema v0.2 splits into two schemas sharing common definitions (subject, claims, provenance).
- The contract gets a stable home in the repo and a normal review workflow; evidence gets
  digest-bound artifact storage (see ADR-0004 — evidence documents are natural attestations).
- Cross-document references require stable identity — drives ADR-0005.
- Gaming vector: a stale contract with fresh evidence looks healthy if tools only check the
  evidence. Verification must always report *against which contract revision* a verdict holds.

## Deferred to PoC

Whether inference evidence should embed pointers into source (file/symbol anchors) to make
promotion-review ergonomic, and how large evidence documents get in practice.
