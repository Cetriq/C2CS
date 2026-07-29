# C2CS trademark and conformance-claim policy

*Status: policy draft accompanying [ADR-0014](spec/design/adr-0014-licensing.md). The
"C2CS" mark is not yet registered; this policy states intended terms so that usage
practice is established before registration.*

The Apache 2.0 license covering this repository grants rights to the *content* — it does
not grant rights to the project's names and marks beyond ordinary descriptive use. This
policy fills that gap. What it protects is the **meaning of a conformance claim**, not
access to the text.

## 1. Descriptive use — free

Referring to C2CS to describe, discuss, criticize, teach, or state factual compatibility
requires no permission:

> "implements C2CS schema v0.2" · "exports C2CS contracts" · "compatible with C2CS"

## 2. Conformance claims — version-bound

The three conformance claims defined by the
[conformance suite](spec/conformance/README.md) may be used only when **bound to specific
versions and backed by passing results**. A valid claim states:

```
C2CS document-schema conformant
  Specification:      C2CS schema v0.2
  Conformance suite:  v0.2.0 (or commit/digest)
  Result:             all applicable fixtures passed
```

The same form applies to *registry-matcher conformant* (bound to a registry version) and
*verification-engine conformant* (bound to both). Claims against floating "latest"
fixtures are not valid conformance claims — a truthful claim must stay truthful when the
main branch moves.

## 3. Certification language — do not use

There is no C2CS certification program. Words and implications such as **"certified"**,
**"officially approved"**, or **"endorsed by C2CS"** may not be used unless and until a
formal certification program exists and grants that language explicitly.

## 4. Logos and marks — separate permission

Any C2CS logo or badge (none exist yet) will require separate written permission,
governed by this policy's successor at that time.

Questions and edge cases: open a public issue. Changes to this policy follow the
governance process ([spec/GOVERNANCE.md](spec/GOVERNANCE.md)).
