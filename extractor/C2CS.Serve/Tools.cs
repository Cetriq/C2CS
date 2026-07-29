using System.Text;

namespace C2CS.Serve;

/// <summary>
/// The five MCP tools, straight from walkthrough 01: orient, read the contract,
/// check a proposed action, check trust, review pending hypotheses. Pure functions
/// over the ModelStore; the protocol layer is elsewhere.
/// </summary>
public sealed class Tools(ModelStore store)
{
    private static readonly string[] Categories = ["network", "filesystem", "process", "environment"];

    public string Overview()
    {
        var sb = new StringBuilder();
        var subject = Yaml.GetNode(store.Contract, "subject");
        sb.AppendLine("# C2CS model overview");
        sb.AppendLine($"Subject: {Yaml.Get(subject, "logical")} v{Yaml.Get(subject, "version")}");
        sb.AppendLine($"Documents: contract={(store.Contract is null ? 0 : 1)}, assessments={store.Assessments.Count}, verdicts={store.Verdicts.Count}");
        sb.AppendLine($"Contract mode: {store.Mode}" + (store.Mode == "closed" ? " (anything not declared is forbidden)" : " (undeclared behavior is unknown, not forbidden)"));

        var operations = Yaml.Seq(Yaml.GetNode(Yaml.GetNode(store.Contract, "concepts"), "operations"));
        sb.AppendLine($"Operations: {operations.Count}; capability claims: {store.CapabilityClaims().Count()}; prohibitions: {store.Forbidden().Count()}");

        var verdict = store.LatestVerdict;
        if (verdict is null)
        {
            sb.AppendLine("Trust: no verdict available — the declared picture is unverified.");
        }
        else
        {
            var overall = Yaml.GetNode(verdict, "overall");
            sb.AppendLine($"Trust (latest verdict): outcome={Yaml.Get(overall, "outcome")}");
            var coverage = Yaml.Map(Yaml.GetNode(overall, "coverage")) ?? [];
            var unknown = coverage.Where(c => Yaml.Str(c.Value) == "unknown").Select(c => (string)c.Key).ToList();
            if (unknown.Count > 0)
                sb.AppendLine($"Caveat: categories not observed: {string.Join(", ", unknown)}");
        }

        sb.AppendLine();
        sb.AppendLine("Epistemics: the contract is DECLARED (human-approved intent); assessments are INFERRED (hypotheses) or OBSERVED (runtime evidence); verdicts compare declared with observed. Use c2cs_check_action before proposing changes that touch network, filesystem, process, or environment behavior.");
        return sb.ToString();
    }

