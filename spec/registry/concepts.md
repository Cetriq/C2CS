# Common Concepts — `c2cs.*`

**The central vocabulary is intentionally tiny.** That is not a limitation of the draft —
it is the design principle. This document exists to create a small, stable, shared language
for the few concepts that are genuinely universal, and to keep everything else out.

Why it exists: suppose three organizations build extractors, and one emits `Customer`, one
`Client`, one `AccountHolder`. An AI consumer sees three words and cannot know whether they
mean the same thing — the graph is common in shape only. But when all three attach
`c2cs.data.personal`, every consumer knows: *this is the same concept.* The vocabulary is
the rendezvous point where "Common" in Code to *Common* Semantics becomes literal for
Tier 2, exactly as the effect categories are for Tier 1.

Why it is tiny: `Customer` is not universal — a bank, a municipality, a hospital, and a
web shop mean materially different things by it. Words like that belong in org namespaces
(`acme.Customer`, `health.Patient`), which interoperate syntactically and can be promoted
if generality is ever demonstrated (ADR-0002). Seeding generously here is how standards
drift into universal business ontologies — the known graveyard the vocabulary rules exist
to avoid. What the ecosystem gets in exchange for the restraint is trust: every `c2cs.*`
name is a kept promise (ADR-0010), like an IANA registry entry — nobody gets to change
what `GET` means.

**Admission test:** a concept enters `c2cs.*` only if **at least two entirely different
domains would use the word with the same meaning** — demonstrated, not assumed. Concepts
have meaning, not truth conditions (ADR-0009); entries are frozen on admission (ADR-0010).

## Data classifications — `c2cs.data.*`

Classifications attach to data entities (`classification:` in schema v0.2) and exist for
the GRC consumer: they let a contract state *what kind* of data an entity carries in a
vocabulary a compliance tool can rely on.

| Identifier | Meaning | Passes the admission test because |
|------------|---------|-----------------------------------|
| `c2cs.data.personal` | data relating to an identified or identifiable natural person (GDPR art. 4(1) sense) | a bank, a hospital, and a web shop all classify "data about identifiable persons" identically at this level of abstraction |
| `c2cs.data.credentials` | secrets that grant access: passwords, keys, tokens, connection strings | a leaked connection string means the same thing in every industry |

Candidates considered and **not** seeded, with reasons — the admission test working:

- `c2cs.data.secret` — fails the one-meaning test: what counts as "secret" at a defense
  contractor and at a web shop are different classification schemes, and the
  access-granting core it shares across domains is already `credentials`.
- `c2cs.data.identity` — ambiguous between identity documents, identifiers, and
  authentication identity; an ambiguous seed burns a name forever (ADR-0010).
- `c2cs.data.configuration` — configuration is a *role* data plays, not a kind of data;
  its sensitive subset is `credentials`, the rest has no common meaning to standardize.
- Finer personal-data taxonomies (special categories, health, financial) — jurisdiction-
  and framework-specific; org namespaces (`acme.data.health-se`) carry them until
  cross-framework generality is demonstrated.

## Reserved namespaces

- `c2cs.op.*` — central operation vocabulary. Reserved, empty at seed: operations are
  domain language, and no operation name has yet earned centrality.
- `c2cs.rel.*` — relation vocabulary, defined in [`relations.md`](relations.md).

An identifier under a reserved namespace that is not in the registry is invalid — vendor
extensions belong in vendor namespaces, never speculatively under `c2cs.*`.
