# C2CS Registry — version 0.1 (draft)

The registry is the **authoritative dictionary of C2CS semantics** — the same idea as
IANA's registries for MIME types and HTTP methods: `GET` is listed in a registry, and
nobody gets to change what `GET` means. It is not a database and not a schema; it is the
normative source for what every shared identifier means — the list of words the whole C2CS
ecosystem promises mean the same thing. This is what makes the semantics
*common*: the curated Tier-1 effect taxonomy, the common Tier-2 vocabulary, and the
relation vocabulary (ADR-0002, ADR-0013). It versions independently of the schema and
moves faster — adding a category or concept is a registry release, not a schema release
(ADR-0007).

**Status: draft.** Per ADR-0003, a registry entry is not complete until conformance
fixtures exercise its meaning; the fixtures ship with the conformance suite, which does not
exist yet. Until then, registry 0.1 is a draft for review, not a released vocabulary.

## Contents

| Section | Entries |
|---------|---------|
| [`effects/`](effects/) — Tier-1 categories | `network`, `filesystem`, `process`, `environment` (active) · [`candidates`](effects/candidates.md): `persistent-storage`, `ipc` (not admitted) |
| [`relations.md`](relations.md) — relation vocabulary | `c2cs.rel.uses`, `reads`, `writes`, `triggers`, `derives-from`, `processes` · not seeded: `owns` |
| [`concepts.md`](concepts.md) — Common Concepts | `c2cs.data.*` seed classifications |

## Entry formats

Every registry entry, regardless of section, shares one **common core**:

| Part | Content |
|------|---------|
| Identifier | the immutable name (ADR-0010) |
| Meaning | the one definition the identifier carries, forever |
| Status | `active` \| `deprecated` — and when deprecated, a `replacement:` pointer |
| Examples | matching and non-matching cases, exercised by fixtures when the conformance suite lands |
| Notes | informative context (e.g. extraction notes); never part of the definition |

Each section then adds the fields its kind of meaning requires:

**Tier-1 effect categories** (in [`effects/`](effects/)) add four mandatory parts — a
category missing any of them is not admissible:

1. **Event model** — what constitutes one event. Events MUST be existential statements
   ("this was seen"), never states or measurements that could contradict each other
   (ADR-0012's admission requirement). The meaning itself names *effects, not technologies
   or APIs* (ADR-0002): the test is "what happened to the world?", never "which library
   was called".
2. **Scope vocabulary (matcher grammar)** — the attributes a claim body may use and the
   matcher forms each attribute accepts (ADR-0003: minimal typed matchers, no expression
   language).
3. **Normalization** — the rules that make matching deterministic (case, separators,
   ranges). Two implementations MUST normalize identically.
4. **Observation mapping** — which observable signals confirm an event exists, and what a
   harness must instrument before it may report `status: analyzed` for the category
   (ADR-0006: an uninstrumented category is `not-analyzed`, never `[]`).

**Relations** (in [`relations.md`](relations.md)) add **domain**, **range**, and
**direction** (ADR-0013).

**Common concepts** (in [`concepts.md`](concepts.md)) add the **admission justification** —
the demonstration that at least two entirely different domains use the word with the same
meaning.

The shared core is no accident: a registry entry is an identifier bound to a meaning, and
only the *kind* of meaning varies. (The same pattern appears at the claim level in
ADR-0009 — identity plus meaning, specialized per kind.)

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

- **0.1 (draft, 2026-07-28)** — initial seed: four effect categories, six relations,
  two data classifications. Documented as considered and not admitted: two candidate
  categories (`persistent-storage`, `ipc`), one relation (`owns`), three data
  classifications (`secret`, `identity`, `configuration`).
