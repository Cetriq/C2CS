using C2CS.Extractor.Core;
using C2CS.Extractor.DotNet;

namespace C2CS.Extractor.Tests;

public class ScannerTests
{
    private static readonly string FixturesDll =
        Path.Combine(AppContext.BaseDirectory, "C2CS.Extractor.Fixtures.dll");

    private static readonly ExtractionResult Result = Scanner.Analyze(FixturesDll);

    private static ResolvedClaim ClaimFrom(string caseName) =>
        Result.Claims.Single(c => c.Site.Method.Contains(caseName));

    private static Dictionary<string, object> Attrs(ResolvedClaim c) =>
        c.Attributes.ToDictionary(a => a.Key, a => a.Value);

    [Fact]
    public void Case01_ConstantProcessStart_HighConfidence()
    {
        var claim = ClaimFrom("Case01");
        Assert.Equal(EffectCategory.Process, claim.Category);
        Assert.Equal("/usr/bin/git", Attrs(claim)["executable"]);
        Assert.Equal(ConfidenceTiers.DirectConstant, claim.Confidence);
        Assert.Matches(@"@IL_[0-9a-f]{4}$", claim.Site.Source);
    }

    [Fact]
    public void Case02_DynamicProcessStart_IsUnresolvedNotGuessed()
    {
        Assert.DoesNotContain(Result.Claims, c => c.Site.Method.Contains("Case02"));
        var finding = Result.Unresolved.Single(u => u.Site.Method.Contains("Case02"));
        Assert.Equal(EffectCategory.Process, finding.Category);
        Assert.Contains("parameter", finding.Reason);
    }

    [Fact]
    public void Case03_ConstantEnvironmentVariable()
    {
        var claim = ClaimFrom("Case03");
        Assert.Equal(EffectCategory.Environment, claim.Category);
        Assert.Equal("DB_CONNECTION", Attrs(claim)["variable"]);
    }

    [Fact]
    public void Case04_ConstantFilePath_WriteAccess()
    {
        var claim = ClaimFrom("Case04");
        Assert.Equal(EffectCategory.Filesystem, claim.Category);
        Assert.Equal("/var/log/fixture.log", Attrs(claim)["path"]);
        Assert.Equal("write", Attrs(claim)["access"]);
    }

    [Fact]
    public void Case05_ConcatenatedPath_ResolvedWithDerivedConfidence()
    {
        var claim = ClaimFrom("Case05");
        Assert.Equal("/var/log/app.log", Attrs(claim)["path"]);
        Assert.Equal(ConfidenceTiers.DerivedConstant, claim.Confidence);
    }

    [Fact]
    public void Case06_ConstantHttpUri_HostAndDefaultPort()
    {
        var claim = ClaimFrom("Case06");
        Assert.Equal(EffectCategory.Network, claim.Category);
        Assert.Equal("outbound", Attrs(claim)["direction"]);
        Assert.Equal("api.example.test", Attrs(claim)["host"]);
        Assert.Equal(443, Attrs(claim)["port"]);
    }

    [Fact]
    public void Case07_UriFromParameter_IsUnresolved()
    {
        Assert.DoesNotContain(Result.Claims, c => c.Site.Method.Contains("Case07"));
        var finding = Result.Unresolved.Single(u => u.Site.Method.Contains("Case07"));
        Assert.Equal(EffectCategory.Network, finding.Category);
    }

    [Fact]
    public void Case08_TcpClient_HostAndPort()
    {
        var claim = ClaimFrom("Case08");
        Assert.Equal("db.example.test", Attrs(claim)["host"]);
        Assert.Equal(5432, Attrs(claim)["port"]);
    }

    [Fact]
    public void Case09_ConnectionString_ServerAndPortParsed()
    {
        var claim = ClaimFrom("Case09");
        Assert.Equal(EffectCategory.Network, claim.Category);
        Assert.Equal("sql.example.test", Attrs(claim)["host"]);
        Assert.Equal(1433, Attrs(claim)["port"]);
    }

    [Fact]
    public void Case10_Wrapper_InnerCallUnresolved_NoInterproceduralGuess()
    {
        var finding = Result.Unresolved.Single(u => u.Site.Method.Contains("Case10"));
        Assert.Contains("wrapper", finding.Reason);
        // The constant at the outer call site is NOT propagated in the PoC.
        Assert.DoesNotContain(Result.Claims, c => Attrs(c).TryGetValue("executable", out var e) && (string)e == "/usr/bin/tool");
    }

    [Fact]
    public void AllFourCategoriesProduceAtLeastOneClaim()
    {
        foreach (var category in Enum.GetValues<EffectCategory>())
            Assert.Contains(Result.Claims, c => c.Category == category);
    }

    [Fact]
    public void OutputIsDeterministic()
    {
        var second = Scanner.Analyze(FixturesDll);
        var a = AssessmentWriter.Write(Result, "0.1", null, null);
        var b = AssessmentWriter.Write(second, "0.1", null, null);
        Assert.Equal(a, b);
    }

    [Fact]
    public void AssessmentDeclaresEveryCategoryWithExplicitStatus()
    {
        var yaml = AssessmentWriter.Write(Result, "0.1", null, null);
        foreach (var name in new[] { "network", "filesystem", "process", "environment" })
            Assert.Contains($"  {name}:\n    status: ", yaml.ReplaceLineEndings("\n"));
        Assert.Contains("kind: inferred", yaml);
        Assert.Contains("confidence:", yaml);
    }

    [Fact]
    public void ReportListsUnresolvedFindingsAndSpecFindings()
    {
        var report = ReportWriter.Write(Result, ReportWriter.CollectSpecFindings(usedBootstrapContract: true));
        Assert.Contains("Case02", report);
        Assert.Contains("Case07", report);
        Assert.Contains("Case10", report);
        Assert.Contains("E1:", report);
        Assert.Contains("E2:", report);
    }
}