    public string Contract()
    {
        if (store.Contract is null) return "No contract in the model directory.";
        var sb = new StringBuilder();
        sb.AppendLine($"# Contract — mode: {store.Mode}");

        var operations = Yaml.Seq(Yaml.GetNode(Yaml.GetNode(store.Contract, "concepts"), "operations"));
        if (operations.Count > 0)
        {
            sb.AppendLine("## Operations");
            foreach (var op in operations)
            {
                sb.AppendLine($"- {Yaml.Get(op, "id")}: {Yaml.Get(op, "summary")}");
                var reads = Yaml.Seq(Yaml.GetNode(op, "reads")).OfType<string>().ToList();
                var writes = Yaml.Seq(Yaml.GetNode(op, "writes")).OfType<string>().ToList();
                var uses = Yaml.Seq(Yaml.GetNode(op, "uses")).OfType<string>().ToList();
                if (reads.Count > 0) sb.AppendLine($"  reads: {string.Join(", ", reads)}");
                if (writes.Count > 0) sb.AppendLine($"  writes: {string.Join(", ", writes)}");
                if (uses.Count > 0) sb.AppendLine($"  uses: {string.Join(", ", uses)}");
            }
        }

        var entities = Yaml.Seq(Yaml.GetNode(Yaml.GetNode(Yaml.GetNode(store.Contract, "concepts"), "data"), "entities"));
        if (entities.Count > 0)
        {
            sb.AppendLine("## Data entities");
            foreach (var e in entities)
                sb.AppendLine($"- {Yaml.Get(e, "name")} ({Yaml.Get(e, "classification")})");
        }

        sb.AppendLine("## Capabilities (what MAY happen)");
        foreach (var (category, claim) in store.CapabilityClaims())
        {
            sb.Append($"- [{category}] {Yaml.Get(claim, "id")}: {DescribeBody(category, claim)}");
            if (Yaml.Get(claim, "rationale") is { } r) sb.Append($" — {r}");
            sb.AppendLine();
        }

        var forbidden = store.Forbidden().ToList();
        if (forbidden.Count > 0)
        {
            sb.AppendLine("## Forbidden (what MUST NOT happen)");
            foreach (var f in forbidden)
            {
                sb.Append($"- [{Yaml.Get(f, "category")}] {Yaml.Get(f, "id")}: {DescribeScope(Yaml.Map(Yaml.GetNode(f, "match")) ?? [])}");
                if (Yaml.Get(f, "rationale") is { } r) sb.Append($" — rationale: {r}");
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>"May I do X?" — the reason this server exists.</summary>
    public string CheckAction(string category, Dictionary<object, object> action)
    {
        if (store.Contract is null) return "No contract loaded — nothing to check against.";
        if (!Categories.Contains(category)) return $"Unknown category '{category}'. Registry 0.1 categories: {string.Join(", ", Categories)}.";

        var sb = new StringBuilder();
        sb.AppendLine($"Action: [{category}] {DescribeScope(action)}");

        var allowedBy = store.CapabilityClaims()
            .Where(c => c.Category == category && Matcher.Matches(category, c.Claim, action))
            .Select(c => c.Claim)
            .ToList();
        var forbiddenBy = store.Forbidden()
            .Where(f => Yaml.Get(f, "category") == category && Matcher.Matches(category, Yaml.Map(Yaml.GetNode(f, "match")) ?? [], action))
            .ToList();

        foreach (var f in forbiddenBy)
        {
            sb.AppendLine($"FORBIDDEN by {Yaml.Get(f, "id")}"
                + (Yaml.Get(f, "rationale") is { } r ? $" — rationale: {r}" : ""));
        }

        foreach (var c in allowedBy)
        {
            sb.AppendLine($"allowed by {Yaml.Get(c, "id")}"
                + (Yaml.Get(c, "rationale") is { } r ? $" — {r}" : ""));
        }

        if (forbiddenBy.Count > 0)
        {
            sb.AppendLine("Verdict-if-observed: violation. Do not implement this without a contract change approved by a human (an exception decision on the prohibition).");
        }
        else if (allowedBy.Count > 0)
        {
            sb.AppendLine("Verdict-if-observed: confirmed — within the declared contract.");
        }
        else if (store.Mode == "closed")
        {
            sb.AppendLine("Not covered by any declared capability. Contract mode is closed: if implemented, this becomes DRIFT (not-conformant). Required path: add a capability claim to the contract via human review — do not just write the code.");
        }
        else
        {
            sb.AppendLine("Not covered by any declared capability. Contract mode is open: this would be recorded as drift (informative), not a violation. Declaring it is still recommended.");
        }

        return sb.ToString();
    }

    public string TrustStatus()
    {
        var verdict = store.LatestVerdict;
        if (verdict is null) return "No verdict available. The declared contract has not been checked against observed behavior — treat the model as intent, not as verified fact.";

        var sb = new StringBuilder();
        var window = Yaml.GetNode(verdict, "evaluated_over");
        var overall = Yaml.GetNode(verdict, "overall");
        sb.AppendLine($"# Latest verdict — outcome: {Yaml.Get(overall, "outcome")}");
        sb.AppendLine($"Window: {Yaml.Get(window, "from")} → {Yaml.Get(window, "to")}");
        sb.AppendLine("Coverage: " + string.Join(", ",
            (Yaml.Map(Yaml.GetNode(overall, "coverage")) ?? []).Select(c => $"{c.Key}={Yaml.Str(c.Value)}")));

        var results = Yaml.GetNode(verdict, "results");
        foreach (var f in Yaml.Seq(Yaml.GetNode(results, "forbidden")))
            if (Yaml.Get(f, "verdict") == "violation")
                sb.AppendLine($"VIOLATION: {Yaml.Get(f, "claim")} (matched: {string.Join(", ", Yaml.Seq(Yaml.GetNode(f, "matched")).OfType<string>())})");
        foreach (var d in Yaml.Seq(Yaml.GetNode(results, "drift")))
            sb.AppendLine($"DRIFT: [{Yaml.Get(d, "category")}] {Yaml.Get(d, "observed")} — {Yaml.Get(d, "note")}");
        foreach (var c in Yaml.Seq(Yaml.GetNode(results, "claims")))
            if (Yaml.Get(c, "verdict") == "not_observed")
                sb.AppendLine($"not_observed: {Yaml.Get(c, "claim")} (declared but never seen in the window — an absence of observation, nothing more)");

        return sb.ToString();
    }

    /// <summary>Inferred claims not yet covered by the contract — the promotion queue.</summary>
    public string PendingHypotheses()
    {
        var inferred = store.InferredAssessments.ToList();
        if (inferred.Count == 0) return "No inferred assessments in the model directory.";

        var sb = new StringBuilder();
        sb.AppendLine("# Pending hypotheses (inferred, awaiting promotion review)");
        var any = false;
        foreach (var assessment in inferred)
        {
            foreach (var kv in Yaml.Map(Yaml.GetNode(assessment, "capabilities")) ?? [])
            {
                var category = (string)kv.Key;
                foreach (var claimNode in Yaml.Seq(Yaml.GetNode(kv.Value, "claims")))
                {
                    if (Yaml.Map(claimNode) is not { } claim) continue;
                    var covered = store.CapabilityClaims()
                        .Any(c => c.Category == category && Matcher.Matches(category, c.Claim, claim));
                    any = true;
                    sb.AppendLine($"- [{category}] {DescribeScope(claim)} (confidence {Yaml.Get(claim, "confidence")}, source {Yaml.Get(claim, "source") ?? "n/a"})"
                        + (covered ? " — already covered by a declared claim" : " — NOT in contract: candidate for promotion or a finding to investigate"));
                }
            }
        }

        return any ? sb.ToString() : "Inferred assessments present, but no capability claims in them.";
    }

    private static string DescribeBody(string category, Dictionary<object, object> claim) => DescribeScope(claim);

    private static string DescribeScope(Dictionary<object, object> scope)
    {
        var skip = new HashSet<string> { "id", "by", "date", "rationale", "promoted_from", "confidence", "source", "evidence", "first_seen", "last_seen" };
        var parts = scope
            .Where(kv => kv.Key is string k && !skip.Contains(k))
            .Select(kv => $"{kv.Key}={Render(kv.Value)}");
        return string.Join(" ", parts);
    }

    private static string Render(object? v) => v switch
    {
        List<object> list => "[" + string.Join(",", list.Select(Render)) + "]",
        _ => Yaml.Str(v) ?? v?.ToString() ?? "",
    };
}
