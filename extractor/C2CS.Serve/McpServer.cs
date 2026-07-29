using System.Text.Json;
using System.Text.Json.Nodes;

namespace C2CS.Serve;

/// <summary>
/// Minimal MCP server: newline-delimited JSON-RPC 2.0 over stdio. Hand-rolled rather
/// than SDK-based to keep the reference tooling dependency-light and deterministic;
/// implements initialize, tools/list, tools/call, and ping.
/// </summary>
public sealed class McpServer(Tools tools, string modelDirectory)
{
    private const string ProtocolVersion = "2025-06-18";

    public void Run(TextReader input, TextWriter output)
    {
        string? line;
        while ((line = input.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            JsonNode? message;
            try { message = JsonNode.Parse(line); }
            catch { continue; }
            if (message is null) continue;

            var id = message["id"];
            var method = message["method"]?.GetValue<string>();
            if (method is null) continue;
            if (id is null) continue; // notification — nothing requires action in this server

            JsonNode response;
            try
            {
                response = Handle(method, message["params"]);
            }
            catch (Exception e)
            {
                response = Error(-32603, e.Message);
            }

            var envelope = new JsonObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = id.DeepClone(),
            };
            if (response is JsonObject o && o.ContainsKey("__error"))
                envelope["error"] = o["__error"]!.DeepClone();
            else
                envelope["result"] = response;
            output.WriteLine(envelope.ToJsonString(new JsonSerializerOptions { WriteIndented = false }));
            output.Flush();
        }
    }

    private JsonNode Handle(string method, JsonNode? @params) => method switch
    {
        "initialize" => new JsonObject
        {
            ["protocolVersion"] = ProtocolVersion,
            ["capabilities"] = new JsonObject { ["tools"] = new JsonObject() },
            ["serverInfo"] = new JsonObject
            {
                ["name"] = "c2cs-serve",
                ["version"] = "0.1.0",
            },
            ["instructions"] =
                "Serves the C2CS semantic model (contract, assessments, verdicts) for "
                + modelDirectory
                + ". Start with c2cs_overview. Before implementing changes that touch network, "
                + "filesystem, process, or environment behavior, call c2cs_check_action.",
        },
        "ping" => new JsonObject(),
        "tools/list" => new JsonObject { ["tools"] = ToolDescriptors() },
        "tools/call" => CallTool(@params),
        _ => Error(-32601, $"method not found: {method}"),
    };

    private static JsonNode Error(int code, string message) =>
        new JsonObject { ["__error"] = new JsonObject { ["code"] = code, ["message"] = message } };

    private JsonNode CallTool(JsonNode? @params)
    {
        var name = @params?["name"]?.GetValue<string>();
        var args = @params?["arguments"] as JsonObject;
        var text = name switch
        {
            "c2cs_overview" => tools.Overview(),
            "c2cs_contract" => tools.Contract(),
            "c2cs_trust_status" => tools.TrustStatus(),
            "c2cs_pending_hypotheses" => tools.PendingHypotheses(),
            "c2cs_check_action" => tools.CheckAction(
                args?["category"]?.GetValue<string>() ?? "",
                ToScope(args?["attributes"] as JsonObject)),
            _ => null,
        };
        if (text is null) return Error(-32602, $"unknown tool: {name}");

        return new JsonObject
        {
            ["content"] = new JsonArray(new JsonObject { ["type"] = "text", ["text"] = text }),
            ["isError"] = false,
        };
    }

    /// <summary>JSON arguments → the untyped string-scalar shape the matcher speaks.</summary>
    private static Dictionary<object, object> ToScope(JsonObject? attributes)
    {
        var scope = new Dictionary<object, object>();
        foreach (var kv in attributes ?? [])
        {
            scope[kv.Key] = kv.Value switch
            {
                JsonArray arr => arr.Select(x => (object)(x?.ToString() ?? "")).ToList(),
                { } v => v.ToString(),
                null => "",
            };
        }

        return scope;
    }

    private static JsonArray ToolDescriptors()
    {
        static JsonObject Tool(string name, string description, JsonObject properties, string[] required) => new()
        {
            ["name"] = name,
            ["description"] = description,
            ["inputSchema"] = new JsonObject
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = new JsonArray(required.Select(r => (JsonNode)r).ToArray()),
            },
        };

        return
        [
            Tool("c2cs_overview",
                "What am I looking at? Subject, document family, contract mode, and current trust status of the C2CS semantic model. Start here.",
                [], []),
            Tool("c2cs_contract",
                "The declared contract: operations, data entities and classifications, capabilities (what MAY happen, with rationale), and prohibitions (what MUST NOT happen, with rationale).",
                [], []),
            Tool("c2cs_check_action",
                "May the system do X? Checks a proposed action against declared capabilities and prohibitions using the registry matcher semantics. Call BEFORE implementing behavior changes. Example: category=network, attributes={direction:outbound, host:api.vendor.example, port:443}.",
                new JsonObject
                {
                    ["category"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["enum"] = new JsonArray("network", "filesystem", "process", "environment"),
                    },
                    ["attributes"] = new JsonObject
                    {
                        ["type"] = "object",
                        ["description"] = "Matcher attributes per registry category: network {direction,host,port}; filesystem {path,access}; process {executable}; environment {variable}.",
                    },
                },
                ["category", "attributes"]),
            Tool("c2cs_trust_status",
                "How trustworthy is the declared picture right now? Latest verdict outcome, observation coverage per category, violations, drift, and unexercised claims.",
                [], []),
            Tool("c2cs_pending_hypotheses",
                "Inferred (machine-hypothesized) claims awaiting human promotion review, with confidence and source, marked as covered or missing in the contract.",
                [], []),
        ];
    }
}
