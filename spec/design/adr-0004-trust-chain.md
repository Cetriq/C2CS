# ADR-0004: Define C2CS as an in-toto attestation predicate

**Status:** Accepted (2026-07-28)
**Date:** 2026-07-28

> Accepted with amendments from review: "approved truth" reworded to **approved statements**
> (truth is a philosophical word; security reviewers are rightly allergic to it, and the
> whitepaper's epistemics forbid it), the identity-not-correctness principle added, predicate
> immutability made explicit, and the predicate URI namespace reserved beyond the initial
> three types.

## Principle

**Attestations bind identity to statements, not correctness to statements.**

A signature means *"Alice said this"* — never *"this is true."* This is the same boundary
the whitepaper draws between inference and verification, carried into the trust layer: C2CS
never manufactures certainty, it makes authorship and integrity checkable.

## Context

The product pitch is "approved statements about the system", but in v0.1 `provenance.by:` is
a string in a YAML file anyone can edit. Without integrity and authenticity, the trust claims
collapse: a declared claim is only normative if you can prove who declared it, and an
observation is only evidence if you can prove which harness produced it against which
artifact. Building a bespoke signing scheme is both expensive and a credibility risk —
security reviewers distrust homegrown crypto envelopes. C2CS should own **semantics**, not
cryptography.

## Options

### Option A — Bespoke signature block in the YAML
Full control, zero ecosystem, high scrutiny cost. Every consumer must implement custom
verification.

### Option B — Generic signing (DSSE/JWS envelope only)
Sound envelope, but no statement structure: nothing binds the signature to a subject artifact
digest in a standard way, so we would reinvent that binding — which is exactly what in-toto
statements already define.

### Option C — C2CS documents as in-toto attestation predicates
Predicate types under a single namespace pattern, `https://c2cs.dev/attestation/<type>/v<N>`:
- `…/contract/v1`
- `…/assessment/v1`
- `…/verdict/v1`
- **Reserved, unused in v1:** `…/registry/v1` (attesting a registry release) and
  `…/profile/v1` (attesting a conformance profile) — named now so later additions follow one
  pattern instead of accreting divergent URI schemes.

An attestation's `subject` carries the artifact digests (matching `subject.artifacts` in the
document); the C2CS document is the predicate. Signing, key management, and transparency come
from the existing ecosystem (DSSE envelopes, sigstore/cosign, Rekor), and attestations store
next to SBOMs and SLSA provenance in registries that already handle them.

## Recommendation

**Option C.** The mechanical win is inheriting a supply-chain ecosystem instead of building
one. The larger win is **positioning**: C2CS stops being "a new document format someone
invented" and becomes *another supply-chain attestation type*, completing a natural triad —

| Attestation | Answers |
|-------------|---------|
| SBOM (CycloneDX/SPDX) | what the software **consists of** |
| SLSA provenance | how the software **was built** |
| **C2CS** | what the software **does** |

"A semantic complement to the SBOM" was a metaphor; this makes it technically literal — same
store, same signing, same verification tooling. The document model from ADR-0001 maps
cleanly: contracts signed by the architect (or a release process on their behalf),
assessments by the extractor/harness identity, verdicts by the verifier.

**Immutability:** a predicate MUST NOT be modified after signing. Corrections and updates are
new attestations that supersede old ones; supersession is expressed by issuing, never by
editing. (Obvious — and worth one sentence now to prevent a mutable-attestation tool later.)

Plain unsigned YAML files remain valid *documents* — a developer runs `c2cs extract` without
ever meeting Rekor, Fulcio, or DSSE; the enterprise CI adds signing. Conformance levels
(ADR-0006) define when attestation is required. That is the adoption ladder. The trust chain
is: **who signed, over which artifact digest, against which contract revision** — all three
come free from the statement structure.

## Consequences

- The spec gains an "attestation binding" section defining the predicate types and how
  document fields map to statement subjects (normative per ADR-0008).
- GRC consumers get provenance they can actually audit ("approved by" = a verifiable
  signature, not a string).
- The signed statement/decision shape generalizes: a future policy layer (contract →
  assessment → decision) can integrate with OPA/Kyverno/Gatekeeper-style admission without
  format changes — noted as a forward possibility, not designed here.
- Cost: the PoC toolchain takes a dependency on DSSE/cosign for the signed path.
- **Messaging risk:** DSSE, in-toto, SLSA, Rekor in the pitch will read as "yet another
  supply-chain security tool". Supply chain is the *transport*; semantics is the *product* —
  that distinction must be explicit wherever C2CS is presented (website, talks, README).
- Gaming vector: a valid signature over a stale contract (see ADR-0001) — verdict
  attestations must name the contract's digest, not just its path.

## Deferred to PoC

Key distribution ergonomics for small teams (keyless sigstore vs org keys), and whether
verdict attestations should be emitted per CI run by default.
