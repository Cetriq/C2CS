# ADR-0007: Spec governance, versioning, and registry evolution

**Status:** Accepted (2026-07-28)
**Date:** 2026-07-28

> Accepted with amendments from review: the neutrality trigger made principle-based
> (demonstrable multi-party adoption, not "≥2 implementations"), the spec license changed
> from an assumed CC BY to an open action item (specification licensing ≠ document
> licensing), "deprecated entries remain valid indefinitely" added, and the
> normative/informative principle cross-referenced to ADR-0008, which was independently
> called for in this review and already exists as proposed.

## Principle

**What this ADR sells is predictability.** The question an adopter actually asks is *"can I
build something that depends on C2CS?"* — and the answer is made of version promises,
compatibility rules, and a visible change process. Governance is the mechanism; dependability
is the product. Corollary, decided in ADR-0008: *the specification is normative; reference
implementations are informative* — the extractor's behavior never retroactively defines what
the standard means.

## Context

A standard's users adopt its change process as much as its format. Before the first external
user, three things must be written down: who decides changes, what a version number promises,
and how the fast-moving parts (the vocabulary registry, ADR-0002) evolve without destabilizing
the slow-moving schema. This does not require a foundation on day one — but an unwritten
process reads as "the vendor can change anything anytime", which kills exactly the neutral-
layer positioning C2CS depends on ("not locked to one vendor" is a stated goal of the vision).

The schema and the registry are different kinds of thing: the schema is *infrastructure* and
must move slowly; the registry is *knowledge* and will move fast. The same split recurs in
OpenTelemetry (spec vs semantic conventions), Kubernetes (API machinery vs API groups), and
IANA (protocols vs registries).

## Options

### Option A — Informal, decide later
Cheapest now; every early adopter prices in the risk, and retrofitting governance after
disputes is far harder than before them.

### Option B — Foundation/consortium from the start
Maximum neutrality signal, but heavyweight before there is a community to govern, and it
would freeze the spec at its least-informed moment.

### Option C — Written lightweight process now, staged neutrality
- **Decision process:** changes via RFC pull requests against `spec/`; a named maintainer
  group (initially the founders) decides, with rationale recorded — ADRs for design decisions,
  changelog for the rest. Public issues for objections.
- **Versioning promise (schema):** semver where **minor = strictly additive** (documents
  valid under 0.x are valid under 0.x+1; consumers must ignore unknown fields — a rule that
  sounds trivial and is not: it is what makes future evolution possible at all), *major =
  breaking, with a written migration note*. Pre-1.0 is explicitly experimental, stated in
  the spec.
- **Registry vs schema cadence:** the vocabulary registry versions independently and faster.
  Adding a Tier-1 category or Tier-2 convention is a registry release, not a schema release.
  Registry entries are never deleted — only `deprecated` with a pointer to their replacement,
  and **deprecated entries remain valid indefinitely**: a document written against any
  registry version can be interpreted forever.
- **Staged neutrality:** a written commitment that governance transitions to a neutral home
  (foundation or standards body) once there is **demonstrable multi-party adoption** — a
  principle, deliberately not a number: two implementations, one implementation with
  independent external users, or several serious consumers can each constitute an ecosystem;
  a count cannot capture that and would invite gaming or stalling on a technicality.

## Recommendation

**Option C.** It matches the "tool before standard" strategy: light enough not to slow the
spec down while it is learning from the PoC, explicit enough that an early adopter can cite
the rules. The additive-minor promise is the same IR discipline argued for in the product
discussion — ecosystem trust dies with the first silent breaking change, so the promise is
made formal here.

**Licensing — open action item, decide before the first tool release.** CC BY (the repo's
current license, right for the whitepaper) signals *article*, not *technical standard*, and
was wrongly assumed here for the spec. Specifications typically use Apache 2.0, Open Web
Foundation agreements, the Community Specification License, or bespoke specification terms —
survey what OpenAPI, OpenTelemetry, SPDX, and OCI actually use and pick accordingly. The
constraint that survives from ADR-0008 regardless of choice: everything normative is open;
patent posture should be explicit (a reason Apache-style grants are the specification norm).

## Consequences

- New files: `spec/GOVERNANCE.md` (the process above) and `spec/CHANGELOG.md`; the registry
  gets its own version and changelog when ADR-0002's registry lands.
- The conformance test suite (ADR-0003/0006) becomes part of the release definition: a spec
  release ships with its fixtures, so "implements C2CS 0.x" is testable per version.
- Cost: RFC discipline applies to ourselves too — founder changes go through the same PR
  process, which is slower than editing, and is the point.
- The ADR/changelog split holds: ADRs record architecture, the changelog records releases.
- Licensing action item above supersedes the earlier assumption in the product discussion
  that the spec simply inherits CC BY; the whitepaper keeps CC BY either way.

## Deferred to PoC

Nothing — governance is deliberately the one ADR with no empirical dependency. It should be
adopted before any external contribution arrives, since process changes after contributions
raise provenance questions.
