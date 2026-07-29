#!/usr/bin/env python3
"""C2CS conformance fixture checker (informative tooling, ADR-0008).

Validates the fixture manifest: valid documents must pass their kind's JSON Schema,
invalid documents must fail it, and every file in a verification case must be
structurally valid. Verdict *semantics* (does the expected verdict follow from the
inputs?) are checked by running a verification engine against the cases — this script
only guards structure.

Requires: pyyaml, jsonschema (>=4.18). Run from anywhere:
    python3 check.py [--manifest path/to/manifest.yaml]
"""

import argparse
import datetime
import json
import pathlib
import sys

import yaml
from jsonschema import Draft202012Validator
from referencing import Registry, Resource

HERE = pathlib.Path(__file__).resolve().parent
CONFORMANCE = HERE.parent
SCHEMA_DIR = CONFORMANCE.parent / "schema"

SCHEMA_FILES = {
    "common": "c2cs-common.schema.json",
    "contract": "c2cs-contract.schema.json",
    "assessment": "c2cs-assessment.schema.json",
    "verdict": "c2cs-verdict.schema.json",
}


def _jsonify(node):
    """YAML loaders produce date/datetime objects; schemas speak strings."""
    if isinstance(node, dict):
        return {k: _jsonify(v) for k, v in node.items()}
    if isinstance(node, list):
        return [_jsonify(v) for v in node]
    if isinstance(node, (datetime.date, datetime.datetime)):
        return node.isoformat()
    return node


def load_yaml(path: pathlib.Path):
    return _jsonify(yaml.safe_load(path.read_text()))


def build_validators():
    schemas = {k: json.loads((SCHEMA_DIR / f).read_text()) for k, f in SCHEMA_FILES.items()}
    registry = Registry().with_resources(
        (s["$id"], Resource.from_contents(s)) for s in schemas.values()
    )
    return {
        kind: Draft202012Validator(schemas[kind], registry=registry)
        for kind in ("contract", "assessment", "verdict")
    }


def validate(doc, validators):
    kind = doc.get("kind") if isinstance(doc, dict) else None
    if kind not in validators:
        return [f"unknown or missing document kind: {kind!r}"]
    return [
        f"{'/'.join(str(p) for p in e.absolute_path) or '<root>'}: {e.message}"
        for e in validators[kind].iter_errors(doc)
    ]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--manifest", default=str(CONFORMANCE / "manifest.yaml"))
    args = ap.parse_args()
    manifest_path = pathlib.Path(args.manifest).resolve()
    base = manifest_path.parent
    manifest = load_yaml(manifest_path)
    validators = build_validators()

    passed, failed = 0, 0

    def report(ok, label, detail=""):
        nonlocal passed, failed
        passed, failed = passed + ok, failed + (not ok)
        print(f"{'PASS' if ok else 'FAIL'}  {label}" + (f"\n      {detail}" if detail else ""))

    for rel in manifest["documents"]["valid"]:
        errors = validate(load_yaml(base / rel), validators)
        report(not errors, f"valid    {rel}", "; ".join(errors[:3]))

    for entry in manifest["documents"]["invalid"]:
        rel = entry["file"]
        errors = validate(load_yaml(base / rel), validators)
        report(bool(errors), f"invalid  {rel}  [{entry['rule']}]",
               "" if errors else "expected schema rejection, document passed")

    for case in manifest["verification"]:
        case_dir = base / case["case"]
        for f in sorted(case_dir.glob("*.yaml")):
            errors = validate(load_yaml(f), validators)
            report(not errors, f"case     {f.relative_to(base)}", "; ".join(errors[:3]))

    print(f"\n{passed} passed, {failed} failed")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
