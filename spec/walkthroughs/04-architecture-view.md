# Walkthrough 04 — Architecture view

**Consumer:** an architect (or the architecture tool rendering for one). **Scenario:**
render `credit-service` as part of the system landscape: what it does, what data moves,
what its effect surface is — without reading its code.

## Steps

1. **Nodes.** From the contract: the subject itself (`subject.logical`, `version`), its
   operations, its entities. From `subject.sbom`, the composition link-out — components
   stay in the SBOM, the view links rather than duplicates. *Fields: subject, sbom,
   operations, entities.*

2. **Edges.** The Tier-2 graph assembles from the relation shorthands: operation
   ─reads→ entity, ─writes→ entity, ─uses→ claim (ADR-0013's uniform edge model). The
   `uses` edges connect meaning to the verifiable effect surface — the architect sees
   *why* each capability exists. *Fields: reads/writes/uses as c2cs.rel triples.*

3. **Effect surface.** The capability claims themselves render as the subject's boundary:
   two internal network egress points, one log write scope, no subprocesses — and the
   prohibition drawn as a hard border ("nothing leaves `*.internal.acme.example`").
   `mode: closed` tells the renderer the boundary is exhaustive, not illustrative.
   *Fields: capability bodies, forbidden, mode.*

4. **Verification overlay.** The latest verdict colors the view: confirmed claims solid,
   `not_observed` dashed, the drift/violation edge red (an *undeclared* egress to the
   telemetry vendor appears on the diagram precisely because the verdict carries it),
   `environment: unknown` grey. The architecture view shows declared *and* actual in one
   picture — which is the whole point of the standard. *Fields: verdict results, drift,
   overall.coverage.*

5. **Zoom out — and hit the wall.** The architect adds `audit-service` (another subject,
   another contract) and wants the obvious edge: `credit-service`'s `cap.net.audit`
   *is* `audit-service`'s inbound. **No document can say that.** Cross-subject edges are
   composition — reserved by ADR-0005 (`attributed-to:` is for components, and even that
   is unused in v1). The tool can *infer* the link by matching host names against
   subjects, but that is a heuristic outside the standard.

## What worked

- One service renders completely from its document family: purpose, data, boundary,
  and live conformance status, with zero code access.
- The relation model's small seed was enough for a real diagram — `triggers` covers
  intra-service flows; nothing in the walkthrough wanted `owns`.
- The verification overlay is the differentiator against every hand-drawn architecture
  diagram: this one shows where reality currently disagrees.

## Findings

- **F2 (main finding): the composition wall has a waiting consumer.** System-landscape
  rendering needs cross-subject edges (egress↔ingress correspondence, at minimum). This
  is the concrete, consumer-backed requirement the future composition ADR should start
  from — exactly what ADR-0005 deferred waiting for.
- Inbound capabilities are underrepresented: the example contract declares only outbound
  claims, but a service's *served* surface (its inbound `network` claims) is what other
  subjects connect to. The `network` category supports `direction: inbound` already —
  the finding is for contract-writing guidance, not the schema.
- No new fields wanted; the walkthrough exercised the existing graph model as designed.
