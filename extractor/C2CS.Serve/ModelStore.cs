namespace C2CS.Serve;

/// <summary>
/// Loads a C2CS document family from a directory: one contract, any number of
/// assessments and verdicts. Document kind is read from the document, never from file
/// names (ADR-0005: nothing depends on file layout).
/// </summary>
public sealed class ModelStore
{
    public Dictionary<object, object>? Contract { get; private set; }

    public List<Dictionary<object, object>> Assessments { get; } = [];

    public List<Dictionary<object, object>> Verdicts { get; } = [];

    public string Directory { get; }

    public ModelStore(string directory)
    {
        Directory = directory;
        foreach (var file in System.IO.Directory.GetFiles(directory, "*.yaml").OrderBy(f => f, StringComparer.Ordinal))
        {
            Dictionary<object, object> doc;
            try { doc = Yaml.LoadFile(file); }
            catch { continue; } // non-C2CS yaml in the directory is ignored
            switch (Yaml.Get(doc, "kind"))
            {
                case "contract":
                    Contract ??= doc;
                    break;
                case "assessment":
                    Assessments.Add(doc);
                    break;
                case "verdict":
                    Verdicts.Add(doc);
                    break;
            }
        }
    }

    public Dictionary<object, object>? LatestVerdict =>
        Verdicts.OrderBy(v => Yaml.Get(Yaml.GetNode(v, "evaluated_over"), "to") ?? "", StringComparer.Ordinal).LastOrDefault();

    public IEnumerable<Dictionary<object, object>> InferredAssessments =>
        Assessments.Where(a => Yaml.Get(Yaml.GetNode(a, "assessment"), "kind") == "inferred");

    public string Mode => Yaml.Get(Contract, "mode") ?? "open";

    /// <summary>Declared capability claims per category: (category, claim map).</summary>
    public IEnumerable<(string Category, Dictionary<object, object> Claim)> CapabilityClaims()
    {
        foreach (var kv in Yaml.Map(Yaml.GetNode(Contract, "capabilities")) ?? [])
        {
            var category = (string)kv.Key;
            foreach (var claim in Yaml.Seq(kv.Value))
                if (Yaml.Map(claim) is { } m)
                    yield return (category, m);
        }
    }

    public IEnumerable<Dictionary<object, object>> Forbidden()
    {
        foreach (var entry in Yaml.Seq(Yaml.GetNode(Contract, "forbidden")))
            if (Yaml.Map(entry) is { } m)
                yield return m;
    }
}
