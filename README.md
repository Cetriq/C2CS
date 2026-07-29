# C2CS — Code to Common Semantics

[![DOI](https://zenodo.org/badge/DOI/10.5281/zenodo.20780441.svg)](https://doi.org/10.5281/zenodo.20780441)

> **Software is no longer scarce. Understanding is.**

AI is beginning to produce software faster than people and organizations can understand,
verify, and govern it. The bottleneck of software engineering is moving from *writing* code
to *knowing what it does*.

**C2CS is an open, in-progress standard for describing what software does** — its
operations, data, resources, and effects — as machine-readable semantic documents whose
declared behavior can be checked against observed behavior. It is designed as a semantic
complement to the Software Bill of Materials:

| Artifact | Answers |
|----------|---------|
| SBOM (CycloneDX/SPDX) | what the software **consists of** |
| SLSA provenance | how the software **was built** |
| **C2CS** | what the software **does** |

## How it works

C2CS separates three kinds of statements into three document kinds, because they have
different owners, lifecycles, and epistemic weight:

- **Contract** — what the software *shall* do. Human-declared, lives in the repo, reviewed
  like code. Includes capabilities, prohibitions (`forbidden`), and semantic concepts
  (operations, data entities, relations).
- **Assessment** — what the software *probably* does (inferred by an extractor/LLM, with
  confidence) or *was seen* doing (observed by a runtime harness, with evidence). Machine-
  produced, digest-bound to an exact artifact.
- **Verdict** — the deterministic comparison of a contract against observed assessments:
  `confirmed`, `not_observed`, `drift`, or `violation`, with a derived overall outcome.

```
                 ┌─ extractor ─▶ inferred assessment ─▶ human review ─▶ contract
source/binary ───┤                                                        │
                 └─ runtime ───▶ observed assessment ─────▶ verifier ◀────┘
                                                               │
                                                            verdict
```

A worked example — one document family for the same service — lives in
[`spec/examples/`](spec/examples/): a
[contract](spec/examples/credit-service.c2cs-contract.yaml), an
[inferred assessment](spec/examples/credit-service.inferred.c2cs-assessment.yaml), an
[observed assessment](spec/examples/credit-service.observed.c2cs-assessment.yaml), and a
[verdict](spec/examples/credit-service.c2cs-verdict.yaml) that catches an undeclared
network connection.

Documents can be signed as in-toto attestation predicates, so C2CS statements store and
verify alongside SBOMs and SLSA provenance in existing supply-chain infrastructure.

## The honest boundary

Determining what an arbitrary program intends is undecidable (Rice's theorem) — no tool can
promise otherwise, and C2CS does not. The standard is built on splitting two problems that
are usually conflated:

- **Inferring intent from arbitrary code** is best-effort, probabilistic, and stays that
  way. C2CS uses it to *bootstrap* contract drafts that a human reviews and approves.
- **Checking observed behavior against a declared contract** is tractable engineering.
  This is what C2CS verifies — deterministically and reproducibly.

Uncertainty is represented explicitly: an unanalyzed category is `unknown`, never silently
empty, and an unknown can never make a verdict *better*.

## Design

The specification is developed decision-first. Thirteen accepted architecture decision
records in [`spec/design/`](spec/design/README.md) cover the document model, vocabulary,
verification semantics, trust chain, identity, conformance, governance, the claim model,
and semantic relations. A few of the axioms:

- *Contracts describe what shall hold. Assessments describe what was inferred or observed.*
- *A claim has truth conditions; a concept has meaning.*
- *Unknown is first-class information.*
- *Existential observations accumulate; ignorance never overrides knowledge.*
- *Confidence is a forecast of survival under authoritative review.*
- *The meaning of an existing semantic identifier never changes.*
- *Conformance is defined by artifacts, not by our tools.*

The current schema draft is [`spec/c2cs-schema-v0.2.md`](spec/c2cs-schema-v0.2.md).

## Status

Early and honest about it: this is a **pre-1.0 working draft with no implementation yet**.
What exists today is the whitepaper, the accepted design decisions, schema v0.2, and the
worked example family. What comes next, in order: the effect registry (the vocabulary that
makes the semantics *common*), conformance test fixtures, and a first extractor +
verification prototype as the spec's experiment instrument.

## Repository structure

```
C2CS/
├── README.md
├── LICENSE                    # CC BY 4.0
├── paper/
│   ├── c2cs-whitepaper.md     # the research core
│   ├── references.bib
│   ├── notes/                 # dated idea notes
│   └── figures/
├── spec/
│   ├── c2cs-schema-v0.2.md    # current schema draft
│   ├── c2cs-schema-v0.1.md    # superseded, kept for history
│   ├── design/                # ADRs 0001–0013 (all accepted) + index
│   ├── registry/              # the vocabulary: effect categories, relations, concepts
│   ├── walkthroughs/          # the five consumers reading the example documents
│   └── examples/              # one document family: contract, assessments, verdict
├── examples/                  # whitepaper case studies (planned)
└── experiments/               # research experiments (planned)
```

## Research

C2CS began as — and keeps — a research question: *can LLM-based inference of high-level
program semantics produce least-privilege security policy that is meaningfully tighter than
policy derived from low-level behavioral traces, without breaking intended functionality?*
The whitepaper at [`paper/c2cs-whitepaper.md`](paper/c2cs-whitepaper.md) develops that
question, its theoretical limits, and its relation to prior art. The standard is the
engineering consequence: the contract makes the undecidable inference problem a tractable
conformance problem, and the research continues on the inference side.

## Citation

If you reference this work, please cite it via its DOI:

> Törnquist, C. (2026). *C2CS: Code to Common Semantics.* Zenodo. https://doi.org/10.5281/zenodo.20780441

The DOI above is the **concept DOI** — it always resolves to the latest version. To cite a
specific version, use that version's DOI (e.g. v0.2.0: `10.5281/zenodo.21652846`). A
[`CITATION.cff`](CITATION.cff) is included so GitHub shows a "Cite this repository" button.

## Discussion

This is early-stage work, and the design is meant to be argued with. Issues and questions
are welcome — especially from people building extractors, verification tooling, or AI
agents that would consume semantic models. Spec changes follow the RFC process described in
[ADR-0007](spec/design/adr-0007-spec-governance.md).

## License

This work is currently licensed under
[Creative Commons Attribution 4.0 International (CC BY 4.0)](LICENSE). The licensing model
for the specification artifacts is under review
([ADR-0007](spec/design/adr-0007-spec-governance.md)) and will be settled before any
tooling release.
