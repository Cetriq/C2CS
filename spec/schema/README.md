# C2CS JSON Schemas — schema version 0.2

Machine validation for the three document kinds (normative per ADR-0008). JSON Schema
draft 2020-12; one schema per kind plus shared definitions:

| File | Validates |
|------|-----------|
| [`c2cs-contract.schema.json`](c2cs-contract.schema.json) | `kind: contract` |
| [`c2cs-assessment.schema.json`](c2cs-assessment.schema.json) | `kind: assessment` (both inferred and observed, conditionally) |
| [`c2cs-verdict.schema.json`](c2cs-verdict.schema.json) | `kind: verdict` |
| [`c2cs-common.schema.json`](c2cs-common.schema.json) | shared `$defs` (subject, producer, digests, IDs, timestamps) |

Schemas cross-reference by `$id` (`https://c2cs.dev/schema/0.2/<name>`); load all four
into your validator's resolver/registry. Documents are YAML — parse to the JSON data
model first (YAML dates/timestamps become strings). Select the schema by the document's
`kind` field.

## What the schemas check

The **mechanically checkable subset** of the normative rules: document structure, the
ADR-0001 invariants (a contract carrying `confidence`/`evidence` is rejected; an
assessment carrying `forbidden`/`mode` is rejected), three-valued category reports and
their claims-iff-analyzed rule, per-kind claim shapes (inferred requires `confidence`,
observed requires `evidence`/`first_seen`/`last_seen`), producer pinning, claim-ID and
digest grammar, and verdict structure.

## What they deliberately do not check

- **Matcher attributes** on claim bodies (`host`, `path`, `direction`, …) are open at the
  schema level: their vocabulary, forms, and normalization are defined per category by
  the [registry](../registry/README.md) and validated at that level.
- **Cross-document semantics** — digest agreement, aggregation, verdict correctness —
  belong to the verification engine and are exercised by the
  [conformance fixtures](../conformance/README.md).

## Strictness and forward compatibility

These schemas are **strict** (`additionalProperties: false` at the document level): a 0.2
document with unknown top-level fields is invalid *as 0.2*. This coexists with
ADR-0007's "consumers must ignore unknown fields" as follows: that rule governs
*consuming* documents that declare a **newer minor** than the consumer knows (validate
with your newest schema at your own minor's strictness, ignore what the newer minor
added); it does not license unknown fields within a declared version. Schemas version
with the spec minor — 0.3 schemas will accompany schema v0.3.
