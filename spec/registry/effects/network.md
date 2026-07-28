# Effect category: `network`

**Status:** active (since registry 0.1)

## Meaning

The subject communicates across a network boundary: data leaves or enters the subject's
process via a network transport. The effect is the *communication attempt itself* —
regardless of library, protocol stack, or whether the peer answered.

## Event model

One event = one connection initiation (outbound) or one accepted connection (inbound) by
the subject, characterized by `(direction, peer, port)`. Existential: "an outbound
connection to db.internal.example:5432 was initiated". Retries and reconnects are separate
events; long-lived connections are one event at initiation. Connection *attempts* count —
a refused or filtered attempt is still an event (security-relevant behavior does not
require success).

## Scope vocabulary (matcher grammar)

| Attribute | Forms | Notes |
|-----------|-------|-------|
| `direction` | `outbound` \| `inbound` | required |
| `host` | exact name (`db.internal.example`) · suffix pattern (`*.internal.example`) · IP literal · `any` | required |
| `except` | list of host forms | only valid when `host` is `any` or a suffix pattern; subtracts from the scope |
| `port` | integer (`5432`) · list (`[80, 443]`) · range (`"1024-65535"`) · `any` | optional, default `any` |

## Normalization

- Host names: lowercase; trailing dot stripped; internationalized names in A-label
  (punycode) form.
- IP literals: IPv6 in RFC 5952 canonical form; IPv4 dotted-quad without leading zeros.
- **Names and addresses are distinct identities.** A claim scoped to a host name covers
  events observed under that name (e.g. via DNS correlation, SNI, or resolver
  instrumentation); it does NOT cover the bare IP the name resolves to, and vice versa.
  Resolution equivalence is deliberately out of scope — it would make matching depend on
  DNS state at verification time, breaking determinism (ADR-0003). Harnesses SHOULD report
  the name when it is knowable.
- Suffix pattern `*.X` matches any name with at least one label before `.X`; it does not
  match `X` itself.
- Port ranges are inclusive.

## Observation mapping

An event exists when the harness observes a connect/accept at the transport layer
attributable to the subject — e.g. socket connect/accept syscalls, kernel connection
events, or an instrumented network stack, with peer address, port, and (when knowable)
peer name. To report `status: analyzed`, a harness MUST have instrumented connection
initiation and acceptance for the subject's processes for the whole window.

## Examples

```yaml
# claim body (scope)
direction: outbound
host: "*.internal.example"
port: 5432
```

Matches: outbound initiation to `db.internal.example:5432`.
Does not match: `db.internal.example:5433` (port), `internal.example:5432` (suffix rule),
`10.0.0.7:5432` (name/address distinction), or an inbound connection (direction).

## Extraction notes (informative)

.NET: `HttpClient`, `Socket`, `TcpClient`, `SqlConnection` connection strings, gRPC
channels; P/Invoke to socket APIs typically forces `not-analyzed` unless resolved.
