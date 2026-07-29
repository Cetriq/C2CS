# C2CS Schema v0.2 — draft

*Status: working draft. Supersedes [v0.1](c2cs-schema-v0.1.md). This version implements the
accepted design decisions ADR-0001 … ADR-0008 (see [`design/`](design/README.md)); every
structural choice below traces to one of them. Pre-1.0 is experimental (ADR-0007).*

Worked example — one document family for the same service:
[contract](examples/credit-service.c2cs-contract.yaml) ·
[inferred assessment](examples/credit-service.inferred.c2cs-assessment.yaml) ·
[observed assessment](examples/credit-service.observed.c2cs-assessment.yaml) ·
[verdict](examples/credit-service.c2cs-verdict.yaml)

## Design principles

Consolidated from the accepted ADRs; each is normative.

1. **Contracts describe what shall hold. Assessments describe what was inferred or
   observed.** Normative vs descriptive; the document kind alone tells you which. (ADR-0001)
2. **Tier 1 defines observable behavior. Tier 2 defines intended meaning.** (ADR-0002)
3. **Verification SHALL be deterministic**, and the conformance test suite is the arbiter.
   (ADR-0003)
4. **Attestations bind identity to statements, not correctness to statements.** (ADR-0004)
5. **Identity is immutable; metadata is mutable. Identities are globally addressable.**
   (ADR-0005)
6. **Unknown is first-class information.** Three states of knowledge — known positive,
   known empty, unknown — never two states of presence. (ADR-0006)
7. **Conformance is defined by artifacts, not by our tools.** (ADR-0008)

## Document model

Three document kinds, discriminated by a top-level `kind` field. All share the header:

```yaml
c2cs: "0.2"          # schema version (minor = strictly additive, ADR-0007)
kind: contract       # contract | assessment | verdict
registry: "0.1"      # vocabulary registry version this document is written against
```

| Kind | Produced by | Lives in | Content |
|------|-------------|----------|---------|
| `contract` | humans | source repo, reviewed like code | declared claims, `forbidden`, mode, Tier-2 concepts |
| `assessment` | one producer, one pinned pipeline | build/observability storage, digest-bound | inferred **or** observed claims (never both) |
| `verdict` | a verification engine | audit storage | claim-by-claim verdicts + overall outcome |

**Invariants (ADR-0001):** a contract MUST NOT contain inferred or observed claims; an
assessment MUST NOT carry normative content (`forbidden`, `mode`, declared claims) — a
verifier treats any such content as a spec violation, not as input. Consumers MUST ignore
fields they do not understand (ADR-0007).

Suggested file naming: `<name>.c2cs-contract.yaml`, `<name>.<kind>.c2cs-assessment.yaml`,
`<name>.c2cs-verdict.yaml`. Nothing may depend on file names — all cross-references go
through identity (ADR-0005).

## Common structures

### Subject identity (ADR-0005)

Three levels, serving different functions:

```yaml
subject:
  logical: pkg:nuget/CreditService     # what the thing is (purl, OCI ref, …)
  version: "2.1.0"                     # which release
  artifacts:                           # which exact bits
    - path: bin/CreditService.dll
      digest: sha256:9b1f6c…
  sbom: sbom/credit-service.cdx.json   # composition lives in the SBOM (never here)
```

Verification binds to artifact digests; humans navigate by logical identity. A **full claim
reference** is `subject + contract digest + claim-id` — the digest pins the contract
revision, without which a reference is ambiguous the moment the contract evolves.

### Producer pinning (ADR-0001)

Every machine-produced document pins its full pipeline; two assessments are comparable only
if their producer blocks match:

```yaml
producer:
  tool: c2cs-dotnet/1.2.0    # extractor, harness, or verification engine
  model: claude-fable-5      # inferred assessments only
  registry: "0.1"
  schema: "0.2"
```

### Contract reference

Assessments and verdicts state which contract revision they speak to:

```yaml
contract:
  subject: pkg:nuget/CreditService@2.1.0
  digest: sha256:c0ffee…
```

