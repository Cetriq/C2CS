# Central concept vocabulary — `c2cs.*`

**Registry 0.1 seed.** Tier-2 concepts have meaning, not truth conditions (ADR-0009), grow
by promotion from vendor namespaces (ADR-0002), and are frozen on admission (ADR-0010).
The central vocabulary is seeded deliberately minimal — most operation and entity naming is
domain-specific and belongs in org namespaces (`acme.*`); only what is demonstrably
universal gets a `c2cs.*` name.

## Data classifications — `c2cs.data.*`

Classifications attach to data entities (`classification:` in schema v0.2) and exist for
the GRC consumer: they let a contract state *what kind* of data an entity carries in a
vocabulary a compliance tool can rely on.

| Identifier | Meaning |
|------------|---------|
| `c2cs.data.personal` | data relating to an identified or identifiable natural person (GDPR art. 4(1) sense) |
| `c2cs.data.credentials` | secrets that grant access: passwords, keys, tokens, connection strings |

Deliberately not seeded: finer personal-data taxonomies (special categories, health,
financial). They are jurisdiction- and framework-specific; org namespaces
(`acme.data.health-se`) carry them until cross-framework generality is demonstrated.

## Reserved namespaces

- `c2cs.op.*` — central operation vocabulary. Reserved, empty at seed: operations are
  domain language, and no operation name has yet earned centrality.
- `c2cs.rel.*` — relation vocabulary, defined in [`relations.md`](relations.md).

An identifier under a reserved namespace that is not in the registry is invalid — vendor
extensions belong in vendor namespaces, never speculatively under `c2cs.*`.
