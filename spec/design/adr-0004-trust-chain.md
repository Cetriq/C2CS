# ADR-0004: Define C2CS as an in-toto attestation predicate

**Status:** Proposed
**Date:** 2026-07-28

## Context

The product pitch is "approved truth about the system", but in v0.1 `provenance.by:` is a
string in a YAML file anyone can edit. Without integrity and authenticity, the trust claims
collapse: a declared claim is only normative if you can prove who declared it, and evidence is
only evidence if you can prove which harness produced it against which artifact. Building a
bespoke signing scheme is both expensive and a credibility risk — security reviewers
distrust homegrown crypto envelopes.

## Options

### Option A — Bespoke signature block in the YAML
Full control, zero ecosystem, high scrutiny cost. Every consumer must implement custom
verification.

### Option B — Generic signing (DSSE/JWS envelope only)
Sound envelope, but no statement structure: nothing binds the signature to a subject artifact
digest in a standard way, so we would reinvent that binding — which is exactly what in-toto
statements already define.

### Option C — C2CS documents as in-toto attestation predicates
Define predicate types, e.g.:
- `https://c2cs.dev/attestation/contract/v1`
- `https://c2cs.dev/attestation/evidence/v1`
- `https://c2cs.dev/attestation/verdict/v1`

An attestation's `subject` carries the artifact digests (matching `subject.artifacts` in the
document); the C2CS document is the predicate. Signing, key management, and transparency come
from the existing ecosystem (DSSE envelopes, sigstore/cosign, Rekor), and attestations store
next to SBOMs and SLSA provenance in registries that already handle them.

## Recommendation

**Option C.** It inherits a supply-chain ecosystem instead of building one, and it makes the
positioning — *a semantic complement to the SBOM* — technically literal: C2CS attestations
sit in the same store, signed the same way, verified by the same tooling as CycloneDX/SPDX
attestations. The document model from ADR-0001 maps cleanly: contracts are signed by humans
(or a release process on their behalf), evidence by the extractor/harness identity, verdicts
by the verifier.

Plain unsigned YAML files remain valid *documents* (dev-loop ergonomics matter); conformance
levels (ADR-0006) define when attestation is required. The trust chain is: **who signed,
over which artifact digest, against which contract revision** — all three come free from the
statement structure.

## Consequences

- The spec gains an "attestation binding" section defining the predicate types and how
  document fields map to statement subjects.
- GRC consumers get provenance they can actually audit ("approved by" = a verifiable
  signature, not a string).
- Cost: the PoC toolchain takes a dependency on DSSE/cosign for the signed path.
- Gaming vector: a valid signature over a stale contract (see ADR-0001) — verdict
  attestations must name the contract's digest, not just its path.

## Deferred to PoC

Key distribution ergonomics for small teams (keyless sigstore vs org keys), and whether
verdict attestations should be emitted per CI run by default.
