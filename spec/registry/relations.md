# Relation vocabulary — `c2cs.rel.*`

**Registry 0.1 seed.** Relations are Tier-2 semantic identifiers under ADR-0013: they have
meaning, no truth conditions; instances inherit the epistemic status of their document;
edges never alter what their endpoints mean; no entailment is mandated. Each entry defines
meaning, **domain** (allowed source node types), **range** (allowed target node types), and
direction. Entries are frozen on admission (ADR-0010).

Node types: `operation`, `entity` (Tier-2 concepts), `claim` (Tier-1, ADR-0009).

| Relation | Domain | Range | Meaning |
|----------|--------|-------|---------|
| `c2cs.rel.uses` | operation | claim | the operation exercises the capability the claim declares — the link from intended meaning down to verifiable behavior (the `uses:` shorthand in schema v0.2) |
| `c2cs.rel.reads` | operation | entity | the operation consumes the entity's data (the `reads:` shorthand) |
| `c2cs.rel.writes` | operation | entity | the operation creates or modifies the entity's data (the `writes:` shorthand) |
| `c2cs.rel.owns` | entity | entity | the source entity is the aggregate root / lifecycle owner of the target (`Customer ─owns→ Invoice`) |
| `c2cs.rel.triggers` | operation | operation | performing the source operation causes the target operation to be performed |
| `c2cs.rel.derives-from` | entity | entity | the source entity's data is computed or derived from the target's (`CreditDecision ─derives-from→ CreditHistory`) |
| `c2cs.rel.processes` | operation | entity | the operation handles the entity without asserting read/write direction — the weakest data-touch relation; prefer `reads`/`writes` when the direction is known |

All relations are directed; the table's meaning fixes the direction. Vendor relations live
in vendor namespaces (`acme.rel.*`) and interoperate syntactically; promotion into
`c2cs.rel.*` follows ADR-0002 and freezes the identifier.

Growth rule: this vocabulary stays in single digits until promotion pressure proves what is
general (ADR-0013). Proposals arrive as vendor relations with real usage, not as additions
to this table.
