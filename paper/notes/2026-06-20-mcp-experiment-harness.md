# Idea note — MCP as the experiment harness (observation + action), Warp-style

*Date: 2026-06-20*
*Trigger: Warp terminal's use of MCP — a terminal/cloud agent calls typed MCP tools (databases,
GitHub, cloud consoles, internal APIs) alongside shell commands, invoking them automatically when
the workflow requires.*

## The idea

Future C2CS experiments should be built on **MCP (Model Context Protocol) as the unified surface
for both observing a program and acting on it** — the same pattern Warp uses to let a terminal
agent operate across the whole stack through typed tools.

Concretely, MCP plays **two roles** in the harness:

1. **Observation surface.** MCP servers expose typed tools over the things ReadAhead needs to see
   — filesystem access, process/exec events, syscalls, network connections, opened resources.
   The inference agent calls these tools to collect *observable behavior* of the running program.
   The same observed stream feeds the **trace-derived baseline** (the AppArmor/seccomp-style
   profile synthesized from observed syscalls that the whitepaper names as closest prior art).

2. **Action surface.** EPO *proposes* and (optionally) applies least-privilege policy through MCP
   tools that wrap the existing enforcement mechanisms — AppArmor / seccomp / sandbox
   entitlements. The full loop **Code → semantic inference → policy proposal → enforcement** runs
   over one typed interface instead of bespoke glue per mechanism.

## Why MCP, and why the Warp analogy

- **One typed interface, many backends.** Just as Warp's agent calls GitHub, dbt, or a custom
  internal service through the same MCP contract, the C2CS harness can swap observation backends
  (different OSes, sandboxes, tracers) and enforcement backends (AppArmor vs seccomp vs Apple
  entitlements) without rewriting the agent — only the MCP server behind the tool changes.
- **Agent-native invocation.** Warp's agent calls MCP tools automatically when the workflow needs
  them. ReadAhead can do the same: decide *what to observe next* and *which policy to propose* as
  tool calls, rather than running a fixed, pre-scripted trace collection. This makes the
  observation adaptive — driven by the current intent hypothesis.
- **Reproducibility.** A typed tool surface with declared inputs/outputs makes each experiment
  run scriptable and comparable: same tools, same logged calls, semantics-derived vs trace-derived
  policy measured under identical instrumentation.

## Connection to the other note

This composes with [[2026-06-20-machine-native-semantics]]: if the Common Semantics layer can be a
dense, machine-native representation, then the intent-hypothesis that drives the MCP tool calls
(and the policy proposal returned) can itself be carried in that compact form between components —
the MCP surface moves the *actions*, the machine-native representation moves the *semantics*.

## Experiment sketch

- **Setup.** Wrap observation (fs/process/syscall/net) and enforcement (AppArmor/seccomp/sandbox)
  as MCP servers. Run a corpus of programs (benign + the obfuscated/adversarial cases from
  `examples/`) under the harness.
- **Two policies per program.** (a) Semantics-derived: ReadAhead infers intent (optionally via
  limited MCP observation) and EPO proposes policy. (b) Trace-derived baseline: policy synthesized
  purely from observed MCP syscall/resource traces.
- **Measure.** Tightness (distance to true least privilege), functionality preservation (does the
  program still work under each policy), and availability (could the policy be produced without a
  representative trace corpus). These are the three metrics already named in `experiments/README`.
- **Caveat to carry.** As noted in the BabelTele note, QA-style fidelity is not policy
  correctness: a small semantic error can be a large privilege-boundary error. The MCP harness is
  exactly where that gap gets measured rather than assumed.

## Status

Captured as a note only. Not yet woven into `c2cs-whitepaper.md` or `experiments/README.md`.
