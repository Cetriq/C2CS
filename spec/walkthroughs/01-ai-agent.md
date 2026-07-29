# Walkthrough 01 — AI agent via MCP

**Consumer:** a coding agent (Claude Code, Cursor, …) connected to an MCP server that
serves the C2CS model. **Scenario:** the developer asks the agent to add a feature to
`credit-service` that calls a third-party risk-scoring API.

## Steps

1. **Orient: what is this system?** The agent requests the semantic model and receives
   the contract. It reads `subject` (which service, which version) and
   `concepts.operations` — `op.customer.credit.evaluate` with its `summary`, `reads:
   [Customer, CreditHistory]`, `writes: [CreditDecision]`. One operation, clear purpose.
   *Fields: subject, operations (summary/reads/writes).*

2. **What is this system allowed to do?** The agent reads `capabilities` (two outbound
   network claims — both internal hosts — one filesystem write scope, `process: []`) and
   `mode: closed`: anything not declared is forbidden. *Fields: capability bodies, mode.*

3. **Evaluate the requested change.** A call to `api.riskvendor.example` is a new
   outbound network event. The agent checks it against the declared scopes (no match) and
   against `forbidden`: `forb.net.external` — `host: any, except: ["*.internal.acme.example"]`,
   with `rationale: Customer data must not leave the internal zone.` The agent can now
   answer *with the reason, not just the rule*: the change conflicts with a declared
   prohibition about data leaving the zone. *Fields: forbidden match + rationale.*

4. **How trustworthy is this picture?** The agent fetches the latest verdict:
   `overall.outcome: not-conformant` (there is already an undeclared telemetry
   connection!) and `coverage: environment: unknown`. The agent tempers its answer: the
   contract is currently violated by running behavior, and one category is unobserved.
   *Fields: verdict overall, drift results.*

5. **Advise.** The agent proposes: (a) the feature needs a contract change — a new
   capability claim plus an amendment or exception decision on `forb.net.external`,
   which is a human approval, not an edit the agent makes; (b) separately flags the
   existing drift to the developer. If the agent drafts the claim, it lands as an
   inferred assessment for promotion — not as a contract edit (ADR-0001 invariants).

## What worked

- The agent answered *may I* without reading any source code — the core Fas-1 pitch.
- `rationale` let the agent explain the *why*, which is what makes the answer persuasive
  to a developer rather than bureaucratic.
- Document kinds carried trust for free: the agent never confused a hypothesis with an
  approval (ADR-0001), and the verdict told it how much to trust the declared picture.

## Findings

- **Discovery is out of spec** (which contract revision is "current", where the latest
  verdict lives) — correctly a tooling/MCP-server concern (ADR-0008), but the walkthrough
  confirms the MCP server needs a "current model + latest verdict" resolution step.
- **`summary` is the only prose an agent gets per operation.** Enough for orientation;
  richer descriptions belong in Tier-2 vendor concepts, not in more spec fields. No
  change proposed.
- Feeds **F3**: the agent would also have used a `rationale` on *capabilities* ("why does
  this service talk to the audit host?") when explaining the system to the developer.
