using System.Security.Cryptography;
using C2CS.Extractor.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CallSite = C2CS.Extractor.Core.CallSite;

namespace C2CS.Extractor.DotNet;

/// <summary>
/// The .NET extractor: Cecil-based IL scan over one assembly, producing resolved claims
/// (only what can be stated per the registry matcher grammar) and unresolved findings
/// (everything relevant that cannot). Deterministic by construction: no time, no
/// randomness, ordered traversal.
/// </summary>
public static class Scanner
{
    public static ExtractionResult Analyze(string assemblyPath)
    {
        var digest = "sha256:" + Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assemblyPath))).ToLowerInvariant();
        using var module = ModuleDefinition.ReadModule(assemblyPath);

        var claims = new List<ResolvedClaim>();
        var unresolved = new List<UnresolvedFinding>();
        int types = 0, methods = 0, callSites = 0;

        foreach (var type in module.GetTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
        {
            types++;
            foreach (var method in type.Methods.OrderBy(m => m.FullName, StringComparer.Ordinal))
            {
                if (!method.HasBody) continue;
                methods++;
                var sim = new StackSimulator();
                sim.Run(method, (ins, stack) =>
                {
                    var target = (MethodReference)ins.Operand;
                    var match = Patterns.Match(target);
                    if (match is null) return;
                    callSites++;
                    var site = new CallSite(
                        method.DeclaringType.FullName + "::" + method.Name,
                        $"IL_{ins.Offset:x4}",
                        match.Api);
                    Handle(match, target, stack, site, claims, unresolved);
                });
            }
        }

        var name = module.Assembly.Name;
        return new ExtractionResult(
            new Artifact(Path.GetFileName(assemblyPath), digest),
            name.Name,
            name.Version?.ToString() ?? "0.0.0",
            claims,
            unresolved,
            new ExtractionCoverage(types, methods, callSites));
    }

    private static void Handle(
        Patterns.Pattern match,
        MethodReference target,
        IReadOnlyList<Value> stack,
        CallSite site,
        List<ResolvedClaim> claims,
        List<UnresolvedFinding> unresolved)
    {
        var args = match.ArgIndices.Select(i => StackSimulator.Argument(stack, target, i)).ToList();

        if (args.Any(a => a is Value.Param))
        {
            unresolved.Add(new UnresolvedFinding(match.Category, site,
                "argument is a method parameter — possible wrapper; interprocedural analysis is out of PoC scope"));
            return;
        }

        if (args.Any(a => a is not Value.Str and not Value.Int))
        {
            unresolved.Add(new UnresolvedFinding(match.Category, site,
                "argument not statically resolvable"));
            return;
        }

        var attributes = match.BuildAttributes(args);
        if (attributes is null)
        {
            unresolved.Add(new UnresolvedFinding(match.Category, site,
                "resolved value could not be expressed in the registry matcher grammar"));
            return;
        }

        var derived = args.Any(a => a is Value.Str { Derived: true });
        var confidence = derived ? ConfidenceTiers.DerivedConstant : ConfidenceTiers.DirectConstant;
        claims.Add(new ResolvedClaim(match.Category, attributes, confidence, site));
    }
}
