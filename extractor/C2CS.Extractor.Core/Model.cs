namespace C2CS.Extractor.Core;

/// <summary>Registry 0.1 Tier-1 effect categories.</summary>
public enum EffectCategory
{
    Network,
    Filesystem,
    Process,
    Environment,
}

public static class EffectCategoryNames
{
    public static string Name(this EffectCategory c) => c switch
    {
        EffectCategory.Network => "network",
        EffectCategory.Filesystem => "filesystem",
        EffectCategory.Process => "process",
        EffectCategory.Environment => "environment",
        _ => throw new System.ArgumentOutOfRangeException(nameof(c)),
    };
}

/// <summary>The analyzed artifact, digest-bound (ADR-0005).</summary>
public sealed record Artifact(string Path, string Sha256Digest);

/// <summary>Where in the artifact a finding comes from: method + IL offset.</summary>
public sealed record CallSite(string Method, string IlOffset, string Api)
{
    /// <summary>The claim's `source` string: method@offset.</summary>
    public string Source => $"{Method}@{IlOffset}";
}

/// <summary>
/// A claim the extractor can state within the registry's matcher grammar.
/// Attribute values are strings or ints; emission order is fixed per category.
/// </summary>
public sealed record ResolvedClaim(
    EffectCategory Category,
    IReadOnlyList<KeyValuePair<string, object>> Attributes,
    double Confidence,
    CallSite Site);

/// <summary>
/// A relevant call site whose scope could not be stated per the matcher grammar.
/// Never becomes a claim (the assessment must not guess); reported informatively.
/// </summary>
public sealed record UnresolvedFinding(EffectCategory Category, CallSite Site, string Reason);

public sealed record ExtractionCoverage(int TypesScanned, int MethodsScanned, int CallSitesInspected);

/// <summary>Everything one extraction run produced, before document emission.</summary>
public sealed record ExtractionResult(
    Artifact Artifact,
    string AssemblyName,
    string AssemblyVersion,
    IReadOnlyList<ResolvedClaim> Claims,
    IReadOnlyList<UnresolvedFinding> Unresolved,
    ExtractionCoverage Coverage);

public static class ToolInfo
{
    public const string ToolId = "c2cs-extractor-dotnet/0.1.0";

    /// <summary>
    /// Schema v0.2 requires producer.model on inferred assessments; this extractor is
    /// mechanical static analysis, no LLM. Recorded as spec finding E2 (a candidate
    /// third assessment kind, deferred by ADR-0003).
    /// </summary>
    public const string Model = "static-il-analysis";

    public const string SchemaVersion = "0.2";
}

public static class ConfidenceTiers
{
    // Fixed, uncalibrated tiers for the PoC. Per ADR-0011 these are forecasts of
    // confirmation; calibration against verdicts is future research, not PoC scope.
    public const double DirectConstant = 0.9;
    public const double DerivedConstant = 0.8; // e.g. string.Concat of constants
}
