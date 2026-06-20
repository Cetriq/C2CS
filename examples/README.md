# Examples

Illustrative, worked examples of C2CS in action. The goal is honesty about both success and
failure modes, as motivated in the whitepaper.

Candidate examples:

- **A case where ReadAhead succeeds** — a program whose high-level intent is correctly inferred
  from structure, yielding a tighter least-privilege policy than a trace-derived baseline.
- **A case where obfuscation defeats inference** — a deliberately obscured program where the
  semantic model is wrong, demonstrating the boundaries documented in the Limitations section.

Each example should state its inputs, the inferred semantics, the proposed policy, and an honest
assessment of where the inference held or broke down.
