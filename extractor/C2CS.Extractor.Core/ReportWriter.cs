using System.Text.Json;

namespace C2CS.Extractor.Core;

/// <summary>
/// Emits the informative output: extraction-report.json. Everything that does not fit
/// the standard lives here — unresolved findings, coverage, spec frictions — so
/// extractor problems never leak into C2CS fields.
/// </summary>
public static class ReportWriter
{
    public static string Write(ExtractionResult result, IReadOnlyList<string> specFindings)
    {
        var report = new
        {
            tool = ToolInfo.ToolId,
            artifact = new { path = result.Artifact.Path, digest = result.Artifact.Sha256Digest },
            assembly = new { name = result.AssemblyName, version = result.AssemblyVersion },
            coverage = new
            {
                types_scanned = result.Coverage.TypesScanned,
                methods_scanned = result.Coverage.MethodsScanned,
                call_sites_inspected = result.Coverage.CallSitesInspected,
                resolved_claims = result.Claims.Count,
                unresolved_findings = result.Unresolved.Count,
            },
            unresolved = result.Unresolved
                .OrderBy(u => u.Site.Source, StringComparer.Ordinal)
                .Select(u => new
                {
                    category = u.Category.Name(),
                    api = u.Site.Api,
                    site = u.Site.Source,
                    reason = u.Reason,
                }),
            limitations = new[]
            {
                "single-assembly, intraprocedural analysis: values passed through method parameters are not resolved (wrappers become unresolved findings)",
                "linear IL value tracking: values merged across branches are not resolved",
                "confidence tiers are fixed heuristics, not calibrated forecasts (ADR-0011 calibration is future work)",
            },
            spec_findings = specFindings,
        };

        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    public static List<string> CollectSpecFindings(bool usedBootstrapContract)
    {
        var findings = new List<string>
        {
            "E2: schema v0.2 requires producer.model on inferred assessments, but this producer is mechanical static analysis with no model — supports the deferred ADR-0003 question of a third assessment kind for mechanically derived findings",
        };
        if (usedBootstrapContract)
        {
            findings.Insert(0,
                "E1: schema v0.2 requires a contract reference on assessments, but the bootstrap scenario (extracting to CREATE the first contract) has none — sentinel digest used; candidate v0.3 change: contract reference optional on inferred assessments");
        }

        return findings;
    }
}
