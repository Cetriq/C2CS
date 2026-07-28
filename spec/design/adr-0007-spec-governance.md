# ADR-0007: Spec governance, versioning, and registry evolution

**Status:** Proposed
**Date:** 2026-07-28

## Context

A standard's users adopt its change process as much as its format. Before the first external
user, three things must be written down: who decides changes, what a version number promises,
and how the fast-moving parts (the vocabulary registry, ADR-0002) evolve without destabilizing
the slow-moving schema. This does not require a foundation on day one — but an unwritten
process reads as "the vendor can change anything anytime", which kills exactly the neutral-
layer positioning C2CS depends on ("not locked to one vendor" is a stated goal of the vision).

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
- **Versioning promise (schema):** semver where *minor = strictly additive* (documents valid
  under 0.x are valid under 0.x+1; consumers must ignore unknown fields), *major = breaking,
  with a written migration note*. Pre-1.0 is explicitly experimental, stated in the spec.
- **Registry vs schema cadence:** the vocabulary registry versions independently and faster.
  Adding a Tier-1 category or Tier-2 convention is a registry release, not a schema release.
  Registry entries are never deleted — only `deprecated` with a pointer to their replacement.
- **Staged neutrality:** a written commitment that if/when ≥2 independent implementations
  exist, governance moves to a neutral home (foundation or standards body). The spec's open
  license (CC BY, already in place) plus this commitment is the credible signal at this stage.

## Recommendation

**Option C.** It matches the "tool before standard" strategy: light enough not to slow the
spec down while it is learning from the PoC, explicit enough that an early adopter can cite
the rules. The additive-minor promise is the same IR discipline argued for in the product
discussion — ecosystem trust dies with the first silent breaking change, so the promise is
made formal here.

## Consequences

- New files: `spec/GOVERNANCE.md` (the process above) and `spec/CHANGELOG.md`; the registry
  gets its own version and changelog when ADR-0002 lands.
- The conformance test suite (ADR-0003/0006) becomes part of the release definition: a spec
  release ships with its fixtures, so "implements C2CS 0.x" is testable per version.
- Cost: RFC discipline applies to ourselves too — founder changes go through the same PR
  process, which is slower than editing, and is the point.
- Licensing split (from the product discussion) is adjacent but separate: spec and registry
  open (CC BY), reference extractor open source, commercial layer above — to be fixed in a
  LICENSE note before the first tool release.

## Deferred to PoC

Nothing — governance is deliberately the one ADR with no empirical dependency. It should be
adopted before any external contribution arrives, since process changes after contributions
raise provenance questions.
