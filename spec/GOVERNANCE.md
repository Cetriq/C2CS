# C2CS Specification Governance

This document is the operative form of [ADR-0007](design/adr-0007-spec-governance.md).
What it sells is predictability: the answer to *"can I build something that depends on
C2CS?"* is made of the version promises, compatibility rules, and change process below.

It governs the **normative artifacts** defined by
[ADR-0008](design/adr-0008-normative-scope.md): the document schemas, the registry, the
verdict semantics, the conformance test suites, and the identity/attestation bindings.
Conformance is defined by artifacts, not by anyone's tools; reference implementations are
informative and have no special status.

## Roles

- **Maintainers** — currently the project founders. Maintainers decide changes and record
  the rationale. This group is listed here and changes to it are themselves governed
  changes.
- **Contributors** — anyone. Proposals and objections arrive through the public process
  below; no private channel confers standing.

## Change process

1. **Proposals are pull requests** against `spec/` (RFC style: the change plus its
   motivation). Objections and discussion happen in public issues and on the PR.
2. **Design decisions get ADRs** in `spec/design/` — context, options, recommendation,
   consequences — and are indexed with a status. Everything else (editorial work,
   registry additions, fixture changes) is recorded in [`CHANGELOG.md`](CHANGELOG.md).
3. **Registry changes** additionally pass the admission tests: a Tier-1 category needs
   its full entry contract including an existential observation mapping; a common concept
   needs the two-domain demonstration; a relation needs domain/range and the
   semantics-not-implementation rule. The reviewer's test for any change to an existing
   entry: *additive, deprecating, or redefining?* Only the first two merge.
4. **The maintainers' own changes follow the same process.** Founder edits go through
   PRs like anyone else's — slower than editing, and the point.
5. **Fixture disputes are spec disputes.** Where prose and fixtures disagree, the
   disagreement is a spec bug, resolved publicly through this process (ADR-0008). A
   defect in a reference implementation is just a bug and needs no governance.

## Version promises

**Schema** (the `c2cs:` field):

- **Minor releases are strictly additive.** A document valid under 0.x is valid under
  0.x+1. Consumers MUST ignore fields they do not understand.
- **Major releases may break**, and ship with a written migration note.
- **Pre-1.0 is experimental.** The promises above are kept within the 0.x line, but 1.0
  may consolidate; adopters of 0.x are early participants, and we say so plainly.

**Registry** (the `registry:` field) versions independently and faster:

- Adding a category, concept, or relation is a registry release, not a schema release.
- **Nothing is ever removed.** Entries are deprecated with a `replacement:` pointer, and
  deprecated entries remain valid indefinitely — a document written against any registry
  version can be interpreted forever.
- **The meaning of an existing semantic identifier never changes** (ADR-0010). Editorial
  clarification MUST NOT require existing conforming implementations to change behavior;
  where conformance fixtures would change, it is a redefinition, and redefinition means a
  new identifier. Semantic stability outranks convenience.

**Releases:** a spec release ships with its conformance fixtures, so "implements C2CS
0.x, registry z" is testable per version, and conformance claims are always scoped to
both versions.

## Neutrality commitment

Governance transitions to a neutral home (a foundation or standards body) once there is
**demonstrable multi-party adoption** — a principle, deliberately not a number: two
independent implementations, one implementation with independent external users, or
several serious consumers can each constitute an ecosystem. This commitment is written
here precisely so that early adopters do not have to price in "the vendor can change
anything anytime".

## Licensing

The repository is currently CC BY 4.0. The survey from ADR-0007's action item is done
and its recommendation is on the table as
[ADR-0014](design/adr-0014-licensing.md) (proposed): Apache 2.0 for all `spec/`
artifacts and tooling, CC BY 4.0 retained for the whitepaper, a separate trademark and
conformance-claim policy, and CSL 1.0 designated as the upgrade path at the neutrality
transition. The decision lands **before any tooling release**. Whatever the choice, the
constraint from ADR-0008 stands: everything normative is open, and the patent posture
will be explicit.
