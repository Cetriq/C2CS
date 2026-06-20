# Experiments

Empirical work supporting the central thesis:

> Can LLM-based inference of high-level program semantics produce least-privilege security policy
> that is meaningfully tighter than policy derived from low-level behavioral traces, without
> breaking intended functionality?

Each experiment should compare **semantics-derived policy** (C2CS) against a **trace-derived
baseline** (e.g. seccomp/AppArmor profiles synthesized from observed syscalls), measuring:

- **Tightness** — how close the resulting policy is to true least privilege.
- **Functionality preservation** — whether intended behavior still works under the policy.
- **Availability** — whether policy can be produced without a representative trace corpus.

Document the corpus, methodology, metrics, and raw results for each run so they are reproducible.
