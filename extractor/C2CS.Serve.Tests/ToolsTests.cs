using C2CS.Serve;
using Xunit;

namespace C2CS.Serve.Tests;

/// <summary>Tools over the reference document family in spec/examples/.</summary>
public class ToolsTests
{
    private static readonly Tools Tools = new(new ModelStore(RepoPaths.ExamplesDir));

    [Fact]
    public void Overview_ShowsSubjectModeAndTrust()
    {
        var text = Tools.Overview();
        Assert.Contains("pkg:nuget/CreditService", text);
        Assert.Contains("mode: closed", text);
        Assert.Contains("not-conformant", text); // the example verdict
        Assert.Contains("environment", text);    // unknown-coverage caveat
    }

    [Fact]
    public void Contract_ListsOperationsCapabilitiesAndForbiddenWithRationale()
    {
        var text = Tools.Contract();
        Assert.Contains("op.customer.credit.evaluate", text);
        Assert.Contains("c2cs.data.personal", text);
        Assert.Contains("cap.net.db", text);
        Assert.Contains("forb.net.external", text);
        Assert.Contains("must not leave the internal zone", text);
    }

    [Fact]
    public void CheckAction_AllowedByDeclaredClaim()
    {
        var text = Tools.CheckAction("network", Scope(("direction", "outbound"), ("host", "db.internal.acme.example"), ("port", "5432")));
        Assert.Contains("allowed by cap.net.db", text);
        Assert.Contains("confirmed", text);
    }

    [Fact]
    public void CheckAction_ExternalHost_ForbiddenWithRationale()
    {
        var text = Tools.CheckAction("network", Scope(("direction", "outbound"), ("host", "api.riskvendor.example"), ("port", "443")));
        Assert.Contains("FORBIDDEN by forb.net.external", text);
        Assert.Contains("rationale", text);
        Assert.Contains("contract change", text);
    }

    [Fact]
    public void CheckAction_UncoveredUnderClosedMode_IsDrift()
    {
        var text = Tools.CheckAction("process", Scope(("executable", "/usr/bin/git")));
        Assert.Contains("DRIFT", text);
        Assert.Contains("human review", text);
    }

    [Fact]
    public void TrustStatus_SurfacesViolationDriftAndUnknownCoverage()
    {
        var text = Tools.TrustStatus();
        Assert.Contains("not-conformant", text);
        Assert.Contains("VIOLATION: forb.net.external", text);
        Assert.Contains("DRIFT", text);
        Assert.Contains("environment=unknown", text);
        Assert.Contains("not_observed: cap.net.audit", text);
    }

    [Fact]
    public void PendingHypotheses_MarksCoveredAndUncovered()
    {
        var text = Tools.PendingHypotheses();
        Assert.Contains("confidence", text);
        Assert.Contains("already covered by a declared claim", text); // inf.net.db ≙ cap.net.db
    }

    private static Dictionary<object, object> Scope(params (string K, string V)[] pairs) =>
        pairs.ToDictionary(p => (object)p.K, p => (object)p.V);
}
