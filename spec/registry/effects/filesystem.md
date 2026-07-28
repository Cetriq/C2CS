# Effect category: `filesystem`

**Status:** active (since registry 0.1)

## Meaning

The subject touches the filesystem: contents or metadata of files or directories are read,
created, modified, removed, or executed. The effect is the access itself, at the level of
what happened to the file tree — not which I/O library performed it.

## Event model

One event = one access of a given class to a given path by the subject, characterized by
`(path, access)`. Existential: "a write to /var/log/credit-service/app.log was performed".

Access classes:
- `read` — contents or metadata read.
- `write` — creation, content/metadata modification, truncation, rename, or deletion.
- `execute` — a file executed or mapped for execution.

## Scope vocabulary (matcher grammar)

| Attribute | Forms | Notes |
|-----------|-------|-------|
| `path` | absolute path prefix (`/var/log/credit-service/`) · exact path (`/etc/hosts`) | required; trailing `/` = directory prefix scope, no trailing `/` = exact file |
| `except` | list of path forms | only valid with a prefix scope; subtracts |
| `access` | `read` \| `write` \| `execute` · list | required |

## Normalization

- Paths are absolute, `/`-separated, with `.` and `..` segments resolved; no duplicate
  separators.
- Matching is case-sensitive. (Case-insensitive filesystems are handled by the harness
  reporting the canonical-case path; the matcher never folds case.)
- Symlinks: events report the path *as resolved by the OS at access time* (the real path).
  A claim scoped to `/var/log/` covers an access performed through a symlink that resolves
  into `/var/log/`; symlink-alias equivalence beyond OS resolution is out of scope.
- Windows paths: deferred to a registry minor release — v0.1 defines POSIX-style paths
  only, and harnesses on other platforms MUST report `not-analyzed` rather than guess a
  mapping.

## Observation mapping

An event exists when the harness observes a file open/create/rename/unlink/exec (or
equivalent) attributable to the subject, with the resolved path and access class. To report
`status: analyzed`, the harness MUST have instrumented all three access classes for the
whole window; partial instrumentation (e.g. writes only) is `not-analyzed`.

## Examples

```yaml
path: /var/log/credit-service/
access: write
```

Matches: creating `/var/log/credit-service/app.log`, rotating it, deleting it.
Does not match: reading it (`access`), writing `/var/log/other/x.log` (prefix),
writing `/var/log/credit-service` the file (prefix scope covers the directory's contents).

## Extraction notes (informative)

.NET: `File`/`Directory`/`FileStream` APIs, logging framework sinks, config loading.
Statically, path values are often runtime-determined — extractors then scope to what is
provable (e.g. a constant prefix) or mark claims with lower confidence.
