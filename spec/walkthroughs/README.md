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

The four walkthroughs above all *succeed*, and they all use the same example — two
methodological weaknesses called out in review. Planned next:

- **05 — a stress walkthrough that is allowed to fail.** Describe a system the model was
  not designed around (a Kubernetes operator, PostgreSQL itself, a batch/ETL processor)
  and record honestly where the model bends or breaks. The goal is a document that says
  *here is the boundary* — the walkthrough counterpart of `effects/candidates.md`.
- **A second example family in a different domain** (a CLI tool or a background job
  worker), so the walkthroughs stop being calibrated to one service shape.
