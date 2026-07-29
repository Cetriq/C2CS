using C2CS.Extractor.Core;
using C2CS.Extractor.DotNet;

if (args.Length < 1 || args[0] is "-h" or "--help")
{
    Console.WriteLine("""
        c2cs-extract — .NET static extractor (PoC)

        Usage: c2cs-extract <assembly.dll> [options]
          --out <dir>         output directory (default: .)
          --registry <ver>    registry version (default: 0.1)
          --contract <file>   contract document to reference (subject + digest)
        Outputs: assessment.c2cs.yaml (normative), extraction-report.json (informative)
        """);
    return args.Length < 1 ? 1 : 0;
}

var assemblyPath = args[0];
string outDir = ".", registry = "0.1";
string? contractPath = null;
for (var i = 1; i < args.Length - 1; i++)
{
    if (args[i] == "--out") outDir = args[i + 1];
    if (args[i] == "--registry") registry = args[i + 1];
    if (args[i] == "--contract") contractPath = args[i + 1];
}

if (!File.Exists(assemblyPath))
{
    Console.Error.WriteLine($"error: {assemblyPath} not found");
    return 1;
}

string? contractSubject = null, contractDigest = null;
if (contractPath is not null)
{
    var bytes = File.ReadAllBytes(contractPath);
    contractDigest = "sha256:" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
    contractSubject = File.ReadLines(contractPath)
        .FirstOrDefault(l => l.TrimStart().StartsWith("logical:"))?.Split(':', 2)[1].Trim();
}

var result = Scanner.Analyze(assemblyPath);
var assessment = AssessmentWriter.Write(result, registry, contractSubject, contractDigest);
var specFindings = ReportWriter.CollectSpecFindings(usedBootstrapContract: contractPath is null);
var report = ReportWriter.Write(result, specFindings);

Directory.CreateDirectory(outDir);
var assessmentPath = Path.Combine(outDir, "assessment.c2cs.yaml");
var reportPath = Path.Combine(outDir, "extraction-report.json");
File.WriteAllText(assessmentPath, assessment);
File.WriteAllText(reportPath, report);

Console.WriteLine($"analyzed   {result.AssemblyName} {result.AssemblyVersion} ({result.Artifact.Sha256Digest[..19]}…)");
Console.WriteLine($"coverage   {result.Coverage.TypesScanned} types, {result.Coverage.MethodsScanned} methods, {result.Coverage.CallSitesInspected} relevant call sites");
Console.WriteLine($"claims     {result.Claims.Count} resolved, {result.Unresolved.Count} unresolved (see report)");
Console.WriteLine($"wrote      {assessmentPath}");
Console.WriteLine($"wrote      {reportPath}");
return 0;
