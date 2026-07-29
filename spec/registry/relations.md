# Relation vocabulary — `c2cs.rel.*`

**Registry 0.1 seed.** Relations are Tier-2 semantic identifiers under ADR-0013: they have
meaning, no truth conditions; instances inherit the epistemic status of their document
(`c2cs.rel.reads` is a concept; `op.credit.evaluate ─reads→ Customer` is an assertion);
edges never alter what their endpoints mean; no entailment is mandated. Entries are frozen
on admission (ADR-0010).

Each entry defines meaning, **domain** — the type of node an instance may start from —
**range** — the type of node it may point to — and direction. Node types: `operation`,
`entity` (Tier-2 concepts), `claim` (Tier-1, ADR-0009).

**Rule: relations describe semantics, not implementation.** `calls`, `imports`,
`references` and their kin are code-structure facts — extractor-internal, at another level
of abstraction — and will not be admitted at any point. A relation states what the system's
*meaning* connects, not how its source files are wired.

| Relation | Domain | Range | Meaning |
|----------|--------|-------|---------|
| `c2cs.rel.uses` | operation | claim | the operation exercises the capability the claim declares — the link from intended meaning down to verifiable behavior (the `uses:` shorthand in schema v0.2) |
| `c2cs.rel.reads` | operation | entity | the operation consumes the entity's data (the `reads:` shorthand) |
| `c2cs.rel.writes` | operation | entity | the operation creates or modifies the entity's data (the `writes:` shorthand) |
| `c2cs.rel.triggers` | operation | operation | performing the source operation causes the target operation to be performed |
| `c2cs.rel.derives-from` | entity | entity | the source entity's data is computed or derived from the target's (`CreditDecision ─derives-from→ CreditHistory`) |
| `c2cs.rel.processes` | operation | entity | the operation handles the entity without asserting read/write direction — the weakest data-touch relation; prefer `reads`/`writes` when the direction is known |

All relations are directed; the table's meaning fixes the direction. Each seeded relation
passes the concepts-style admission test (two entirely different domains, same meaning):
data consumption, production, causation, and lineage read identically in banking,
healthcare, and logistics. `derives-from` sits closest to the admission line — data
lineage is cross-domain, but if real usage shows divergent readings it is the first entry
to re-examine before 1.0.

## Considered and not seeded

- `c2cs.rel.owns` — **fails the admission test as proposed.** The draft defined it as
  "aggregate root / lifecycle owner", which is Domain-Driven Design vocabulary, not a
  universal meaning; and the everyday word is ambiguous between ownership, containment,
  and reference (`Customer owns Invoice` / `Invoice belongs-to Customer` /
  `Invoice references Customer`). A relation that different domains would instantiate
  differently is exactly what the central namespace must not contain — an ambiguous seed
  burns the name forever (ADR-0010). Ownership starts its life as a vendor relation
  (`acme.rel.owns`, `ddd.rel.aggregate-root`) and may be promoted if one meaning proves
  general. Central identifiers are earned through actual usage, not by seeming reasonable
  on the drawing board.

Vendor relations live in vendor namespaces (`acme.rel.*`) and interoperate syntactically;
promotion into `c2cs.rel.*` follows ADR-0002 and freezes the identifier.

Growth rule: this vocabulary stays in single digits until promotion pressure proves what is
general (ADR-0013). Proposals arrive as vendor relations with real usage, not as additions
to this table.
