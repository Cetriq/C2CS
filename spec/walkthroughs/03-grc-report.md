# Walkthrough 03 — GRC / compliance report

**Consumer:** a compliance officer (or the GRC tool reporting to one). **Scenario:**
quarterly review of `credit-service`: what personal data does it process, is it allowed to
leave the internal zone, who decided that, and has it held in practice?

## Steps

1. **What data is here?** From the contract's `concepts.data.entities`: `Customer` and
   `CreditDecision`, both classified `c2cs.data.personal` — a registry identifier the GRC
   tool can rely on across every service it audits (the Common Concepts admission test is
   exactly for this consumer). *Fields: entities, classification.*

2. **What touches it?** Operations whose `reads`/`writes` reference those entities:
   `op.customer.credit.evaluate` reads `Customer`/`CreditHistory`, writes
   `CreditDecision`. Via `uses`, the officer sees which verifiable capabilities that
   processing exercises. *Fields: operations, uses.*

3. **What is prohibited, and why?** `forb.net.external` with its `rationale` ("customer
   data must not leave the internal zone") — the policy statement itself, in the
   artifact, not in a wiki. *Fields: forbidden, rationale.*

4. **Who decided?** Every declared claim carries `by` and `date`; `cap.net.audit` and the
   operation carry `promoted_from` — the audit trail showing they originated in an
   inferred assessment and were human-approved into the contract. At conformance class
   **Signed** (ADR-0004/0006), "who" is a verifiable signature rather than a string —
   this walkthrough is the argument for climbing that ladder. *Fields: by, date,
   promoted_from; attestation binding.*

5. **Has it held?** The latest verdict: `forb.net.external → violation`, `matched:
   ["sha256:aa11…#obs.net.9"]`. Following the reference into the observed assessment
   gives the incident's evidence: host, port, `first_seen`/`last_seen`, and the
   `evidence:` trace URI — a complete chain from policy to observed breach. The
   `coverage` table adds the honest caveat: `environment` was not observed this window.
   *Fields: verdict forbidden/drift results, matched, evidence, first_seen/last_seen,
   overall.coverage, evaluated_over (the audited period).*

6. **Report.** The quarterly report writes itself from the chain: data inventory
   (step 1–2), policy (3), accountability (4), outcome including one incident with
   evidence and one observability gap (5).

## What worked

- The full chain **policy → approval → observation → evidence** exists in the documents
  with no side channels; every arrow is a digest or an identifier reference.
- `promoted_from` turned out to be a *compliance* feature as much as a workflow one: it
  is the provenance of the decision.
- The three-valued coverage table maps directly onto what an auditor must disclose:
  verified, verified-absent, not-examined.

## Findings

- Feeds **F3**: the officer asked "why is the audit-host connection allowed?" — a
  `rationale` on capabilities (optional, additive) would complete the policy picture.
- **Unsigned documents cap the assurance level.** With plain YAML, `by:` is a claim about
  approval, not proof of it. Correct per the adoption ladder (ADR-0004), but reports
  should state the conformance class of their inputs — a reporting convention for the
  informative layer.
- The officer wants the audited *period* stated per report — `evaluated_over` provides
  it; multi-window reporting (a quarter = many verdicts) is aggregation the GRC tool
  does, and ADR-0012's algebra makes it safe.
