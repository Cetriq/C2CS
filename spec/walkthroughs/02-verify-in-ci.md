# Walkthrough 02 — `c2cs verify` in CI

**Consumer:** a verification engine run as a CI gate. **Scenario:** the pipeline builds
`CreditService.dll`, runs the integration test suite under an observation harness, and
gates the merge on conformance to the contract in the repo.

## Steps

1. **Collect inputs.** The engine takes the contract from the repo and the observed
   assessment the harness produced for the test-run window. *Fields: kind, registry,
   contract digest reference.*

2. **Admissibility (ADR-0012).** For the assessment: does `contract.digest` match the
   contract being verified? Do `subject.artifacts` digests match the built DLL? Is the
   assessment's `registry` version compatible with the engine's? All yes → admissible.
   (An assessment from yesterday's build would fail the artifact-digest check and be
   listed as excluded, with the reason — not silently dropped.) *Fields: producer block,
   contract ref, subject digests, assessment window.*

3. **Aggregate.** One assessment this run — aggregation is trivial, but the rules still
   apply: coverage per category is taken from `status`, and `environment: not-analyzed`
   is recorded as unknown. *Fields: category status.*

4. **Match.** Per category, each observed claim is tested against declared scopes using
   the registry's matcher grammar and normalization: `obs.net.1` falls under
   `cap.net.db`'s extension (host exact, port exact); `obs.fs.1` under `cap.fs.applog`
   (prefix); `obs.net.9` (telemetry vendor) matches no capability and falls inside
   `forb.net.external`'s extension. *Fields: claim bodies, match/except forms, evidence.*

5. **Verdict.** The engine emits the verdict document: `confirmed` ×2 plus the
   category-level `process` confirmation, `not_observed` for `cap.net.audit` (the test
   suite never exercised auditing — informative, not failing), `drift` + `violation` for
   the telemetry connection, `overall.outcome: not-conformant`, coverage table included.
   *Fields: all verdict fields, specificity per claim.*

6. **Gate.** CI policy (tool policy, not spec — ADR-0008): fail the merge on
   `not-conformant`; on `undetermined`, warn and require a human ack; print `specificity`
   warnings for wildcard-broad claims. The developer sees *which* observed event broke
   *which* prohibition, with `matched` references down to the trace URI.

## What worked

- The verdict is reproducible by construction: `evaluated_over` pins the window and the
  assessment digests, so re-running the engine on the same inputs is byte-comparable.
- `not_observed` cleanly separated "behavior untested in CI" from "violation" — a CI run
  that exercises little produces an honest, non-failing verdict.
- The three-valued model kept the gate honest: had there been no violation, `environment:
  unknown` would still have capped the outcome at `undetermined` rather than a false green.

## Findings

- **Outcome→exit-code mapping is policy**, and teams will want a standard convention
  eventually — informative-layer documentation, not spec.
- **Window/test-coverage interplay:** `not_observed` in CI measures test-suite behavioral
  coverage as a side effect. Potentially valuable signal; no spec change needed.
- Confirms the ADR-0012 deferred question: a five-minute test window reporting `analyzed`
  for `network` is technically honest but weak — minimum-observation guidance belongs in
  harness conformance work, not the schema.
