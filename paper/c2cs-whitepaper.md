# C2CS: Code to Common Semantics

## Abstract

Current cybersecurity tooling relies on signatures, heuristics, behavioral indicators, and
policy enforcement. These approaches are effective against known threats but reason about
*form*, not *purpose*: they rarely address the question of what a piece of software is
actually trying to accomplish.

Recent advances in Large Language Models (LLMs) have shown a growing ability to analyze source
code, reason about binaries, and infer plausible semantic intent from executable logic. This
paper asks a single, bounded research question:

> **Can modern LLMs infer the semantics of executable software well enough to improve
> security decisions — and specifically, to generate or refine access policy?**

We introduce **C2CS (Code to Common Semantics)**, an approach that attempts to *infer probable
semantic intentions* from observable program structure and behavior, producing a
machine-reasonable representation of likely purpose, resources, data flows, and privilege
requirements. We are explicit that this is **best-effort probabilistic inference, not
verification**: the general problem of determining program behavior is undecidable, and C2CS is
designed around that boundary rather than in denial of it.

On top of this inference layer we sketch three applications: **ReadAhead**, an analysis pipeline
that produces a risk hypothesis as *decision support* prior to or during execution; **Executable
Policy Objects (EPO)**, a layer that *proposes* least-privilege policy on top of existing
enforcement mechanisms (SELinux, AppArmor, sandbox entitlements) rather than replacing them; and
a longer-horizon **AI-native operating system (NOS)** direction, treated strictly as future work.

The central contribution is not policy enforcement, nor policy generation in general — both are
established — but the specific pipeline **Code → semantic inference → policy proposal**, where
policy is derived from *inferred high-level intent* rather than from observed low-level behavior.

---

## Thesis statement

*C2CS investigates whether LLM-based inference of high-level program semantics can produce
least-privilege security policy that is meaningfully tighter than policy derived from low-level
behavioral traces, without breaking intended functionality.*

This phrasing is deliberately falsifiable: it names a baseline (trace-derived policy), a metric
direction (tighter privilege, preserved functionality), and a mechanism (semantic inference). It
can be argued for, experimented on, and published without first building an operating system.

---

## Section X — Limitations and Theoretical Boundaries

A security mechanism that claims to know what software "intends" must first be honest about what
can and cannot be known. C2CS is built around the following limits, not in spite of them. Stating
the boundaries first is not a concession — it motivates every design choice that follows.

**Undecidability (Rice's theorem).** Any non-trivial semantic property of an arbitrary program is
undecidable in general. There is no procedure that correctly extracts the intent of all programs.
C2CS therefore does not *translate* code into semantics; it *infers a probable interpretation*.
This is precisely why the system is framed as probabilistic and best-effort: the theory forbids a
sound, complete intent extractor, so the design target is useful approximation with calibrated
uncertainty — not proof.

**The halting problem.** Whether a given execution path is even reached can be undecidable. Intent
that depends on runtime conditions (configuration, network responses, time, user input) cannot be
fully determined statically. C2CS must treat its semantic model as a hypothesis conditioned on
observable structure and the behavior seen so far, and must degrade gracefully when execution
diverges from that hypothesis.

**Obfuscation.** An adversary who knows that code is analyzed semantically will obscure intent —
through packing, encryption, indirection, and dead-code dilution — specifically to defeat
inference. Any claim that C2CS *blocks* malicious software must survive an adaptive adversary; in
the general case it will not. This is the central reason ReadAhead is positioned first as an
observer, advisor, and classifier, and only conditionally as a gatekeeper.

**Distributed and deferred intent.** Malicious behavior can be split across processes, time, and
machines so that no single analyzed unit reveals harmful intent. Each fragment may appear benign.
Semantic inference over one binary cannot, in principle, recover an intent that exists only in the
composition of many.

**Adversarial software generally.** Beyond obfuscation, software can be constructed to actively
mislead an inference model — semantic analogues of adversarial examples, trigger-based behavior,
and benign-until-activated logic. The threat model must assume the analyzed artifact may be
hostile to the analyzer itself.

Taken together, these boundaries rule out C2CS as a sound, standalone gatekeeper. They do not rule
out C2CS as a source of decision support and policy *proposals* that improve on the status quo on
average — which is the claim this paper actually makes.

---

## Section Y — Relation to Prior Work

C2CS sits adjacent to several mature fields, and its novelty is only defensible once those are
acknowledged and bounded against.

**Program analysis.** Static analysis, abstract interpretation, and taint tracking all reason
about program behavior and data flow. They are sound or conservative by construction and operate
at the level of instructions and flows. C2CS differs by targeting *high-level intent* via learned
inference, accepting unsoundness in exchange for semantic abstraction these methods do not attempt.

**Capability- and policy-based security.** SELinux, AppArmor, seccomp, Android permissions, and
Apple sandbox entitlements already govern what software *may* do. C2CS does not replace these and
makes no claim to. The open question it targets is upstream of them: *what privileges should a
program be granted, given its probable semantics?* EPO is therefore a layer that generates or
proposes policy for these existing enforcement mechanisms.

**Automatic policy generation / policy mining.** Automatic policy and profile generation already
exists: AppArmor profile generation (`aa-genprof`, `aa-logprof`), seccomp profiles synthesized
from syscall traces, firewall and network-policy synthesis, and "policy mining" in the RBAC/ABAC
literature that derives access policy from observed behavior. These are real, established
techniques, and they are the closest prior art to C2CS. Claiming that policy generation itself is
novel would be incorrect.

The specific, narrower contribution of C2CS is the *source* of the derivation. Existing methods
derive policy from **observed low-level behavior** (syscalls, logs, execution traces) — which
means they can only constrain a program to what it has already been seen to do, and inherit
whatever the observed runs happened to exercise. C2CS proposes deriving policy from **inferred
high-level semantics/intent**, before or independent of exhaustive observed behavior. If that
inference is good enough, the resulting policy could be both tighter (closer to true least
privilege) and available earlier (without a representative trace corpus). Whether that "if" holds
is exactly the empirical question the paper sets out to motivate.

**Emerging AI communication models.** Work suggesting that AI systems can exchange compressed,
non-human-readable semantic representations is relevant as a forward-looking note. The connection
is kept as speculative future work rather than load-bearing argument.
