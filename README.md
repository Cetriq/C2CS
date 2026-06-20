# C2CS — Code to Common Semantics

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
├── examples/
└── experiments/
```

## Status

Working draft, in preprint/conference format. The whitepaper lives at
[`paper/c2cs-whitepaper.md`](paper/c2cs-whitepaper.md).

## License

This work is licensed under [Creative Commons Attribution 4.0 International (CC BY 4.0)](LICENSE).
