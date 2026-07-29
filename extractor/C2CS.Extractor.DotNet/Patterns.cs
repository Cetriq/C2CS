using C2CS.Extractor.Core;
using Mono.Cecil;

namespace C2CS.Extractor.DotNet;

/// <summary>
/// The PoC support surface: which .NET APIs map to which registry categories, which
/// arguments carry the scope, and how resolved values become matcher attributes.
/// Deliberately small — growth happens against real extraction failures, not upfront.
/// </summary>
public static class Patterns
{
    public sealed record Pattern(
        EffectCategory Category,
        string Api,
        int[] ArgIndices,
        Func<IReadOnlyList<Value>, IReadOnlyList<KeyValuePair<string, object>>?> BuildAttributes);

    private static readonly HashSet<string> FileRead =
        ["ReadAllText", "ReadAllLines", "ReadAllBytes", "ReadLines", "OpenRead", "Exists", "OpenText"];

    private static readonly HashSet<string> FileWrite =
        ["WriteAllText", "WriteAllLines", "WriteAllBytes", "AppendAllText", "AppendAllLines", "Create", "CreateText", "Delete", "Move", "Copy", "OpenWrite"];

    private static readonly HashSet<string> DirRead = ["Exists", "GetFiles", "GetDirectories", "EnumerateFiles", "EnumerateDirectories"];
    private static readonly HashSet<string> DirWrite = ["CreateDirectory", "Delete", "Move"];

    private static readonly HashSet<string> HttpMethods =
        ["GetAsync", "GetStringAsync", "GetByteArrayAsync", "GetStreamAsync", "PostAsync", "PutAsync", "PatchAsync", "DeleteAsync"];

    public static Pattern? Match(MethodReference m)
    {
        var type = m.DeclaringType.FullName;
        var name = m.Name;

        // process
        if (type == "System.Diagnostics.Process" && name == "Start"
            && m.Parameters.Count >= 1 && m.Parameters[0].ParameterType.FullName == "System.String")
            return new(EffectCategory.Process, "Process.Start", [0], a => Executable(a[0]));
        if (type == "System.Diagnostics.ProcessStartInfo" && name == ".ctor"
            && m.Parameters.Count >= 1 && m.Parameters[0].ParameterType.FullName == "System.String")
            return new(EffectCategory.Process, "ProcessStartInfo..ctor", [0], a => Executable(a[0]));
        if (type == "System.Diagnostics.ProcessStartInfo" && name == "set_FileName")
            return new(EffectCategory.Process, "ProcessStartInfo.FileName", [0], a => Executable(a[0]));

        // environment
        if (type == "System.Environment" && name == "GetEnvironmentVariable" && m.Parameters.Count >= 1)
            return new(EffectCategory.Environment, "Environment.GetEnvironmentVariable", [0],
                a => a[0] is Value.Str s ? Attrs(("variable", s.V)) : null);

        // filesystem
        if (type == "System.IO.File" && (FileRead.Contains(name) || FileWrite.Contains(name)))
            return new(EffectCategory.Filesystem, "File." + name, [0],
                a => PathAttrs(a[0], FileWrite.Contains(name) ? "write" : "read"));
        if (type == "System.IO.Directory" && (DirRead.Contains(name) || DirWrite.Contains(name)))
            return new(EffectCategory.Filesystem, "Directory." + name, [0],
                a => PathAttrs(a[0], DirWrite.Contains(name) ? "write" : "read"));

        // network
        if (type == "System.Net.Http.HttpClient" && HttpMethods.Contains(name)
            && m.Parameters.Count >= 1 && m.Parameters[0].ParameterType.FullName == "System.String")
            return new(EffectCategory.Network, "HttpClient." + name, [0], a => UriAttrs(a[0]));
        if (type == "System.Net.Http.HttpRequestMessage" && name == ".ctor"
            && m.Parameters.Count == 2 && m.Parameters[1].ParameterType.FullName == "System.String")
            return new(EffectCategory.Network, "HttpRequestMessage..ctor", [1], a => UriAttrs(a[0]));
        if (type == "System.Net.Sockets.TcpClient" && name == ".ctor"
            && m.Parameters.Count == 2
            && m.Parameters[0].ParameterType.FullName == "System.String"
            && m.Parameters[1].ParameterType.FullName == "System.Int32")
            return new(EffectCategory.Network, "TcpClient..ctor", [0, 1],
                a => a is [Value.Str host, Value.Int port, ..]
                    ? Attrs(("direction", "outbound"), ("host", host.V.ToLowerInvariant()), ("port", port.V))
                    : null);
        // Simple-name heuristic: covers System.Data/Microsoft.Data SqlClient without a
        // package dependency. Documented PoC heuristic, revisited with real usage.
        if (m.DeclaringType.Name == "SqlConnection" && name == ".ctor"
            && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.String")
            return new(EffectCategory.Network, "SqlConnection..ctor", [0], a => ConnectionStringAttrs(a[0]));

        return null;
    }

    private static IReadOnlyList<KeyValuePair<string, object>>? Executable(Value v) =>
        v is Value.Str s && s.V.Length > 0 ? Attrs(("executable", s.V)) : null;

    private static IReadOnlyList<KeyValuePair<string, object>>? PathAttrs(Value v, string access) =>
        v is Value.Str s && s.V.Length > 0 ? Attrs(("path", s.V), ("access", access)) : null;

    private static IReadOnlyList<KeyValuePair<string, object>>? UriAttrs(Value v)
    {
        if (v is not Value.Str s || !Uri.TryCreate(s.V, UriKind.Absolute, out var uri)) return null;
        if (uri.Scheme != "http" && uri.Scheme != "https") return null;
        return Attrs(("direction", "outbound"), ("host", uri.Host.ToLowerInvariant()), ("port", uri.Port));
    }

    private static IReadOnlyList<KeyValuePair<string, object>>? ConnectionStringAttrs(Value v)
    {
        if (v is not Value.Str s) return null;
        foreach (var part in s.V.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var eq = part.IndexOf('=');
            if (eq <= 0) continue;
            var key = part[..eq].Trim().ToLowerInvariant();
            if (key is not ("server" or "data source" or "address" or "addr")) continue;
            var value = part[(eq + 1)..].Trim();
            var comma = value.IndexOf(',');
            if (comma > 0 && int.TryParse(value[(comma + 1)..], out var port))
                return Attrs(("direction", "outbound"), ("host", value[..comma].ToLowerInvariant()), ("port", port));
            return Attrs(("direction", "outbound"), ("host", value.ToLowerInvariant()));
        }

        return null;
    }

    private static IReadOnlyList<KeyValuePair<string, object>> Attrs(params (string Key, object Value)[] pairs) =>
        pairs.Select(p => new KeyValuePair<string, object>(p.Key, p.Value)).ToList();
}
