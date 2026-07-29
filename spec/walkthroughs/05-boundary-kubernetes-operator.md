# Walkthrough 05 — boundary test: a Kubernetes operator

**This is a boundary test, and it is allowed to fail.** Its purpose is not to stress the
model but to map its *validity domain* — where the abstractions hold, where they thin
out, and why. It is the walkthrough counterpart of `registry/effects/candidates.md`. The
subject is deliberately something C2CS was not designed around.

**Subject:** `cert-rotation-operator`, an in-cluster operator that watches Secrets across
namespaces, rotates expiring TLS certificates, writes the new certificates back, and
restarts affected Deployments.

## The attempt

Writing the Tier-1 contract goes smoothly — suspiciously smoothly:

```yaml
mode: closed
capabilities:
  network:
    - id: cap.net.apiserver
      direction: outbound
      host: kubernetes.default.svc
      port: 443
      rationale: All operator work goes through the Kubernetes API.
  filesystem:
    - id: cap.fs.satoken
      path: /var/run/secrets/kubernetes.io/serviceaccount/
      access: read
  process: []
  environment:
    - variable: POD_NAMESPACE
```

Every category is covered, the mode is closed, every scope is exact. A harness confirms
all of it. The verdict is `conformant` — and the contract is **almost meaningless**.

## Where it breaks

**B1 — Semantic tunneling.** Everything the operator *does* — reading Secrets in every
namespace, patching Deployments, deleting Pods — is one TLS connection to one host and
port. At the OS boundary, where Tier 1 observes, a certificate rotator and a
cluster-destroying operator are *indistinguishable*: both are `outbound
kubernetes.default.svc:443, confirmed`. Drift detection cannot see "the operator started
deleting Pods today", because at the transport level nothing drifted. The verifiable core
degenerates to "talks to the API server" — true of every operator, including a malicious
one.

**B2 — The specificity indicator measures the wrong thing here.** ADR-0003's gaming
mitigation assumes scope breadth correlates with semantic breadth: a `host: any` claim is
legibly empty. Here the claim is maximally *specific* — exact host, exact port — and
still semantically near-empty. Tunneled platform APIs decorrelate scope specificity from
semantic specificity, and no current mechanism makes that legible in a verdict.

**B3 — The meaningful part lands in the unverifiable tier.** Tier 2 can say everything
that matters (`op.cert.rotate` reads/writes a `TlsCertificate` entity; concepts for
namespaces and workloads) — but Tier 2 has meaning, not truth conditions (ADR-0009), so
for this subject the *entire* semantic payload is unverifiable and the *entire*
verifiable payload is trivial. The credit-service inverted: there, Tier 1 carried the
story; here it carries almost none of it.

## What held

- The schema, identity model, and three-valued semantics all worked unchanged — nothing
  *broke mechanically*. The failure is one of *resolution*, not of structure.
- Subject identity for an in-cluster workload resolves as ADR-0005 anticipated: an OCI
  image reference plus digest binds the artifacts; the runtime identity (ServiceAccount)
  is attribution the harness uses, not a new identity level.
- `process: []` and the exact filesystem/environment scopes are genuinely meaningful —
  the OS-boundary model is not wrong here, just insufficient alone.

## The honest boundary, stated

**C2CS Tier 1 currently describes effects at the OS boundary. Subjects whose effect
surface tunnels through a platform API get a near-empty verifiable core.** This class is
large and growing: Kubernetes operators, and more generally any workload doing all of its
work through one cloud API endpoint (`*.amazonaws.com:443` covers "delete every bucket"
and "read one object" equally). For such subjects, C2CS today offers verifiable
*transport* claims plus unverifiable *semantic* description — useful, but far short of
the credit-service story.

Stated as the property it is rather than the failure it looks like: **C2CS does not
describe what a system does — it describes what is observable at the chosen effect
boundary.** Today that boundary is the OS (syscalls, connections, spawns). Kubernetes
moved this subject's effective boundary to the API server and its audit log, and nearly
all the semantics moved with it. The model behaved exactly as defined; what this test
maps is how much of a given subject's meaning lives *at* the boundary the registry
currently observes. That relativity was implicit in the registry's observation mappings
all along — this walkthrough makes it explicit.

## The direction this points (not solved here)

The fix is **not** to stretch the existing categories. It is a possible new *class* of
registry category: **platform-boundary categories**, where the "world" is a platform's
API rather than the OS. Sketch: a `kubernetes-api` category with matcher grammar over
`(verb, resource, namespace)` and observation mapping via the API server **audit log** —
which is an existing, existential event source ("this verb on this resource was
performed"), so it passes ADR-0012's admission requirement. Two honest tensions to
resolve before any such admission:

- It strains the *effects-not-technologies* rule (ADR-0002) — though arguably the
  platform API *is* the effect boundary for platform-native subjects, the way the kernel
  is for OS-native ones.
- It would largely mirror Kubernetes RBAC. That is not duplication but the whitepaper's
  own pattern: enforcement exists (RBAC), and C2CS's role is declared-versus-observed
  drift over the audit stream — the same relationship EPO has to AppArmor/seccomp.

This goes through `candidates.md` and the admission process like everything else — after
the PoC, with real usage, per the growth-restraint position.

**Warning flag, raised in review and adopted as standing guidance:** the platform list
does not end at Kubernetes — AWS, Azure, GCP, Salesforce, SAP all qualify as "effect
boundaries" by the same argument, and admitting them casually would explode Tier 1 into a
catalog of platforms, dissolving exactly the small technology-independent core that makes
the current four categories implementable everywhere. If platform-boundary categories
ever come, they come one at a time, against the full admission bar (existential
observation mapping included), with demonstrated need from real contracts — this
walkthrough shows why such a category *may* be needed, and equally why it must *earn*
its place through the same process as everything else in C2CS.

## Findings (continuing the numbering)

- **F5 — semantic tunneling:** platform-API subjects reduce Tier 1 to near-empty
  transport claims (B1). The model's honest current boundary.
- **F6 — specificity ≠ semantics under tunneling:** the ADR-0003 legibility mechanism
  does not cover this case (B2); verdicts over tunneled subjects look better than they
  are. Candidate mitigation: verdicts could flag categories where one claim matched a
  disproportionate share of all observed events — cheap, deterministic, and honest.
- **F7 — platform-boundary categories:** a possible future category class with the
  audit-log observation mapping; documented direction, deliberately not admitted.
