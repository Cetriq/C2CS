# ADR-0005: URI-based identity, composition-ready references

**Status:** Proposed
**Date:** 2026-07-28

## Context

Real systems are compositions: a service pulls in libraries, and the network call a
third-party library makes is observable behavior of the service — but whose *claim* is it?
v1 should not solve aggregation of claims across components. But identity and referencing are
one-way doors: if IDs and references cannot address "a claim, in a document, about a subject"
unambiguously, composition can never be added later without breaking every existing document.
The document split in ADR-0001 already forces cross-document references (assessment → contract),
so identity must be settled now regardless.

## Options

### Option A — Free-form local IDs (v0.1 status quo)
`cap.net.db` is readable but only unique within one file; cross-document and cross-system
references are impossible without convention soup.

### Option B — UUIDs everywhere
Globally unique, humanly meaningless. Contracts are meant to be reviewed by humans; opaque
IDs poison the review experience and diffs.

### Option C — Hierarchical addressing: URI subjects + local claim IDs
- **Subjects** get URI identity, reusing existing schemes rather than inventing one:
  package-url (`pkg:nuget/CreditService@2.1.0`) where applicable, plus artifact digests for
  binding (digest identifies *the bits*, purl identifies *the thing* — both, as in the
  attestation model of ADR-0004).
- **Claims** keep short, readable, document-unique IDs (`cap.net.db`), globally addressable
  as `<subject-uri>#<claim-id>`.
- **Reserved (unused in v1):** an `attributed-to:` field on claims, pointing at a subject URI,
  so a future composition layer can express "this network capability enters through libX"
  without changing the claim structure.

## Recommendation

**Option C.** It keeps contracts human-reviewable, reuses purl (which SBOM tooling already
speaks — composition will eventually join against the SBOM's component identities, so sharing
the identifier scheme now is what keeps that door open), and it costs v1 almost nothing: one
identity section in the spec plus one reserved field.

Explicitly out of scope for v1, recorded here so it is a decision and not an accident:
claim aggregation across components, transitive contract checking ("does my dependency's
contract fit inside mine?"), and system-of-systems models.

## Consequences

- Spec gains an identity section: subject URI rules, claim-ID grammar (lowercase dotted
  segments), fragment addressing, and stability rule — renaming a claim ID is a breaking
  change to the contract, tracked like an API rename.
- Assessments reference contracts by subject URI + contract digest (ties into
  ADR-0001/0004).
- Gaming vector: subject identity confusion — an assessment attached to a subject URI whose
  digest doesn't match the artifact actually observed. Verifiers must check digest agreement,
  not just URI equality.

## Deferred to PoC

Whether purl covers the subjects we actually meet first (a deployed internal service is not a
package — likely needs a `c2cs:` URI scheme or OCI reference for that case), and how claim-ID
stability holds up under regeneration by the extractor.
