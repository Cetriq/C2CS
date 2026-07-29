using YamlDotNet.Serialization;

namespace C2CS.Serve;

/// <summary>
/// Untyped YAML loading. All scalars arrive as strings (YamlDotNet's untyped behavior);
/// helpers convert on demand. Deterministic, no schema assumptions beyond shape.
/// </summary>
public static class Yaml
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().Build();

    public static Dictionary<object, object> LoadFile(string path) =>
        (Dictionary<object, object>)Deserializer.Deserialize<object>(File.ReadAllText(path))!;

    public static Dictionary<object, object>? Map(object? node) => node as Dictionary<object, object>;

    public static List<object> Seq(object? node) => node as List<object> ?? [];

    public static string? Str(object? node) => node as string;

    public static string? Get(object? node, string key) =>
        Map(node)?.TryGetValue(key, out var v) == true ? v as string : null;

    public static object? GetNode(object? node, string key) =>
        Map(node)?.TryGetValue(key, out var v) == true ? v : null;

    public static int? Int(object? node) =>
        node is string s && int.TryParse(s, out var i) ? i : null;

    public static bool? Bool(object? node) =>
        node is string s && bool.TryParse(s, out var b) ? b : null;
}
