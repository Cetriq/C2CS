# C2CS conformance fixtures — and how to build a conformant implementation

This directory is the **operative definition** of C2CS 0.2 conformance (ADR-0008: where
prose and fixtures disagree, that disagreement is a spec bug — resolved publicly, through
governance). It contains everything needed to build and test a validator or a
verification engine **without reading any reference implementation's source** — that
independence is the acceptance test for the spec itself.

Contents:

- [`manifest.yaml`](manifest.yaml) — the fixture index: valid documents, invalid
  documents (each naming the rule it violates), matcher fixtures, and verification cases
  with expected outcomes.
- [`documents/`](documents/) — document-validity fixtures.
- [`registry/`](registry/) — **matcher fixtures**, one file per Tier-1 category: pairs of
  (scope, event) with the expected match result, exercising the registry's matcher
  grammars and normalization rules (suffix and name/IP semantics, `except:` subtraction,
  port ranges, path prefixes and `..` resolution, basename matching, environment
  prefixes). These test the registry's normative matching semantics — distinct from
  document structure and from verification algebra.
- [`verification/`](verification/) — golden verification cases: a contract, one or more
  observed assessments, and the expected verdict.
- [`tools/check.py`](tools/check.py) — informative structural checker (requires `pyyaml`
  and `jsonschema`). It has no conformance authority; the fixtures do.

## How to build a conformant validator

1. **Structural validation.** Validate documents against the JSON Schemas in
   [`../schema/`](../schema/), selected by the document's `kind`. The schemas encode the
   mechanically checkable subset of the normative rules: the ADR-0001 invariants
   (contracts carry no assessment qualifiers; assessments carry no normative content),
   three-valued category reports, producer pinning, claim-ID and digest grammar, and the
   per-kind claim shapes.
2. **Registry validation** (beyond JSON Schema): matcher attributes on claim bodies are
   defined per category by the [registry](../registry/README.md) — a structural validator
   MAY skip this; a full validator checks attributes against the category's scope
   vocabulary and normalization rules.
3. **Run the document fixtures:** every file under `documents.valid` in the manifest MUST
   be accepted; every file under `documents.invalid` MUST be rejected.

## How to build a conformant verification engine

The algorithm is specified by ADR-0003 (matching, verdict semantics, determinism) and
ADR-0012 (admissibility, aggregation). In outline:

1. **Admissibility** — accept only assessments whose contract digest, subject artifact
   digests, and registry version match; excluded assessments are listed with reasons.
2. **Aggregation** — union of observed events per category; knowledge-maximal coverage
   (`analyzed` if any admissible assessment analyzed the category; an observed event
   outranks any number of empty reports). Aggregation MUST be associative, commutative,
   idempotent, and order-independent.
3. **Matching** — per category, test observed events against declared scopes using the
   registry's matcher grammar and normalization rules. Only observed assessments
   participate; inferred content never reaches verdicts.
4. **Verdicts** — `confirmed` / `not_observed` / `drift` / `violation` per the verdict
   table; category-level results for a closed contract's empty categories.
5. **Overall** — any violation, or drift under `closed`, → `not-conformant`; else any
   in-scope category `unknown` under `closed` → `undetermined`; else `conformant`.

**Run the verification cases:** for each case in the manifest, feed the contract and all
`observed*.yaml` files to the engine and compare the produced verdict against
`expected-verdict.yaml`.

**Comparison rules (normative for fixture runs):**

- `producer.tool` compares as a wildcard (your engine's name goes there); all other
  fields compare exactly.
- Array order is significant only within `matched` lists if you emit them sorted;
  fixtures list `results` entries in contract order and `matched` in evidence order —
  engines SHOULD emit the same ordering, and MUST at minimum be set-equal.
- Case 07 additionally requires **order-independence**: feeding the assessments in any
  order MUST yield the identical verdict.

## Pass criteria — three scoped conformance claims

Conformance is claimed per capability, so an implementation can truthfully support part
of the standard without claiming all of it. Claims are always bound to specific versions
— schema, registry, **and the conformance-suite version or commit digest the fixtures
were taken from** (ADR-0008/0014). A claim against floating "latest" fixtures is not a
valid claim: it would silently stop being true when the main branch moves. Valid claim
form:

```
C2CS document-schema conformant
  Specification:      C2CS schema v0.2
  Conformance suite:  v0.2.0 (or commit/digest)
  Result:             all applicable fixtures passed
```

| Claim | Requires passing |
|-------|------------------|
| **C2CS document-schema conformant** (0.2) | all document fixtures: valid accepted, invalid rejected |
| **C2CS registry-matcher conformant** (registry 0.1) | all matcher fixtures: every (scope, event) pair yields the expected result |
| **C2CS verification-engine conformant** (0.2, registry 0.1) | both of the above, plus all verification cases reproduced — an engine contains a validator and a matcher by construction |

## Status and gaps

- Matcher fixtures now cover the four active categories' grammars and normalization
  rules. Coverage will grow with implementation experience — a matcher edge case found
  by an engine becomes a fixture (and if fixtures must change, that is a spec change
  through governance).
- Expected results (verdicts and matches) are hand-computed. When a first engine exists,
  it runs these cases and every disagreement is either an engine bug or a fixture bug —
  and a fixture bug is a spec change (ADR-0008).
