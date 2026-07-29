# C2CS reference tooling — extractor + serve (PoC)

**Informative tooling** (ADR-0008): nothing here has conformance authority; the
fixtures in [`../spec/conformance/`](../spec/conformance/) do. This directory hosts the
two reference tools: **`c2cs-extract`** (this page) and **[`c2cs-serve`](#c2cs-serve--mcp-server)**,
the MCP server that gives an AI agent the semantic model.

The extractor's single goal is to prove the chain

```
compiled .NET assembly → static IL analysis → inferred assessment
  → schema validation → promotion review / comparison with a contract
```

on real artifacts, without widening scope. Small, deterministic, boringly credible.

## Usage

```
dotnet run --project C2CS.Extractor.Cli -- <assembly.dll> \
    [--out <dir>] [--registry 0.1] [--contract <contract.yaml>]
```

Two outputs, strictly separated:

- **`assessment.c2cs.yaml`** — normative: an `inferred` assessment, valid against schema
  v0.2, containing only claims expressible in the registry matcher grammar, each with
  `confidence` and a `source` (method + IL offset).
- **`extraction-report.json`** — informative: coverage statistics, unresolved findings
  with reasons, known limitations, and spec-friction findings. Extractor problems live
  here and never leak into C2CS fields.

## The prime rule

The extractor distinguishes strictly between what is *proven in the artifact*, what is
*derivable with bounded certainty*, and what *cannot be determined* — and it never
guesses:

- `Process.Start("/usr/bin/git")` → claim, confidence 0.9 (direct constant).
- `File.WriteAllText(string.Concat("/var/log/", "app.log"), …)` → claim, confidence 0.8
  (derived constant).
- `Process.Start(configuration.ToolPath)` → **no claim** — an unresolved finding in the
  report ("argument is a method parameter — possible wrapper").
- A category whose only relevant call sites are unresolved is reported
  `status: not-analyzed`, never `claims: []` — known-empty is a claim of absence and
  must not be asserted when call sites exist that could not be scoped (ADR-0006).

Confidence tiers are fixed heuristics in the PoC, not calibrated forecasts; calibration
against verdicts (ADR-0011) is future research.

## Support surface (PoC)

| Category | Patterns |
|----------|----------|
| `process` | `Process.Start(string…)`, `ProcessStartInfo(string…)` / `.FileName` |
| `environment` | `Environment.GetEnvironmentVariable` |
| `filesystem` | `File.*` / `Directory.*` with constant paths (access class per method) |
| `network` | `HttpClient` verb methods and `HttpRequestMessage` with constant URIs, `TcpClient(host, port)`, `*SqlConnection(connectionString)` (simple-name heuristic) |

Value resolution: linear intraprocedural IL tracking (constants through stack and
locals, `string.Concat` of constants). No interprocedural analysis, no branch-merge
resolution, no AI interpretation — by design, in this version.

## Architecture

```
C2CS.Extractor.Core     model types + assessment (YAML) and report (JSON) writers
C2CS.Extractor.DotNet   Mono.Cecil scanner: pattern table + linear stack simulator
C2CS.Extractor.Cli      c2cs-extract entry point
C2CS.Extractor.Tests    xunit suite over the fixture assembly (the ten PoC cases)
fixtures/               C2CS.Extractor.Fixtures — the ten cases as compiled code
```

Flow: assembly → IL/metadata scan → candidate effects → constant resolution →
resolved claims / unresolved findings → dedup + confidence → assessment → validation.

## Spec findings (the PoC as experiment instrument)

- **E1 — bootstrap contract reference.** Schema v0.2 requires a contract reference on
  every assessment, but the bootstrap scenario (extracting in order to *create* the
  first contract) has none. The PoC emits a documented sentinel
  (`bootstrap:no-contract`, all-zero digest). Candidate v0.3 change: contract reference
  optional on inferred assessments.
- **E2 — `producer.model` on mechanical analysis.** The schema requires `model` on
  inferred assessments; this producer is static analysis with no model
  (`static-il-analysis` is emitted). Supports the deferred ADR-0003 question of a third
  assessment kind for mechanically derived findings.

## Definition-of-done status

- ✅ analyzes a real external .NET assembly (Mono.Cecil 0.11.6: 351 types, 3 046
  methods — 5 filesystem call sites found, all dynamic, honestly reported as
  0 claims + 5 unresolved)
- ✅ at least one claim in all four registry categories (fixture assembly)
- ✅ refrains from guessing on dynamic values (cases 02, 07, 10 stay unresolved)
- ✅ clear unresolved findings with reasons
- ✅ output validates against schema v0.2
- ✅ deterministically identical output for the same artifact (tested)
- ✅ fixture suite runs in CI

## c2cs-serve — MCP server

`C2CS.Serve` exposes a C2CS document family (contract + assessments + verdicts) to AI
agents over MCP (newline-delimited JSON-RPC on stdio; hand-rolled, dependency-light).
The five tools implement walkthrough 01 directly:

| Tool | Answers |
|------|---------|
| `c2cs_overview` | "What am I looking at?" — subject, documents, mode, trust status |
| `c2cs_contract` | operations, entities/classifications, capabilities and prohibitions with rationale |
| `c2cs_check_action` | **"May I do X?"** — matches a proposed action against the contract using the registry matcher semantics; answers allowed-by / FORBIDDEN-by (with rationale) / would-be-drift |
| `c2cs_trust_status` | latest verdict: outcome, coverage, violations, drift, unexercised claims |
| `c2cs_pending_hypotheses` | inferred claims awaiting promotion review, marked covered / missing |

The matcher engine behind `c2cs_check_action` is implemented from the normative
artifacts alone and passes **all 50 registry matcher fixtures** — making `c2cs-serve`
the first *registry-matcher conformant (registry 0.1)* implementation, per the
conformance suite's own independence test.

Claude Code configuration (`.mcp.json` in your project):

```json
{
  "mcpServers": {
    "c2cs": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/C2CS/extractor/C2CS.Serve", "--",
               "--model-dir", "/path/to/your/c2cs-model"]
    }
  }
}
```

Try it against the reference family: `--model-dir ../spec/examples` — then ask the
agent "may this service call api.riskvendor.example?" and watch it answer with the
prohibition's rationale instead of a guess.
