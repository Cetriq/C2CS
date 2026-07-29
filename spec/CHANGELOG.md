# Specification changelog

Release record for the normative artifacts (see [GOVERNANCE.md](GOVERNANCE.md)).
Design decisions live as ADRs in [`design/`](design/README.md); this file records what
shipped when.

## Unreleased

- **Registry matcher fixtures (2026-07-29):** 50 (scope, event) cases across the four
  active categories (`conformance/registry/`), exercising suffix and name/IP semantics,
  `except:` subtraction, port ranges, path prefixes and `..` resolution, basename
  matching, and environment prefixes. Conformance restructured into three scoped claims:
  document-schema, registry-matcher, and verification-engine conformant.
- **Implementer package (2026-07-29):** JSON Schemas for the three document kinds
  (`schema/`, draft 2020-12, strict per version) and the first conformance fixtures
  (`conformance/`): 7 valid documents, 8 invalid documents each naming the rule it
  violates, 7 golden verification cases with expected verdicts, a fixture manifest, an
  implementer guide, and an informative structural checker. Example digests upgraded to
  full sha256 so the reference family validates strictly. Gap recorded: per-category
  matcher-grammar fixtures are the next addition; the registry stays draft until they
  exist.

- **F3 adopted (2026-07-29):** optional `rationale` on capability claims (schema v0.2
  text and contract example) — requested independently by the GRC and AI-agent
  walkthrough consumers.
- Use-case walkthroughs added (`walkthroughs/`, 2026-07-29) with the field-coverage
  audit; consumer list extended with the promotion reviewer (finding F1).
- Registry 0.1 **draft** (2026-07-28/29): four effect categories (`network`,
  `filesystem`, `process`, `environment`), six relations, two data classifications;
  documented rejections: `persistent-storage`, `ipc`, `owns`, `secret`, `identity`,
  `configuration`. Remains draft until conformance fixtures exist.
- GOVERNANCE.md and this changelog added (2026-07-29), per ADR-0007.

## Schema v0.2 — 2026-07-28

- Three document kinds (contract / assessment / verdict), implementing ADR-0001.
- Three-level subject identity and full claim references (ADR-0005); producer pinning
  (ADR-0001); three-valued Tier-1 categories as `{status, claims}` (ADR-0006).
- Normative verdict semantics: `confirmed` / `not_observed` / `drift` / `violation`,
  derived overall outcome including `undetermined` (ADR-0003/0006).
- Worked example expanded to a document family; supersedes schema v0.1.
- Design basis: ADRs 0001–0013 accepted (2026-07-28/29).

## Schema v0.1 — 2026-07-28 (superseded)

- First single-document sketch: per-claim provenance, two tiers, contract mode, verdict
  table. Superseded by v0.2 the same week; retained for history.
