using C2CS.Serve;
using Xunit;

namespace C2CS.Serve.Tests;

/// <summary>
/// Runs the matcher engine against ALL registry matcher fixtures in
/// spec/conformance/registry/ — the operative definition of matching semantics.
/// Passing every case is what "C2CS registry-matcher conformant (registry 0.1)" means.
/// </summary>
public class MatcherFixtureTests
{
    public static TheoryData<string, string> AllCases()
    {
        var data = new TheoryData<string, string>();
        foreach (var file in Directory.GetFiles(RepoPaths.MatcherFixturesDir, "*.yaml").OrderBy(f => f))
        {
            var doc = Yaml.LoadFile(file);
            foreach (var caseNode in Yaml.Seq(Yaml.GetNode(doc, "cases")))
                data.Add(Path.GetFileNameWithoutExtension(file), Yaml.Get(caseNode, "name")!);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(AllCases))]
    public void MatcherFixture(string category, string caseName)
    {
        var doc = Yaml.LoadFile(Path.Combine(RepoPaths.MatcherFixturesDir, category + ".yaml"));
        var c = Yaml.Seq(Yaml.GetNode(doc, "cases")).First(n => Yaml.Get(n, "name") == caseName);
        var scope = Yaml.Map(Yaml.GetNode(c, "scope"))!;
        var ev = Yaml.Map(Yaml.GetNode(c, "event"))!;
        var expected = Yaml.Bool(Yaml.GetNode(c, "matches"))!.Value;

        var actual = Matcher.Matches(Yaml.Get(doc, "category")!, scope, ev);

        Assert.True(expected == actual,
            $"{category}/{caseName}: expected matches={expected}, got {actual} (rule: {Yaml.Get(c, "rule")})");
    }
}

public static class RepoPaths
{
    public static string Root
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "spec")))
                dir = dir.Parent;
            return dir?.FullName ?? throw new InvalidOperationException("repo root with spec/ not found");
        }
    }

    public static string MatcherFixturesDir => Path.Combine(Root, "spec", "conformance", "registry");

    public static string ExamplesDir => Path.Combine(Root, "spec", "examples");
}
