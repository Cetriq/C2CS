using System.Text;

namespace C2CS.Extractor.Core;

/// <summary>
/// Emits the normative output: an inferred assessment per schema v0.2.
/// Hand-rolled YAML with fixed field order so identical input yields identical bytes.
/// </summary>
public static class AssessmentWriter
{
    public const string BootstrapContractSubject = "bootstrap:no-contract";
    public static readonly string BootstrapContractDigest = "sha256:" + new string('0', 64);

    public static string Write(
        ExtractionResult result,
        string registryVersion,
        string? contractSubject,
        string? contractDigest)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Inferred assessment produced by " + ToolInfo.ToolId + ".");
        sb.AppendLine("# Normative output only; extraction details live in extraction-report.json.");
        sb.AppendLine("c2cs: \"" + ToolInfo.SchemaVersion + "\"");
        sb.AppendLine("kind: assessment");
        sb.AppendLine("registry: \"" + registryVersion + "\"");
        sb.AppendLine("assessment:");
        sb.AppendLine("  kind: inferred");
        sb.AppendLine("producer:");
        sb.AppendLine("  tool: " + Scalar(ToolInfo.ToolId));
        sb.AppendLine("  model: " + Scalar(ToolInfo.Model));
        sb.AppendLine("  registry: \"" + registryVersion + "\"");
        sb.AppendLine("  schema: \"" + ToolInfo.SchemaVersion + "\"");
        sb.AppendLine("contract:");
        sb.AppendLine("  subject: " + Scalar(contractSubject ?? BootstrapContractSubject));
        sb.AppendLine("  digest: " + (contractDigest ?? BootstrapContractDigest));
        sb.AppendLine("subject:");
        sb.AppendLine("  logical: " + Scalar("pkg:dotnet/" + result.AssemblyName));
        sb.AppendLine("  version: \"" + result.AssemblyVersion + "\"");
        sb.AppendLine("  artifacts:");
        sb.AppendLine("    - path: " + Scalar(result.Artifact.Path));
        sb.AppendLine("      digest: " + result.Artifact.Sha256Digest);
        sb.AppendLine("capabilities:");

        foreach (var category in new[]
                 { EffectCategory.Network, EffectCategory.Filesystem, EffectCategory.Process, EffectCategory.Environment })
        {
            var claims = Deduplicate(result.Claims.Where(c => c.Category == category));
            var unresolved = result.Unresolved.Count(u => u.Category == category);
            sb.AppendLine("  " + category.Name() + ":");
            if (claims.Count == 0 && unresolved > 0)
            {
                // Call sites exist but none could be stated per the matcher grammar.
                // "analyzed, claims: []" would assert known-empty, which is false —
                // three-valued honesty (ADR-0006) demands unknown here.
                sb.AppendLine("    status: not-analyzed");
                continue;
            }

            sb.AppendLine("    status: analyzed");
            if (claims.Count == 0)
            {
                sb.AppendLine("    claims: []");
                continue;
            }

            sb.AppendLine("    claims:");
            var n = 0;
            foreach (var claim in claims)
            {
                n++;
                sb.AppendLine("      - id: inf." + category.Name() + "." + n);
                foreach (var (key, value) in claim.Attributes)
                    sb.AppendLine("        " + key + ": " + (value is int i ? i.ToString() : Scalar((string)value)));
                sb.AppendLine("        confidence: " + claim.Confidence.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine("        source: " + Scalar(claim.Site.Source));
            }
        }

        return sb.ToString();
    }

    /// <summary>Identical bodies collapse to one claim (ADR-0009: category+scope is the meaning).</summary>
    private static List<ResolvedClaim> Deduplicate(IEnumerable<ResolvedClaim> claims)
    {
        return claims
            .GroupBy(BodyKey)
            .Select(g => g.OrderBy(c => c.Site.Source, StringComparer.Ordinal).First())
            .OrderBy(BodyKey, StringComparer.Ordinal)
            .ToList();
    }

    private static string BodyKey(ResolvedClaim c) =>
        string.Join("|", c.Attributes.Select(a => a.Key + "=" + a.Value));

    private static string Scalar(string value)
    {
        var needsQuote = value.Length == 0
            || value.Any(ch => ch is ':' or '#' or '{' or '}' or '[' or ']' or ',' or '&' or '*' or '?' or '|' or '>' or '!' or '%' or '@' or '`' or '"' or '\'')
            || value.StartsWith(' ') || value.EndsWith(' ') || value.StartsWith('-');
        if (!needsQuote) return value;
        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
