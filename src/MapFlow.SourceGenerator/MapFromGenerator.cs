using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MapFlow.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class MapFromGenerator : IIncrementalGenerator
{
    private const string IMapFromName = "IMapFrom";
    private const string IMapToName = "IMapTo";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var fromCandidates = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => IsInterfaceCandidate(node, IMapFromName),
            transform: GetMapFromTarget)
            .Where(static m => m is not null);

        var toCandidates = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => IsInterfaceCandidate(node, IMapToName),
            transform: GetMapToTarget)
            .Where(static m => m is not null);

        context.RegisterSourceOutput(fromCandidates, (spc, target) => GenerateFromCode(spc, target!));
        context.RegisterSourceOutput(toCandidates, (spc, target) => GenerateToCode(spc, target!));
    }

    // ─── Syntax predicate ────────────────────────────────────────

    private static bool IsInterfaceCandidate(SyntaxNode node, string interfaceName)
    {
        if (node is not TypeDeclarationSyntax typeDecl)
            return false;

        if (!typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            return false;

        if (typeDecl.BaseList is null)
            return false;

        foreach (var baseType in typeDecl.BaseList.Types)
        {
            if (baseType.Type is SimpleNameSyntax name
                && name.Identifier.ValueText == interfaceName)
                return true;
        }

        return false;
    }

    // ─── Resolve keyword ─────────────────────────────────────────

    private static string GetTypeKeyword(TypeDeclarationSyntax typeDecl)
    {
        return typeDecl.Kind() switch
        {
            SyntaxKind.ClassDeclaration => "class",
            SyntaxKind.StructDeclaration => "struct",
            SyntaxKind.RecordDeclaration => "record",
            SyntaxKind.RecordStructDeclaration => "record struct",
            _ => "class"
        };
    }

    // ─── IMapFrom ────────────────────────────────────────────────

    private static MapFromTarget? GetMapFromTarget(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var typeDecl = (TypeDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        if (model.GetDeclaredSymbol(typeDecl, ct) is not INamedTypeSymbol destSymbol)
            return null;

        var mapFromInterface = destSymbol.AllInterfaces
            .FirstOrDefault(i => i.OriginalDefinition is
            {
                Name: IMapFromName,
                ContainingNamespace: { Name: "MapFlow" }
            });

        if (mapFromInterface is null)
            return null;

        var sourceType = mapFromInterface.TypeArguments[0];
        if (sourceType is not INamedTypeSymbol sourceNamed)
            return null;

        // Skip if user already wrote MapFrom (implicit or explicit)
        if (HasManualImplementation(destSymbol, mapFromInterface, "MapFrom", 1))
            return null;

        var keyword = GetTypeKeyword(typeDecl);
        var ns = destSymbol.ContainingNamespace?.ToDisplayString() ?? "";
        var className = typeDecl.Identifier.Text;
        var typeParams = typeDecl.TypeParameterList?.ToString() ?? "";
        var constraints = typeDecl.ConstraintClauses.ToString();
        var sourceFull = sourceNamed.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var matches = GetMatchingProperties(destSymbol, sourceNamed);

        return new MapFromTarget(ns, className, typeParams, constraints, keyword, sourceFull, matches);
    }

    // ─── IMapTo ──────────────────────────────────────────────────

    private static MapFromTarget? GetMapToTarget(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var typeDecl = (TypeDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        if (model.GetDeclaredSymbol(typeDecl, ct) is not INamedTypeSymbol sourceSymbol)
            return null;

        var mapToInterface = sourceSymbol.AllInterfaces
            .FirstOrDefault(i => i.OriginalDefinition is
            {
                Name: IMapToName,
                ContainingNamespace: { Name: "MapFlow" }
            });

        if (mapToInterface is null)
            return null;

        var destType = mapToInterface.TypeArguments[0];
        if (destType is not INamedTypeSymbol destNamed)
            return null;

        // Skip if user already wrote MapTo (implicit or explicit)
        if (HasManualImplementation(sourceSymbol, mapToInterface, "MapTo", 0))
            return null;

        var keyword = GetTypeKeyword(typeDecl);
        var ns = sourceSymbol.ContainingNamespace?.ToDisplayString() ?? "";
        var className = typeDecl.Identifier.Text;
        var typeParams = typeDecl.TypeParameterList?.ToString() ?? "";
        var constraints = typeDecl.ConstraintClauses.ToString();
        var destFull = destNamed.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Source readable → dest writable (reverse of IMapFrom)
        var srcProps = GetReadableProperties(sourceSymbol);
        var destProps = GetWritableProperties(destNamed);

        var matches = new List<string>(srcProps.Count);
        foreach (var kvp in srcProps)
        {
            var name = kvp.Key;
            var srcProp = kvp.Value;

            if (destProps.TryGetValue(name, out var destProp)
                && SymbolEqualityComparer.Default.Equals(srcProp.Type, destProp.Type))
            {
                matches.Add(name);
            }
        }

        return new MapFromTarget(ns, className, typeParams, constraints, keyword, destFull, matches);
    }

    // ─── Shared helpers ──────────────────────────────────────────

    private static bool HasManualImplementation(
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol interfaceSymbol,
        string methodName,
        int expectedParams)
    {
        // Check for implicit implementations (user wrote the method directly)
        if (typeSymbol.GetMembers(methodName)
                .OfType<IMethodSymbol>()
                .Any(m => m is { Parameters.Length: >= 0, IsStatic: false }))
            return true;

        // Check for explicit interface implementations
        var interfaceMethod = interfaceSymbol.GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault();

        if (interfaceMethod is not null)
        {
            var impl = typeSymbol.FindImplementationForInterfaceMember(interfaceMethod);
            if (impl is IMethodSymbol method && method.ExplicitInterfaceImplementations.Length > 0)
                return true;
        }

        return false;
    }

    private static List<string> GetMatchingProperties(
        INamedTypeSymbol destSymbol,
        INamedTypeSymbol sourceNamed)
    {
        var destProps = GetWritableProperties(destSymbol);
        var sourceProps = GetReadableProperties(sourceNamed);

        var matches = new List<string>(destProps.Count);
        foreach (var kvp in destProps)
        {
            var name = kvp.Key;
            var destProp = kvp.Value;

            if (sourceProps.TryGetValue(name, out var sourceProp)
                && SymbolEqualityComparer.Default.Equals(destProp.Type, sourceProp.Type))
            {
                matches.Add(name);
            }
        }

        return matches;
    }

    private static Dictionary<string, IPropertySymbol> GetWritableProperties(INamedTypeSymbol symbol)
    {
        return symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && !p.IsIndexer && p.SetMethod is not null)
            .ToDictionary(p => p.Name);
    }

    private static Dictionary<string, IPropertySymbol> GetReadableProperties(INamedTypeSymbol symbol)
    {
        return symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && !p.IsIndexer && p.GetMethod is not null)
            .ToDictionary(p => p.Name);
    }

    // ─── Code generation ─────────────────────────────────────────

    private static void GenerateFromCode(SourceProductionContext spc, MapFromTarget target)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(target.Namespace))
        {
            sb.AppendLine($"namespace {target.Namespace};");
            sb.AppendLine();
        }

        var head = $"partial {target.Keyword} {target.ClassName}{target.TypeParams}";
        if (!string.IsNullOrEmpty(target.Constraints))
            head += $" {target.Constraints}";

        sb.AppendLine(head);
        sb.AppendLine("{");
        sb.AppendLine($"    public void MapFrom({target.SourceOrDest} source)");
        sb.AppendLine("    {");

        foreach (var prop in target.Properties)
        {
            sb.AppendLine($"        this.{prop} = source.{prop};");
        }

        sb.AppendLine();
        sb.AppendLine("        CustomMapFrom(source);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    partial void CustomMapFrom({target.SourceOrDest} source);");
        sb.AppendLine("}");

        spc.AddSource(
            $"{target.ClassName}.MapFrom.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static void GenerateToCode(SourceProductionContext spc, MapFromTarget target)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(target.Namespace))
        {
            sb.AppendLine($"namespace {target.Namespace};");
            sb.AppendLine();
        }

        var head = $"partial {target.Keyword} {target.ClassName}{target.TypeParams}";
        if (!string.IsNullOrEmpty(target.Constraints))
            head += $" {target.Constraints}";

        sb.AppendLine(head);
        sb.AppendLine("{");
        sb.AppendLine($"    public {target.SourceOrDest} MapTo()");
        sb.AppendLine("    {");
        sb.AppendLine($"        var destination = new {target.SourceOrDest}();");

        foreach (var prop in target.Properties)
        {
            sb.AppendLine($"        destination.{prop} = this.{prop};");
        }

        sb.AppendLine();
        sb.AppendLine("        CustomMapTo(ref destination);");
        sb.AppendLine("        return destination;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    partial void CustomMapTo(ref {target.SourceOrDest} destination);");
        sb.AppendLine("}");

        spc.AddSource(
            $"{target.ClassName}.MapTo.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}

internal sealed class MapFromTarget
{
    public string Namespace { get; }
    public string ClassName { get; }
    public string TypeParams { get; }
    public string Constraints { get; }
    public string Keyword { get; }
    public string SourceOrDest { get; }
    public List<string> Properties { get; }

    public MapFromTarget(
        string ns,
        string className,
        string typeParams,
        string constraints,
        string keyword,
        string sourceOrDest,
        List<string> properties)
    {
        Namespace = ns;
        ClassName = className;
        TypeParams = typeParams;
        Constraints = constraints;
        Keyword = keyword;
        SourceOrDest = sourceOrDest;
        Properties = properties;
    }
}