## The contract document

```yaml
mode: closed         # closed = anything not declared is forbidden (least privilege)
                     # open   = undeclared behavior is unknown, not a violation
```

**Tier 1 — capabilities.** Declared claims grouped by registry category. Each claim has a
readable, document-unique ID (lowercase dotted segments), matcher attributes as defined by
the category's registry entry, and attribution:

```yaml
capabilities:
  network:
    - id: cap.net.db
      direction: outbound
      host: db.internal.acme.example
      port: 5432
      rationale: Credit evaluation reads and writes the credit database.   # optional
      by: claes@acme.example        # everything in a contract is declared, so no
      date: 2026-07-28              # provenance block — just who and when (+ optional
                                    # promoted_from, see Promotion)
```

`rationale` is optional on capability claims and, as before, expected on `forbidden`
entries. It is a qualifier (ADR-0009): human-facing justification that never affects
matching. Adopted from walkthrough finding F3 — the GRC consumer and the AI agent both
asked "why is this *allowed*", not only "why is this forbidden".

Omission rule: under `closed` mode, a registry category absent from the contract is
equivalent to "no capabilities of this category are allowed" (writing `network: []`
explicitly is encouraged for review clarity but not required). Under `open` mode, an absent
category is simply unconstrained.

**Forbidden.** Normative prohibitions — always human-declared, never inferred, with matcher
scopes (grammar owned by the category's registry entry, ADR-0003):

```yaml
forbidden:
  - id: forb.net.external
    category: network
    match: {direction: outbound, host: any, except: ["*.internal.acme.example"]}
    rationale: Customer data must not leave the internal zone.
```

**Tier 2 — semantic concepts.** Operations, data entities, and flows. Concepts link *down*
to the Tier-1 claims they use (`uses:`), never the reverse, and may use namespaced
vocabulary (`c2cs.*` central, `acme.*` vendor — ADR-0002). Concepts are not mechanically
verifiable and never participate in verdicts.

**Promotion (ADR-0001).** A claim lifted from an inferred assessment into the contract
records its origin: `promoted_from: <assessment-digest>#<claim-id>`. Promotion is a
reviewable act (typically a generated PR), not a field flipping.

## The assessment document

```yaml
assessment:
  kind: observed               # inferred | observed — exactly one per document
  window: {from: "2026-07-26T00:00Z", to: "2026-07-28T12:00Z"}   # observed only
```

**Three-valued categories (ADR-0006).** Every Tier-1 registry category MUST appear with an
explicit status — silence is a spec violation:

```yaml
capabilities:
  network:
    status: analyzed
    claims: [ … ]        # known positive
  process:
    status: analyzed
    claims: []           # known empty — analyzed, none found
  environment:
    status: not-analyzed # unknown — carries no information about the world
```

> This resolves the syntax question ADR-0006 deferred: a homogeneous
> `{status, claims}` wrapper rather than `category: unknown` as a value. One shape for all
> three states, machine-validatable (claims required iff `status: analyzed`), and extensible
> (a future `method:` or `depth:` field slots in additively). `not-analyzed ≠ []` — no
> implicit coercion in either direction, ever.

**Inferred claims** carry a confidence and optionally a source anchor for promotion review:

```yaml
- id: inf.net.db
  direction: outbound
  host: db.internal.acme.example
  port: 5432
  confidence: 0.92
  source: src/Infrastructure/CreditDb.cs#L41
```

Inferred assessments may also propose Tier-2 `concepts` (same shape as the contract's,
plus confidence). Inferred content informs human review and promotion — never verdicts.

**Observed claims** carry evidence pointers and sighting times:

```yaml
- id: obs.net.1
  direction: outbound
  host: db.internal.acme.example
  port: 5432
  evidence: c2cs-trace://run-47/net#1042
  first_seen: 2026-07-26T09:12Z
  last_seen: 2026-07-28T11:58Z
```

Producers report what they saw; **matching observations against contract claims is
exclusively the verification engine's job** (ADR-0003 — the engine is a pure function, and
producers issue no verdicts).

