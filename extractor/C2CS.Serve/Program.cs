using C2CS.Serve;

var modelDir = ".";
for (var i = 0; i < args.Length; i++)
{
    if (args[i] is "-h" or "--help")
    {
        Console.WriteLine("""
            c2cs-serve — MCP server for a C2CS semantic model (PoC)

            Usage: c2cs-serve --model-dir <dir>
              <dir> contains a C2CS document family: a contract plus any assessments
              and verdicts (*.yaml, identified by their kind field).

            Speaks MCP (newline-delimited JSON-RPC 2.0) on stdio. Tools:
              c2cs_overview, c2cs_contract, c2cs_check_action,
              c2cs_trust_status, c2cs_pending_hypotheses
            """);
        return 0;
    }

    if (args[i] == "--model-dir" && i + 1 < args.Length) modelDir = args[i + 1];
}

if (!Directory.Exists(modelDir))
{
    Console.Error.WriteLine($"error: model directory not found: {modelDir}");
    return 1;
}

var store = new ModelStore(modelDir);
if (store.Contract is null)
    Console.Error.WriteLine($"warning: no contract document found in {modelDir} — serving assessments/verdicts only");

new McpServer(new Tools(store), Path.GetFullPath(modelDir)).Run(Console.In, Console.Out);
return 0;
