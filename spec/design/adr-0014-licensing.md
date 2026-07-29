# ADR-0014: Licensing of the specification, fixtures, tooling, and name

**Status:** Proposed
**Date:** 2026-07-29

## Context

The ADR-0007 action item, executed as a narrow survey. The repository is currently CC BY
4.0 throughout — right for the whitepaper, wrong as the sole license for a technical
standard: CC BY carries **no patent grant**, and specifications are exactly where implicit
patent exposure deters implementers. The survey questions are the five that matter:

1. What license governs the normative text, the ADRs, the registry, and the examples?
2. What license governs the JSON Schemas and conformance fixtures?
3. What license governs informative tooling and the coming extractor?
4. May external implementations reuse schemas, fixtures, and registry content in their
   own products and test suites?
5. How may the name **C2CS** and claims like "C2CS conformant" be used?

**Survey findings** (what comparable standards actually use):

| Standard | Specification | Code/tooling | Notes |
|----------|--------------|--------------|-------|
| OpenAPI | Apache 2.0 | Apache 2.0 | one license for everything |
| OpenTelemetry | Apache 2.0 | Apache 2.0 | docs under CC BY 4.0 |
| OCI (image/runtime/dist) | Apache 2.0 | Apache 2.0 | Linux Foundation |
| SPDX | Community-Spec-1.0 **+** CC-BY-3.0 | Apache/MIT | trademark policy requires reciting the spec version |
| CSL 1.0 itself | — | — | patent license to *Necessary Claims*, RAND-Z contributor commitments; designed for specs developed in git alongside test suites and reference implementations |

Two viable families emerge: **Apache 2.0 for everything** (OpenAPI/OTel/OCI — simple, one
license, express patent grant) and **CSL 1.0 for the normative text** (SPDX — more
standards-native patent machinery, but adds contributor-process weight that presumes a
governance body).

## Options

### Option A — Status quo: CC BY 4.0 for everything
Rejected: no patent grant, and it signals *article*, not *technical standard* (the
original ADR-0007 objection). Fixtures and schemas under a documentation license also
complicate embedding them in third-party test code.

### Option B — CSL 1.0 for normative text, Apache 2.0 for code artifacts
The SPDX pattern. Strongest patent posture for a multi-contributor standard — but CSL's
contributor commitments and review-period machinery presume exactly the neutral
governance body C2CS does not yet have (ADR-0007's staged neutrality). Adopting it now
buys process weight before there are contributors to bind.

### Option C — Apache 2.0 for all spec artifacts now; CSL designated as the upgrade path
- **Q1–Q3, one answer:** everything under `spec/` — normative text, ADRs, registry,
  examples, JSON Schemas, conformance fixtures, and informative tooling — plus the coming
  reference extractor, under **Apache 2.0**. One well-understood license, an express
  patent grant, and no license boundary running through the repo that implementers must
  reason about.
- **The whitepaper keeps CC BY 4.0.** It is an article; the license fits, and the Zenodo
  archives (v0.1.0–v0.2.0) were published under it.
- **Q4 — explicit yes.** Apache 2.0 already permits embedding schemas, fixtures, and
  registry content in external products and test suites (including commercial ones);
  the conformance README states this expressly so nobody has to ask a lawyer to build a
  validator.
- **Q5 — trademarks are not licenses.** A separate `TRADEMARKS.md` policy governs the
  name: free descriptive use ("compatible with C2CS", criticism, articles); the
  *conformance claims* of the three classes may be used only when scoped to schema and
  registry versions and backed by passing the current fixtures (the SPDX
  version-recitation pattern; the Kubernetes conformance-mark pattern). Business action
  item recorded, not solved here: actually registering the mark.
- **Upgrade path:** at the ADR-0007 neutrality transition, re-license the normative set
  under CSL 1.0 (or the receiving foundation's regime). Apache 2.0 → CSL is a widening
  for implementers, not a rug-pull, and SPDX demonstrates the combination.

## Recommendation

**Option C.** It answers all five questions with two licenses and one policy, matches
the largest precedent family (OpenAPI/OTel/OCI), delivers the patent grant CC BY lacks,
and defers standards-body machinery to the moment there is a standards body — consistent
with how every other institutional decision in this project has been staged (ADR-0007).

Mechanically, on acceptance: `spec/LICENSE` (Apache 2.0) + a licensing section in the
repo README and `spec/GOVERNANCE.md` updated; `paper/` keeps CC BY 4.0 via the root
LICENSE with the split documented; `TRADEMARKS.md` drafted at repo root. Nothing
normative is commercial, per ADR-0008 — this ADR fixes the *how*.

## Consequences

- Implementers can vendor schemas and fixtures into their test suites without legal
  review friction — the lowest-friction path to the independent implementations the
  neutrality trigger waits for.
- The conformance claims become the protected asset (via trademark policy) rather than
  the text (via restrictive license) — protecting exactly what has value to protect:
  the meaning of "C2CS conformant", not the ability to read the spec.
- Cost: a per-directory license split (repo root CC BY for the paper, `spec/` Apache
  2.0) must be documented clearly, or the repo looks inconsistent.
- Gaming vector: "C2CS conformant" claimed against stale fixture versions. The
  trademark policy's version-recitation requirement (claims name schema + registry
  versions) is the mitigation, same as ADR-0008's scoped conformance claims.

## Deferred

Trademark registration (business action, jurisdictions, cost); CSL re-licensing detail
at the neutrality transition; whether the whitepaper's successor publications also stay
CC BY (per-publication decision).
