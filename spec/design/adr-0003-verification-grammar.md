# ADR-0003: Typed matchers and formal verification semantics

**Status:** Proposed
**Date:** 2026-07-28

## Context

In v0.1, scopes like `"any host outside *.internal.acme.example"` are prose. Verification is
the product's core promise ("does the implementation follow the declared contract?"), and if
its semantics are not defined by the spec, every `c2cs verify` implementation will disagree —
making the standard's central verb vendor-specific. Three things need formal definition:

1. **Scope expressions** — what a claim covers (hosts, paths, ports, processes).
2. **Matching semantics** — when an observed event falls under a claim (host normalization,
   path canonicalization, DNS vs IP identity, symlinks).
3. **Verdict semantics** — what "conformant" means over a set of assessments within a time
   window, per `contract.mode`.

## Options

### Option A — Ad-hoc globs, semantics by implementation
Fast to ship, guaranteed divergence. This is how formats rot.

### Option B — General expression language (CEL or similar)
Maximally expressive, but heavy to implement correctly in every consumer language, hard to
review ("what does this contract actually allow?"), and expressiveness invites contracts
nobody can audit.

### Option C — Minimal typed matchers per Tier-1 category
Each registry category (ADR-0002) defines its own small, closed matcher vocabulary:
- `network`: host patterns (exact, `*.` suffix), port sets/ranges, direction, protocol.
- `filesystem`: path prefixes (canonicalized), access modes.
- `process`: executable path/name patterns.

Deterministic, implementable in any language in a few hundred lines, and every contract stays
human-auditable. A future `expr:` escape hatch (CEL) can be added additively if real needs
outgrow the matchers.

## Recommendation

**Option C.** Verifiability beats expressiveness for a contract format — the same trade
seccomp and AppArmor made. The spec must ship with:

- A normalization appendix per category (lowercase hosts, resolved `..`/symlink policy,
  IP-vs-hostname matching rules).
- The verdict table as normative text: `confirmed` / `unexercised` / `drift` / `violation`,
  parameterized by `contract.mode`, evaluated over an explicit observation window
  (`evaluated_over: {from, to, assessments: [digests]}`) so a verdict is reproducible.
- A conformance test suite: pairs of (contract, assessments) → expected verdicts. An
  implementation is a verifier only if it passes the suite. This is what actually keeps
  implementations aligned — prose never does.

## Consequences

- The verdict, not the model, becomes the auditable output artifact: signed verdict documents
  say "artifact X conformed to contract revision Y over window Z" (composes with ADR-0004).
- Gaming vector: maximal wildcards (`*` host, `/` path prefix) make `closed` mode semantically
  empty while technically conformant. Mitigation: verifiers must compute and report a
  **specificity indicator** per claim (e.g. wildcard breadth) so an empty contract is visible
  in review and reportable in GRC output — the spec makes gaming *legible* rather than
  pretending to make it impossible.

## Deferred to PoC

Whether the matcher set is sufficient for a real .NET service's contract, and what the
practical observation-window granularity is (per CI run, per deploy, rolling).