## The verdict document

Produced by a verification engine from exactly one contract revision plus a set of
assessments, over an explicit window, so the verdict is reproducible (ADR-0003):

```yaml
evaluated_over:
  from: 2026-07-26T00:00Z
  to: 2026-07-28T12:00Z
  assessments:
    - {digest: "sha256:aa11…", kind: observed}
```

**Verdict semantics** (normative; only observed assessments participate):

| Declared | Observed | Verdict |
|----------|----------|---------|
| yes | yes | `confirmed` |
| yes | no | `not_observed` — an absence of observation, nothing more |
| no | yes | `drift` — undeclared behavior |
| forbidden | yes | `violation` — always, regardless of mode |

Three-valued rules (ADR-0006): unknown MUST NOT satisfy a contract (never grounds a
`confirmed`), and unknown prevents a positive closed-mode verdict.

**Per-claim results** name the matched observations (full references) and a specificity
indicator, so wildcard-empty contracts are legible rather than forbidden (ADR-0003):

```yaml
results:
  claims:
    - claim: cap.net.db
      verdict: confirmed
      matched: ["sha256:aa11…#obs.net.1"]
      specificity: exact
  forbidden:
    - claim: forb.net.external
      verdict: violation
      matched: ["sha256:aa11…#obs.net.9"]
  drift:
    - observed: "sha256:aa11…#obs.net.9"
      category: network
      note: also triggers forb.net.external; reported under both headings
```

**Overall outcome**, derived — never averaged:

```yaml
overall:
  outcome: not-conformant    # conformant | not-conformant | undetermined
  coverage: {network: analyzed, filesystem: analyzed, process: analyzed, environment: unknown}
```

- any `violation`, or any `drift` under `closed` mode → `not-conformant`
- else any in-scope category unknown under `closed` mode → `undetermined`
- else → `conformant`

## Attestation binding (ADR-0004)

Signed C2CS documents are in-toto attestations: predicate types
`https://c2cs.dev/attestation/{contract,assessment,verdict}/v1` (with `registry` and
`profile` reserved), the document as predicate, `subject.artifacts` digests as statement
subjects. A predicate MUST NOT be modified after signing — supersession is by issuing new
attestations. Plain unsigned YAML remains valid everywhere; conformance class **Signed**
(ADR-0006) is what requires attestation.

## Conformance (ADR-0006, ADR-0008)

Extractor classes: **Core** (all Tier-1 categories, three-valued, valid producer pinning) →
**Semantic** (+ Tier-2 concepts with confidence) → **Signed** (+ attestation). Verification
engines: single bar — pass the conformance test suite. Conformance claims are scoped:
*"conforms to C2CS 0.2, registry 0.1"*. What is normative versus informative is defined by
ADR-0008; this schema, the registry, the verdict semantics above, and the fixtures are
normative — no tool is.

## Deliberately out of scope for v0.2

Enforcement compilation (EPO), cross-service composition and claim aggregation
(`attributed-to:` is reserved for it, ADR-0005), temporal/sequencing claims, machine-native
dense representation. Unchanged from v0.1.

## Open questions toward v0.3

- The registry draft now exists at [`registry/`](registry/README.md) (version 0.1, draft):
  four effect categories with matcher grammars, normalization rules, and observation
  mappings, plus the relation and concept vocabularies. It remains draft until conformance
  fixtures exercise each entry (ADR-0003).
- The `except:` matcher form is now defined per category in the registry; fixtures are
  what will make it normative.
- JSON Schemas now exist at [`schema/`](schema/README.md) and the first conformance
  fixtures at [`conformance/`](conformance/README.md) — 15 document fixtures and 7
  golden verification cases. Remaining: per-category matcher-grammar fixtures.
- Whether mechanically derived static findings deserve a third assessment kind admissible
  in verdicts (deferred from ADR-0003).
