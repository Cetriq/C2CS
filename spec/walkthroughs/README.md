# Use-case walkthroughs — testing the spec on paper

The spec is written use-case first (see [`../design/README.md`](../design/README.md)):
every field must be traceable to at least one consumer, and a field no consumer needs is
cut. These walkthroughs are that rule executed: each takes one consumer through the
credit-service document family in [`../examples/`](../examples/) step by step, records
which fields the consumer touches, and reports friction and gaps as findings.

| Walkthrough | Consumer | Question |
|-------------|----------|----------|
| [01](01-ai-agent.md) | AI agent (via MCP) | "What does this system do — and may I add this change?" |
| [02](02-verify-in-ci.md) | `c2cs verify` in CI | "Does the implementation conform to the declared contract?" |
| [03](03-grc-report.md) | GRC / compliance | "What data is processed, what is forbidden, who approved it — and is it holding?" |
| [04](04-architecture-view.md) | Architecture view | "Show me the system: operations, data, effect surface." |
| [05](05-boundary-kubernetes-operator.md) | Boundary test (allowed to fail) | "Describe a Kubernetes operator — and map the model's validity domain." |

## Findings summary

**F1 — a fifth consumer exists: the promotion reviewer.** `confidence` and `source` on
inferred claims are touched by none of the four listed consumers — they exist for the
human reviewing an `inferred → declared` promotion (ADR-0001/0011). The consumer list in
the design method has been extended accordingly; without this, the coverage rule would
wrongly flag promotion-review fields as cut candidates.

**F2 — the architecture consumer is the first to hit the composition wall.** A system
view over multiple services cannot express "service A's outbound `cap.net.db` is
service B's inbound" — cross-subject edges are exactly the composition reserved by
ADR-0005 (`attributed-to:`). Known limit, now with a named consumer waiting on it; this
finding is input to the future composition ADR.

**F3 — `rationale` is only on `forbidden`, and two consumers wanted it on capabilities
too.** The GRC report wants "why is this allowed" alongside "why is this forbidden"; the
AI agent wants to explain a capability to a developer. **Adopted (2026-07-29):** optional
`rationale` on capability claims is now in schema v0.2 and the contract example — two
independent consumers asking for the same field is exactly how a field earns its place.

**F4 — no strong cut candidates.** The audit found every schema v0.2 field touched by at
least one consumer (including the promotion reviewer of F1). The closest to unused is
`kind` inside `evaluated_over.assessments` (derivable by dereferencing the digest) — kept,
because verdicts should be readable without dereferencing (ADR-0003's reproducibility
intent), but worth rechecking when fixtures exist.

**F5 — semantic tunneling (from the boundary test).** Subjects whose effect surface
tunnels through a platform API (Kubernetes operators, cloud-API-only workloads) get a
near-empty verifiable core: at the OS boundary, a certificate rotator and a
cluster-destroyer are indistinguishable. The model's honest current boundary — stated,
not fixed.

**F6 — specificity ≠ semantics under tunneling.** An exact host:port claim can be
semantically near-empty, so ADR-0003's specificity indicator misses this case. Candidate
mitigation recorded in walkthrough 05: flag categories where one claim matched a
disproportionate share of observed events.

**F7 — platform-boundary categories.** A possible future registry category class (e.g.
`kubernetes-api` over verbs/resources, observed via the audit log — an existential event
source). Documented direction; goes through the candidates process after the PoC, per
the growth-restraint position.

## Field coverage audit

Consumers: **A** = AI agent, **V** = verify/CI, **G** = GRC, **R** = architecture,
**P** = promotion reviewer (F1).

| Field | Touched by |
|-------|-----------|
| `subject.logical` / `version` / `artifacts.digest` | A V G R |
| `subject.sbom` | G R |
| `mode` | A V |
| capability claim body (id, matcher attrs) | A V G R |
| capability `by` / `date` | G P |
| `promoted_from` | G P |
| `forbidden` (match, `rationale`) | A V G |
| concepts: operations (summary, reads, writes, `uses`) | A G R |
| concepts: entities (`classification`) | A G R |
| assessment `kind` / `window` | V G |
| category `status` (three-valued) | A V G R |
| inferred `confidence` / `source` | A P |
| observed `evidence` / `first_seen` / `last_seen` | V G |
| `producer` block | V P |
| verdict `evaluated_over` | V G |
| verdict per-claim results + `matched` | V G |
| verdict `specificity` | V G |
| verdict `drift` / `forbidden` results | A V G |
| verdict `overall.outcome` / `coverage` | A V G R |

Method note: the walkthroughs are argued against the *current* documents — they are not
hypothetical personas but concrete read-throughs, and they should be re-run (cheaply)
whenever the schema changes. When the conformance fixtures exist, walkthroughs 02's steps
become executable.

## Planned

- **A second example family in a different domain** (a CLI tool or a background job
  worker), so the consumer walkthroughs stop being calibrated to one service shape.
- **Further boundary tests** in the 05 pattern: PostgreSQL itself (a general-purpose
  engine whose semantics are parameterized by its input — likely a different failure
  mode than tunneling) and a batch/ETL processor (dynamic, data-determined scopes).
