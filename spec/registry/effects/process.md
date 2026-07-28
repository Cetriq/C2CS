# Effect category: `process`

**Status:** active (since registry 0.1)

## Meaning

The subject creates other processes: code outside the subject's own image is started on its
behalf. The effect is the spawn itself — shelling out, forking workers, launching helpers —
regardless of the API used to do it.

## Event model

One event = one process creation by the subject, characterized by `(executable)`.
Existential: "the subject spawned /usr/bin/git". Spawn *attempts* count, whether or not the
child started successfully. Each spawn is a separate event; the child's own behavior is not
part of the event (a child's effects belong to composition — reserved, ADR-0005).

## Scope vocabulary (matcher grammar)

| Attribute | Forms | Notes |
|-----------|-------|-------|
| `executable` | absolute path prefix (`/usr/libexec/tools/`) · exact path (`/usr/bin/git`) · basename (`git`) · `any` | required |
| `except` | list of executable forms | only valid with prefix or `any`; subtracts |

Arguments and environment of the spawned process are deliberately not matchable in v0.1 —
they are unbounded and runtime-determined; scoping on them would invite prose-like matchers
(ADR-0003).

## Normalization

- Executable paths follow the `filesystem` normalization rules (absolute, resolved,
  case-sensitive, POSIX-style in v0.1).
- A basename form (`git`, no `/`) matches any executable whose final path segment equals it
  exactly.
- Events report the resolved executable path; harnesses SHOULD also report the basename.

## Observation mapping

An event exists when the harness observes a process-creation operation (fork+exec, spawn,
or equivalent) attributable to the subject, with the child's executable path. To report
`status: analyzed`, the harness MUST have instrumented process creation for all of the
subject's processes for the whole window. An explicit `claims: []` under `analyzed` is the
strong statement "no subprocesses were spawned" — the contract example's `process: []`
relies on exactly this.

## Examples

```yaml
executable: /usr/bin/git
```

Matches: spawning `/usr/bin/git` (any arguments).
Does not match: spawning `/usr/local/bin/git` (exact path), or loading a git *library*
in-process (no spawn — that is composition territory, not a process event).

## Extraction notes (informative)

.NET: `Process.Start`, `ProcessStartInfo`. Statically visible in most cases; dynamic
command construction lowers confidence rather than category coverage.
