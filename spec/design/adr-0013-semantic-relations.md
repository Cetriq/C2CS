# ADR-0013: Semantic relations

**Status:** Accepted (2026-07-28)
**Date:** 2026-07-28

> Accepted with amendments from review: the identifier/instance distinction made explicit
> (relation identifiers have meaning but no truth conditions; relation instances inherit
> the epistemic status of their document), the node-semantics invariant added (relations
> never alter what their endpoints mean), *endpoint types* renamed **domain and range**
> (established graph terminology, no RDF import), and the algorithm boundary stated: the
> standard defines relation identity, not graph algorithms.

## Principle

**Relations are vocabulary, not logic.**

C2CS expresses *that* `Customer` owns `Invoice`; it does not ship an inference engine that
derives what follows from ownership. Relations are descriptive, namespaced edges between
identities — governed exactly like every other semantic identifier — and any reasoning over
them lives in the informative layer (ADR-0008). **The standard defines relation identity,
not graph algorithms**: it delivers the graph; how a consumer traverses, queries, or reasons
over it is the consumer's business. That division of labor is the AI story — an agent asked
"which operations indirectly touch customer data?" reasons over a graph the standard
guarantees is commonly typed, without the standard ever answering the question itself.

**Identifier versus instance.** A relation *identifier* (`c2cs.rel.owns`) is a Tier-2
semantic identifier: it has meaning, no truth conditions (ADR-0009). A relation *instance*
(`Customer ─owns→ Invoice`) is an assertion in a document, and **inherits the epistemic
status of the document that contains it**: declared in a contract, hypothesis-with-
confidence in an inferred assessment (ADR-0011). Instances remain Tier 2 — they never
participate in verdicts — but the distinction is load-bearing for any future discussion of
relation provenance or evidence: the vocabulary cannot be true or false; an edge in a
document can be right or wrong.

## Context

Tier 2 currently names things: operations, entities, classifications. It cannot yet say how
they relate — `Customer` *owns* `Invoice`, `op.credit.evaluate` *triggers*
`op.audit.record`, `CreditDecision` *derives from* `CreditHistory`. For the AI-consumer use
case this is the difference between a vocabulary list and a semantic model of the system:
the exportable graph an agent actually wants has concepts and claims as nodes and relations
as edges. v0.2 already contains relations in disguise — `uses:` (concept → claim),
`reads:`/`writes:` (operation → entity), `flows` (entity → entity via operation) — each
with its own ad-hoc syntax. Left unaddressed, relation vocabulary will grow ad hoc per
extractor, which is the ADR-0002 fragmentation problem all over again.

## Options

### Option A — Free-form predicate strings
`relation: "owns"`. No interop; two extractors express the same edge differently, and the
graph is common in shape only.

### Option B — Full ontology semantics (RDF/OWL)
Adopt an ontology language with entailment, class hierarchies, and reasoning semantics.
Maximal expressiveness, but it drags every implementer into reasoner territory, and
"universal business ontology" is a known graveyard (ADR-0002 rejected it for concepts;
the argument is stronger for relations). It also violates the minimalism that made
ADR-0003 implementable in a few hundred lines.

### Option C — Namespaced relation vocabulary + typed triples
Relations become Tier-2 identifiers under the existing vocabulary regime:

- **Vocabulary.** A central relation namespace (`c2cs.rel.*`) seeded deliberately small —
  single digits: `uses`, `reads`, `writes`, `owns`, `triggers`, `derives-from`,
  `processes`. Vendor namespaces (`acme.rel.*`) extend freely and can be promoted
  (ADR-0002). Every relation entry defines its meaning, its **domain and range** (which
  node types it may connect: concept, claim, entity, operation), and its direction — and is
  frozen on admission (ADR-0010).
- **Form.** A relation instance is a typed triple over identities:
  `{from: Customer, rel: c2cs.rel.owns, to: Invoice}` — addressable identities on both
  ends (ADR-0005), a registry identifier in the middle.
- **No mandated entailment.** The standard defines no transitivity, no inheritance, no
  inference obligations. A relation entry MAY note logical properties informatively
  (e.g. "typically transitive"), but no conforming tool is required to derive anything.
- **Relations are Tier 2.** Identifiers have meaning, not truth conditions (ADR-0009);
  instances inherit their document's epistemic status (see Principle). Relations never
  participate in verdicts, may carry confidence when inferred (ADR-0011 rule 5), and follow
  the concept promotion path.
- **Invariant: relations do not alter the semantics of the connected nodes.** An edge is
  only a connection — `Customer ─owns→ Invoice` changes what neither `Customer` nor
  `Invoice` means. This is ADR-0009's sole-source rule seen from the graph: node semantics
  come from the registry, never from topology.

The existing shorthands remain: `uses:`, `reads:`, `writes:` are defined as syntactic sugar
for their `c2cs.rel.*` triples, so v0.2 documents stay valid (additive, ADR-0007) while the
graph export gets one uniform edge model.

## Recommendation

**Option C.** It answers the AI-consumer need (a real graph with typed, shared edges)
without opening the ontology front: everything hard about relations — governance,
stability, namespacing, promotion, meaning-vs-truth — is already solved by ADR-0002, 0009,
and 0010; this ADR just puts relations under those regimes. The seed set stays in single
digits until promotion pressure proves what is general, which is how the concept vocabulary
already grows.

## Consequences

- The registry gains a `relations/` section with the seeded central vocabulary; entries
  carry meaning, endpoint types, direction, `status`/`replacement` (ADR-0010).
- Schema v0.3 adds a `relations:` block to Tier-2 concepts (contracts and inferred
  assessments), and restates `uses:`/`reads:`/`writes:` as sugar.
- The graph export story becomes precise: nodes = subjects, claims, concepts; edges =
  relations + the structural references (`uses`, `promoted_from`, `attributed-to` when it
  arrives). This is the deliverable the MCP server serves — defined by the standard,
  implemented in the informative layer.
- Gaming vector: semantic overloading — a vendor relation whose name suggests a central
  meaning (`acme.rel.owns` meaning something else than `c2cs.rel.owns`). Mitigation: same
  as ADR-0002 — names are namespaced, tools display the namespace, and promotion review is
  the gate where meaning collisions are caught.

## Deferred to PoC

Which relations the .NET extractor can actually infer with useful confidence (ownership and
derivation are much harder than reads/writes); whether the seed set is right; and whether
agents consuming the graph need edge-level provenance beyond the document kind.
