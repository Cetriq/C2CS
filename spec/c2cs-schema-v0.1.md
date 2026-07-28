# C2CS Schema v0.1 — draft for discussion

*Status: working draft. This is the first concrete sketch of the C2CS model format — the "IR"
of the system. Nothing here is stable yet; the point of v0.1 is to have something extractable,
servable over MCP, and checkable in CI to argue about.*

*The load-bearing design decisions for v0.2 are being worked as ADRs in
[`design/`](design/README.md) — notably a contract/evidence document split (ADR-0001) and
three-valued Tier-1 completeness (ADR-0006) that will change the structure sketched here.*

A C2CS model is a YAML document describing what a piece of software **does** — its operations,
data, resources, and effects — as a set of *claims*, each carrying provenance. It complements
the SBOM, which describes what software *consists of*.

Worked example: [`examples/credit-service.c2cs.yaml`](examples/credit-service.c2cs.yaml).

## Design principles

1. **Provenance per claim.** Every claim carries exactly one provenance source:
   `declared` (a human decision — normative), `inferred` (an LLM hypothesis — carries a
   confidence), or `observed` (runtime evidence — carries pointers to traces). This encodes the
   whitepaper's central boundary directly in the data model: inferring intent from arbitrary
   code is undecidable (Rice), but checking observed behavior against declared claims is not.
   The format never lets the two problems blur.

2. **Two tiers, strictly separated.** The *verifiable core* (`capabilities`, `forbidden`)
   contains claims that can be mechanically compared against observed behavior. The *semantic
   annotations* (`operations`, `data`) carry business meaning — what a capability is *for* —
   and are not mechanically verifiable. An unverifiable claim must never borrow credibility
   from the verifiable tier; tools rendering a model must keep the distinction visible.
   Annotations link *down* to the capabilities they use (`uses:`), never the reverse.

3. **Contract mode.** `contract.mode: closed` asserts a closed world: any behavior not covered
   by a declared capability is a violation (least privilege). `open` means undeclared behavior
   is merely unknown. Under `closed`, an empty capability list is itself a claim.

4. **Complement the SBOM, don't duplicate it.** Composition (components, versions, licenses)
   belongs in the SBOM; the model references it via `subject.sbom`. C2CS answers *what the
   software does*, not *what it contains*.

5. **IR discipline.** The schema is versioned via the top-level `c2cs` field. Within a minor
   version, changes are additive only. Consumers must ignore fields they do not understand.
   Ecosystem trust dies with the first silent breaking change.

## Provenance reference

| Field        | `declared`       | `inferred`               | `observed`                  |
|--------------|------------------|--------------------------|-----------------------------|
| `by`         | required (who)   | —                        | —                           |
| `date`       | required         | —                        | —                           |
| `model`      | —                | required (which LLM)     | —                           |
| `confidence` | —                | required (0–1)           | —                           |
| `status`     | —                | `pending-approval`       | —                           |
| `evidence`   | —                | optional (what it read)  | required (trace URI)        |
| `first_seen` / `last_seen` | — | —                        | required                    |

Approval of an `inferred` claim rewrites its provenance to `declared` (recording who approved
and when). Provenance is a lifecycle, not just a label: `inferred → declared` is the bootstrap
path for existing code; `observed` never becomes `declared` without a human in the loop.

`forbidden` entries are always `declared` — a prohibition is a decision, never an inference.

## Verification semantics

Verification is a comparison between the `declared` and `observed` columns of the verifiable
core. For each behavior category:

| Declared | Observed | Verdict |
|----------|----------|---------|
| yes      | yes      | **confirmed** |
| yes      | no       | **unexercised** — not a violation; the behavior simply hasn't run |
| no       | yes      | **drift** — a violation under `closed` mode, an unknown under `open` |
| forbidden| yes      | **violation** — always, regardless of mode |

`inferred` claims never participate in verdicts; they are hypotheses awaiting approval.

Each capability category maps to an observable signal (the MCP experiment harness provides
these as typed tools): `network` → egress connections; `filesystem` → file syscalls;
`process` → exec events; `environment` → variable reads.

## Deliberately out of scope for v0.1

- **Enforcement compilation** — translating a model into AppArmor/seccomp/entitlement profiles
  (the EPO layer; Phase 2).
- **Cross-service composition** — one model describes one subject; system-of-systems semantics
  come later.
- **Temporal/sequencing claims** ("A must happen before B") and argument-level data flow.
- **Machine-native dense representation** — the BabelTele-style compact encoding remains a
  research question (see `paper/notes/2026-06-20-machine-native-semantics.md`); v0.1 stays
  human-readable.

## Open questions

- Granularity of `operations`: per public endpoint, per use case, or per code unit? The
  extractor prototype should decide this empirically.
- Should `confidence` be calibrated (and how is that measured), or is it advisory only in v0.1?
- Wildcard/scope syntax for `forbidden` (`*.internal.acme.example`) needs a defined grammar
  before `c2cs verify` can be deterministic.
- How much of Tier 1 is extractable *statically* from a .NET assembly vs. requiring observation?
