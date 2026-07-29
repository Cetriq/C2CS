namespace C2CS.Serve;

/// <summary>
/// Registry 0.1 matcher engine: does an event fall under a claim scope? Implemented
/// from the normative artifacts alone (registry entries + matcher fixtures), per
/// ADR-0008's independence test. Exercised against all fixtures in
/// spec/conformance/registry/ by the test suite.
/// </summary>
public static class Matcher
{
    public static bool Matches(string category, Dictionary<object, object> scope, Dictionary<object, object> ev) =>
        category switch
        {
            "network" => Network(scope, ev),
            "filesystem" => Filesystem(scope, ev),
            "process" => Process(scope, ev),
            "environment" => Environment(scope, ev),
            _ => throw new ArgumentException($"unknown category: {category}"),
        };

    // ── network ─────────────────────────────────────────────────────

    private static bool Network(Dictionary<object, object> scope, Dictionary<object, object> ev)
    {
        var scopeDirection = Yaml.Get(scope, "direction");
        var evDirection = Yaml.Get(ev, "direction");
        if (scopeDirection is not null && scopeDirection != evDirection) return false;

        var evHost = NormalizeHost(Yaml.Get(ev, "host") ?? "");
        if (!HostFormMatches(Yaml.Get(scope, "host") ?? "any", evHost)) return false;
        foreach (var exception in Yaml.Seq(Yaml.GetNode(scope, "except")))
            if (HostFormMatches(Yaml.Str(exception) ?? "", evHost)) return false;

        return PortMatches(Yaml.GetNode(scope, "port"), Yaml.Int(Yaml.GetNode(ev, "port")));
    }

    private static string NormalizeHost(string host)
    {
        var h = host.Trim().ToLowerInvariant();
        return h.EndsWith('.') ? h[..^1] : h;
    }

    private static bool IsIpLiteral(string host) =>
        host.Length > 0 && (char.IsDigit(host[0]) || host.Contains(':'))
        && System.Net.IPAddress.TryParse(host, out _);

    private static bool HostFormMatches(string form, string evHost)
    {
        form = NormalizeHost(form);
        if (form == "any") return true;
        if (form.StartsWith("*."))
        {
            // Suffix matches names with at least one label before it; never *.X == X,
            // and names/addresses are distinct identities.
            var suffix = form[2..];
            return !IsIpLiteral(evHost) && evHost.EndsWith("." + suffix) && evHost.Length > suffix.Length + 1;
        }

        if (IsIpLiteral(form)) return IsIpLiteral(evHost) && form == evHost;
        return !IsIpLiteral(evHost) && form == evHost;
    }

    private static bool PortMatches(object? scopePort, int? evPort)
    {
        switch (scopePort)
        {
            case null: return true; // omitted = any
            case string s when s == "any": return true;
            case string s when s.Contains('-'):
            {
                var parts = s.Split('-', 2);
                return evPort is int p
                    && int.TryParse(parts[0], out var lo) && int.TryParse(parts[1], out var hi)
                    && p >= lo && p <= hi; // ranges are inclusive at both ends
            }

            case string s: return Yaml.Int(s) is int single && evPort == single;
            case List<object> list: return evPort is int e && list.Any(x => Yaml.Int(x) == e);
            default: return false;
        }
    }

    // ── filesystem ──────────────────────────────────────────────────

    private static bool Filesystem(Dictionary<object, object> scope, Dictionary<object, object> ev)
    {
        var evPath = NormalizePath(Yaml.Get(ev, "path") ?? "");
        var evAccess = Yaml.Get(ev, "access") ?? "";

        if (!AccessMatches(Yaml.GetNode(scope, "access"), evAccess)) return false;
        if (!PathFormMatches(Yaml.Get(scope, "path") ?? "", evPath)) return false;
        foreach (var exception in Yaml.Seq(Yaml.GetNode(scope, "except")))
            if (PathFormMatches(Yaml.Str(exception) ?? "", evPath)) return false;
        return true;
    }

    private static bool AccessMatches(object? scopeAccess, string evAccess) => scopeAccess switch
    {
        null => false,
        string s => s == evAccess,
        List<object> list => list.Any(x => Yaml.Str(x) == evAccess),
        _ => false,
    };

    /// <summary>Resolve '.' and '..' segments; matching is case-sensitive by rule.</summary>
    private static string NormalizePath(string path)
    {
        var trailingSlash = path.EndsWith('/') && path.Length > 1;
        var stack = new List<string>();
        foreach (var segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".") continue;
            if (segment == "..") { if (stack.Count > 0) stack.RemoveAt(stack.Count - 1); continue; }
            stack.Add(segment);
        }

        return "/" + string.Join('/', stack) + (trailingSlash ? "/" : "");
    }

    private static bool PathFormMatches(string form, string evPath)
    {
        form = NormalizePath(form);
        if (form.EndsWith('/')) return evPath.StartsWith(form, StringComparison.Ordinal);
        return evPath == form; // exact scopes cover exactly one path
    }

    // ── process ─────────────────────────────────────────────────────

    private static bool Process(Dictionary<object, object> scope, Dictionary<object, object> ev)
    {
        var evExe = NormalizePath(Yaml.Get(ev, "executable") ?? "");
        if (!ExecutableFormMatches(Yaml.Get(scope, "executable") ?? "", evExe)) return false;
        foreach (var exception in Yaml.Seq(Yaml.GetNode(scope, "except")))
            if (ExecutableFormMatches(Yaml.Str(exception) ?? "", evExe)) return false;
        return true;
    }

    private static bool ExecutableFormMatches(string form, string evExe)
    {
        if (form == "any") return true;
        if (!form.Contains('/'))
        {
            // Basename form: exact match on the final path segment.
            var basename = evExe[(evExe.LastIndexOf('/') + 1)..];
            return basename == form;
        }

        return PathFormMatches(form, evExe);
    }

    // ── environment ─────────────────────────────────────────────────

    private static bool Environment(Dictionary<object, object> scope, Dictionary<object, object> ev)
    {
        var evVariable = Yaml.Get(ev, "variable") ?? "";
        if (!VariableFormMatches(Yaml.Get(scope, "variable") ?? "", evVariable)) return false;
        foreach (var exception in Yaml.Seq(Yaml.GetNode(scope, "except")))
            if (VariableFormMatches(Yaml.Str(exception) ?? "", evVariable)) return false;
        return true;
    }

    private static bool VariableFormMatches(string form, string evVariable)
    {
        if (form == "any") return true;
        if (form.EndsWith('*'))
        {
            // Prefix pattern: the match must be strictly longer than the prefix.
            var prefix = form[..^1];
            return evVariable.StartsWith(prefix, StringComparison.Ordinal) && evVariable.Length > prefix.Length;
        }

        return evVariable == form; // case-sensitive, exactly as the platform reports
    }
}
