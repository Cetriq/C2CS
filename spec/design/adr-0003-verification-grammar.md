# ADR-0003: Typed matchers and formal verification semantics

**Status:** Accepted (2026-07-28)
**Date:** 2026-07-28

> Accepted with amendments from review: the term *verification* defined up front, a
> determinism principle added at the top, verdict `unexercised` renamed **`not_observed`**
> (absence of observation is an epistemic state, not a test result), assessment production
> separated from verdict evaluation, matcher grammars moved into the registry, and the
> normative-scope question spun off as ADR-0008.

## Definition and principles

**Verification in C2CS means evaluating declared contracts against available assessments.**
It is not mathematical proof, not runtime enforcement, and not policy compliance in the
audit-framework sense — the word is defined here once so those debates end here.

1. **Verification SHALL be deterministic.** Two implementations given the same contract and
   the same set of assessments MUST produce the same verdicts. Everything in this ADR serves
   this sentence.
2. **The conformance test suite is the arbiter.** The suite — fixtures of (contract,
   assessments) → expected verdicts — defines conformance; an implementation is a verifier
   only if it passes it. This is what actually keeps implementations aligned (JSON Schema,
   OpenTelemetry, OCI, and OpenAPI all hold together this way — prose never does).

## Context

In v0.1, scopes like `"any host outside *.internal.acme.example"` are prose. Verification is
the product's core promise, and if its semantics are not defined by the spec, every
`c2cs verify` implementation will disagree — making the standard's central verb
vendor-specific. Even an innocent `host: "*.example.com"` hides a dozen divergences: does it
match `a.b.example.com`? `EXAMPLE.COM`? the IP behind the DNS name? a CNAME? IPv6?
Three things need formal definition:

1. **Scope expressions** — what a claim covers (hosts, paths, ports, processes).
2. **Matching semantics** — when an observed event falls under a claim (host normalization,
   path canonicalization, DNS vs IP identity, symlinks).
3. **Verdict semantics** — what "conformant" means over a set of assessments within a time
   window, per `contract.mode`.

Verification is the *third* step of a pipeline whose first two are assessment production —
and the steps must not blur:

```
extractor  → assessment (kind: inferred)   [static analysis]
runtime    → assessment (kind: observed)   [observation harness]
contract + assessments → verifier → verdicts
```

The verifier is a pure function over (contract, assessments). It produces no assessments of
its own, and assessment producers issue no verdicts.

## Options

### Option A — Ad-hoc globs, semantics by implementation
Fast to ship, guaranteed divergence. This is how formats rot.

### Option B — General expression language (CEL or similar)
Maximally expressive, but heavy to implement correctly in every consumer language, hard to
review ("what does this contract actually allow?"), and expressiveness invites contracts
nobody can audit.

### Option C — Minimal typed matchers per Tier-1 category, defined in the registry
Each registry category (ADR-0002) defines, as part of its registry entry, its own small,
closed matcher vocabulary and normalization rules:
- `network`: host patterns (exact, `*.` suffix), port sets/ranges, direction, protocol.
- `filesystem`: path prefixes (canonicalized), access modes.
- `process`: executable path/name patterns.

Deterministic, implementable in any language in a few hundred lines, and every contract stays
human-auditable. A future `expr:` escape hatch (CEL) can be added additively if real needs
outgrow the matchers.

## Recommendation

**Option C.** Verifiability beats expressiveness for a contract format — the same trade
seccomp, AppArmor, and Kubernetes selectors made: they succeeded *because* they are limited
enough for humans to understand.

**Matcher grammars live in the registry, not the verifier.** A category's registry entry
carries its matcher grammar, normalization rules, and observation mapping together (they are
one definition: what the category means, how it is written, how it is checked). Adding
`message-bus` or `cloud-storage` is then a registry release plus conformance fixtures — the
verdict engine itself is untouched. A verifier declares which registry categories (and
versions) it implements and must pass the fixtures for each.

The spec must ship with:

- A normalization appendix per category (lowercase hosts, resolved `..`/symlink policy,
  IP-vs-hostname matching rules) as part of each registry entry.
- The verdict table as normative text — `confirmed` / `not_observed` / `drift` / `violation`
  — parameterized by `contract.mode`, evaluated over an explicit observation window
  (`evaluated_over: {from, to, assessments: [digests]}`) so a verdict is reproducible.
  `not_observed` (declared, never seen in the window) is deliberately not named "untested":
  it states an absence of observation, nothing more.
- The conformance test suite (see principle 2).

Only observed assessments participate in verdicts. Inferred assessments inform human review
and promotion (ADR-0001), never verdicts.

## Consequences

- The verdict, not the model, becomes the auditable output artifact: signed verdict documents
  say "artifact X conformed to contract revision Y over window Z" (composes with ADR-0004).
- Registry entries grow three mandatory parts: meaning, matcher grammar, observation mapping.
  Registry releases carry their own conformance fixtures (ties into ADR-0007's release
  definition).
- Gaming vector: maximal wildcards (`*` host, `/` path prefix) make `closed` mode semantically
  empty while technically conformant. Mitigation: verifiers must compute and report a
  **specificity indicator** per claim (e.g. wildcard breadth) so an empty contract is visible
  in review and reportable in GRC output — the spec makes gaming *legible* rather than
  pretending to make it impossible.
- The standard is becoming two things at once — an exchange format and a reference engine.
  Which parts are normative (schema, registry, matcher grammars, verdict semantics,
  conformance suite) versus reference implementation is its own load-bearing decision:
  **spun off as ADR-0008.**

## Deferred to PoC

- Whether the matcher set is sufficient for a real .NET service's contract.
- Practical observation-window granularity (per CI run, per deploy, rolling).
- Whether *mechanically derived* static findings (e.g. an import table proving the binary
  links network APIs — no LLM involved) deserve admission to verdicts as a third assessment
  kind, or stay advisory like inference. This may become a future ADR if the PoC shows such
  findings are reliable enough to count.
