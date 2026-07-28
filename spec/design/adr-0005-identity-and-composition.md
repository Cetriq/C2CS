# ADR-0005: Globally addressable subject identity, composition-ready references

**Status:** Accepted (2026-07-28)
**Date:** 2026-07-28

> Accepted with revisions from review: retitled from "URI-based identity" — global
> *addressability* is the design principle, URI/purl merely today's best representation of
> it; the identity model made explicitly three-level (logical / version / artifact); the full
> claim reference defined as subject + contract digest + claim ID; and an immutability
> principle added. `attributed-to:` kept exactly as proposed.

## Principles

**Identity is immutable. Metadata is mutable.** Anything that serves as an address — subject
identity, claim IDs, digests — never changes meaning; anything descriptive may.

**Identities are globally addressable.** How they are *represented* (purl, OCI reference,
git remote, a future `c2cs:` scheme) may vary by subject type and era; that any subject,
contract revision, and claim can be referenced from outside its own file may not. The real
value of this ADR is exactly that: an assessment can point at a claim in a contract without
anything depending on file names or directory layout.

## The identity model

A subject has **three identities**, serving different functions:

| Level | Example | Function |
|-------|---------|----------|
| Logical | `CreditService` | what the thing *is*, stable across versions |
| Version | `2.1.0` | which release of the thing |
| Artifact | `sha256:9b1f…` | which exact bits were analyzed/observed |

A subject reference carries a logical identity and **one or more** artifact identities.
purl happens to encode the first two in one string (`pkg:nuget/CreditService@2.1.0`), which
is convenient — but the model is the three levels, not the purl syntax. Verification binds to
artifact identity; humans and tooling navigate by logical identity; version identity connects
the two over time.

## Context

Real systems are compositions: a service pulls in libraries, and the network call a
third-party library makes is observable behavior of the service — but whose *claim* is it?
v1 should not solve aggregation of claims across components. But identity and referencing are
one-way doors: if references cannot address "a claim, in a contract revision, about a
subject" unambiguously, composition can never be added later without breaking every existing
document. The document split in ADR-0001 already forces cross-document references
(assessment → contract), so identity must be settled now regardless.

## Options

### Option A — Free-form local IDs (v0.1 status quo)
`cap.net.db` is readable but only unique within one file; cross-document and cross-system
references are impossible without convention soup.

### Option B — UUIDs everywhere
Globally unique, humanly meaningless. Contracts are meant to be reviewed by humans; opaque
IDs poison the review experience and diffs.

### Option C — Layered addressing: logical subjects + local claim IDs + digest anchoring
- **Subjects** follow the three-level model above, reusing existing representations rather
  than inventing one: purl where the subject is a package, with other representations (OCI,
  git) admitted per subject type under the same addressability principle.
- **Claims** keep short, readable, document-unique IDs (`cap.net.db`). A claim ID alone is
  *not* an address — **the address is the combination**:

  ```
  subject  +  contract digest  +  claim-id
  ```

  The middle term pins which revision of the contract is meant; without it a reference is
  ambiguous the moment a contract evolves.
- **Reserved (unused in v1):** an `attributed-to:` field on claims, pointing at a subject
  identity, so a future composition layer can express "this network capability enters through
  libX" — covering libraries, plugins, sidecars, and generated code — without changing the
  claim structure.

## Recommendation

**Option C.** It keeps contracts human-reviewable (`cap.net.db` survives code review; a UUID
does not), reuses purl where SBOM tooling already speaks it — composition will eventually
join against the SBOM's component identities, so sharing representations keeps that door
open — and it costs v1 almost nothing: one identity section in the spec plus one reserved
field.

Explicitly out of scope for v1, recorded here so it is a decision and not an accident:
claim aggregation across components, transitive contract checking ("does my dependency's
contract fit inside mine?"), and system-of-systems models.

## Consequences

- Spec gains an identity section (normative per ADR-0008): the three-level subject model,
  claim-ID grammar (lowercase dotted segments), the full reference form, and the stability
  rule — renaming a claim ID within a subject's contract lineage is a breaking change,
  tracked like an API rename, because external references embed the combination.
- Assessments reference contracts by subject identity + contract digest (ties into
  ADR-0001/0004 — the same triple the verdict attestation names).
- Gaming vector: subject identity confusion — an assessment attached to a logical identity
  whose artifact digest doesn't match the bits actually observed. Verifiers must check digest
  agreement, not just logical-identity equality.

## Deferred to PoC

Which representation fits the first real subjects — a deployed internal service is not a
package, so purl may need to be joined by an OCI reference or a minimal `c2cs:` scheme for
that case (admissible without revisiting this ADR, per the addressability principle) — and
how claim-ID stability holds up under regeneration by the extractor.
