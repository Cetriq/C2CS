# Candidate categories — not admitted

Two categories were considered for registry 0.1 and **not admitted**. This file documents
why, so the decisions are visible and the admission bar (ADR-0002: no category without a
defined existential observation mapping; effects, not technologies) is demonstrably
enforced rather than assumed.

## `persistent-storage` — deferred

**Intended meaning:** the subject durably stores data outside process memory — covering
SQL Server, SQLite, Redis AOF, RocksDB alike (the effect-not-technology abstraction from
ADR-0002).

**Why not admitted:** the meaning is right but the observation mapping is not uniform. A
SQLite write is observable as `filesystem` events; a SQL Server write is observable as
`network` events to a database endpoint. There is no *single* observable signal for
"durable storage happened" — the category would either duplicate the two transport
categories or require the harness to understand database protocols (technology knowledge,
which the abstraction rule exists to keep out of Tier 1).

**Current position:** storage behavior is expressible today as `network` and `filesystem`
claims. The durable-storage *intent* ("this operation persists customer data") is Tier-2
territory — an operation concept related to a data entity via `c2cs.rel.writes`. If a
uniform observation mapping emerges (e.g. via standardized storage-layer instrumentation),
admission can be revisited without prejudice.

## `ipc` — rejected as a single category

**Intended meaning:** inter-process communication — pipes, UNIX domain sockets, shared
memory, signals, localhost HTTP.

**Why not admitted:** "IPC" fails the one-mapping test in the other direction — it is not
one effect but a bag of mechanisms with *different* observable signals and different
security meanings (a signal is not a shared-memory segment is not a named pipe). One
category would force one matcher grammar over incompatible event shapes. Localhost TCP is
already `network`; a UNIX domain socket at a path has a natural home in a future
narrow category (e.g. `unix-socket`) with a clean `(path, direction)` event model.

**Current position:** decompose. Narrow, well-mapped categories (`unix-socket`,
`shared-memory`, `signal`) can be admitted individually when the PoC shows which ones real
contracts need. A grab-bag `ipc` will not be admitted at any point — this is a standing
decision under ADR-0010's stability rule, recorded so the name is not burned by a future
imprecise entry.
