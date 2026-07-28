# Effect category: `environment`

**Status:** active (since registry 0.1)

## Meaning

The subject reads configuration from its process environment: environment variables
consumed by the subject. The effect is the read — which knobs from the outside world the
subject's behavior depends on.

## Event model

One event = one read of a named environment variable by the subject, characterized by
`(variable)`. Existential: "the subject read DB_CONNECTION". Reading a variable that is
unset still counts as an event (the dependency exists). Multiple reads of the same variable
may be reported as one event — the event is existential, not a counter.

## Scope vocabulary (matcher grammar)

| Attribute | Forms | Notes |
|-----------|-------|-------|
| `variable` | exact name (`DB_CONNECTION`) · prefix pattern (`ASPNETCORE_*`) · `any` | required |
| `except` | list of variable forms | only valid with prefix or `any`; subtracts |

## Normalization

- Variable names are matched case-sensitively, exactly as reported by the platform.
- Prefix pattern `X_*` matches any name strictly longer than `X_` that begins with `X_`.

## Observation mapping

This category's observability is its honest weak point: environment reads are typically
library calls, not kernel-visible operations, so observation requires **runtime
instrumentation** (an instrumented runtime, interposed standard library, or equivalent) —
kernel-level tracing alone cannot see them. A harness without such instrumentation MUST
report `not-analyzed` for this category; the v0.2 example's observed assessment does
exactly that. An event exists when instrumentation observes a variable lookup by the
subject, with the variable name.

This is the admission bar working as intended (ADR-0002 rule 1): the mapping is defined
and existential — it is merely more expensive to instrument than the other categories.

## Examples

```yaml
variable: ASPNETCORE_*
```

Matches: reading `ASPNETCORE_ENVIRONMENT`.
Does not match: reading `DOTNET_ROOT` (prefix), or the *presence* of a variable in the
process environment without a read (no event — presence is state, not an existential
access, and states are inadmissible under ADR-0012).

## Extraction notes (informative)

.NET: `Environment.GetEnvironmentVariable`, configuration providers
(`AddEnvironmentVariables`). Statically well-visible when names are constants; reflective
configuration binding is the common `not-analyzed` trigger.
