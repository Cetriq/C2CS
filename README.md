# C2CS — Code to Common Semantics

[![DOI](https://zenodo.org/badge/DOI/10.5281/zenodo.20780441.svg)](https://doi.org/10.5281/zenodo.20780441)

**C2CS** explores whether modern Large Language Models can infer the *semantics* of executable
software well enough to improve security decisions — in particular, to generate or refine
least-privilege access policy.

> *C2CS investigates whether LLM-based inference of high-level program semantics can produce
> least-privilege security policy that is meaningfully tighter than policy derived from low-level
> behavioral traces, without breaking intended functionality.*

The core pipeline is **Code → semantic inference → policy proposal**: policy is derived from
*inferred high-level intent* rather than from observed low-level behavior. C2CS treats this as
**best-effort probabilistic inference, not verification** — the general problem of determining
program behavior is undecidable, and the design is built around that boundary.

On top of the inference layer sit three applications:

- **ReadAhead** — an analysis pipeline that produces a risk hypothesis as decision support.
- **Executable Policy Objects (EPO)** — proposes least-privilege policy for existing enforcement
  mechanisms (SELinux, AppArmor, sandbox entitlements) rather than replacing them.
- **NOS** — a longer-horizon AI-native operating system direction, treated strictly as future work.

## Repository structure

```
C2CS/
├── README.md
├── LICENSE              # CC BY 4.0
├── paper/
│   ├── c2cs-whitepaper.md
│   ├── references.bib
│   └── figures/
├── spec/
│   ├── c2cs-schema-v0.2.md   # draft model format ("the IR") — contract/assessment/verdict
│   ├── design/               # ADRs — load-bearing design decisions (0001–0008 accepted)
│   └── examples/             # one document family: contract, assessments, verdict
├── examples/
└── experiments/
```

## Status

Working draft, in preprint/conference format. The whitepaper lives at
[`paper/c2cs-whitepaper.md`](paper/c2cs-whitepaper.md).

## Citation

If you reference this work, please cite it via its DOI:

> Törnquist, C. (2026). *C2CS: Code to Common Semantics.* Zenodo. https://doi.org/10.5281/zenodo.20780441

The DOI above is the **concept DOI** — it always resolves to the latest version. To cite a
specific version, use that version's DOI (e.g. v0.1.0: `10.5281/zenodo.20780442`). A
[`CITATION.cff`](CITATION.cff) is included so GitHub shows a "Cite this repository" button.

## License

This work is licensed under [Creative Commons Attribution 4.0 International (CC BY 4.0)](LICENSE).
