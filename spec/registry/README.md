# C2CS Registry — version 0.1 (draft)

The registry is the vocabulary that makes the semantics *common*: the curated Tier-1 effect
taxonomy, the central Tier-2 concept vocabulary, and the relation vocabulary
(ADR-0002, ADR-0013). It versions independently of the schema and moves faster — adding a
category or concept is a registry release, not a schema release (ADR-0007).

**Status: draft.** Per ADR-0003, a registry entry is not complete until conformance
fixtures exercise its meaning; the fixtures ship with the conformance suite, which does not
exist yet. Until then, registry 0.1 is a draft for review, not a released vocabulary.

## Contents

| Section | Entries |
|---------|---------|
| [`effects/`](effects/) — Tier-1 categories | `network`, `filesystem`, `process`, `environment` (active) · [`candidates`](effects/candidates.md): `persistent-storage`, `ipc` (not admitted) |
| [`relations.md`](relations.md) — relation vocabulary | `c2cs.rel.uses`, `reads`, `writes`, `owns`, `triggers`, `derives-from`, `processes` |
| [`concepts.md`](concepts.md) — central Tier-2 concepts | `c2cs.data.*` seed classifications |

## Entry format (Tier-1 categories)

Each category entry is one definition with four mandatory parts — what the category means,
how claims are written, how they are normalized, and how existence is observed
(ADR-0003/0009). A category missing any part is not admissible.

1. **Meaning** — the effect on the world the category names. Categories name *effects, not
   technologies or APIs* (ADR-0002): the test is "what happened to the world?", never
   "which library was called".
2. **Event model** — what constitutes one event. Events MUST be existential statements
   ("this was seen"), never states or measurements that could contradict each other
   (ADR-0012's admission requirement).
3. **Scope vocabulary (matcher grammar)** — the attributes a claim body may use and the
   matcher forms each attribute accepts (ADR-0003: minimal typed matchers, no expression
   language).
4. **Normalization** — the rules that make matching deterministic (case, separators,
   ranges). Two implementations MUST normalize identically.
5. **Observation mapping** — which observable signals confirm an event exists, and what a
   harness must instrument before it may report `status: analyzed` for the category
   (ADR-0006: an uninstrumented category is `not-analyzed`, never `[]`).

Plus registry metadata: `status: active | deprecated` and, when deprecated, `replacement:`
(ADR-0010). Extraction notes (how a specific language/runtime maps onto the category) are
informative, never part of the definition.

## Rules (consolidated from the ADRs)

- **Identifiers are immutable.** Meaning never changes; add, deprecate, replace — never
  redefine. Deprecated entries remain valid indefinitely. (ADR-0002/0010)
- **The registry is the sole source of claim semantics.** (ADR-0009)
- **No vendor namespaces in Tier 1.** Only Tier-2 concepts and relations may use them.
  (ADR-0002)
- **Unknown namespaced names are preserved, not dropped**, by conforming tools. (ADR-0002)
- **Observation mappings MUST yield existential statements.** (ADR-0012)
- **Semantic stability outranks convenience.** A meaning that must change is a new
  identifier. (ADR-0010)

## Versioning

The registry carries its own version (documents pin it via the top-level `registry:`
field). Additions are minor releases; deprecations are minor releases; nothing is ever
removed. This changelog is the release record:

- **0.1 (draft, 2026-07-28)** — initial seed: four effect categories, seven relations,
  two data classifications. Two candidate categories documented as not admitted.
