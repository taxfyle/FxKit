using System.Collections.Immutable;
using System.Text;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace FxKit.CompilerServices.CodeGenerators.EnumMatch;

/// <summary>
///     Generates a Match extension for the enum.
/// </summary>
[Generator]
public class EnumMatchGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter for enums.
        var enumDeclarations =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "FxKit.CompilerServices.EnumMatchAttribute",
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (ctx, _) => (EnumDeclarationSyntax)ctx.TargetNode);

        // Combine the selected enums with the Compilation.
        var compilationAndEnums = context.CompilationProvider.Combine(enumDeclarations.Collect());

        // Generate the source using the compilation and enums.
        context.RegisterSourceOutput(
            compilationAndEnums,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    /// <summary>
    ///     Filter based on the node type. Only enums are chosen that have more than 1 attribute.
    /// </summary>
    /// <param name="syntaxNode"></param>
    /// <returns></returns>
    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode) =>
        syntaxNode is EnumDeclarationSyntax;

    /// <summary>
    ///     Generates the code.
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="enumDeclarations"></param>
    /// <param name="ctx"></param>
    private static void Execute(
        Compilation compilation,
        ImmutableArray<EnumDeclarationSyntax> enumDeclarations,
        SourceProductionContext ctx)
    {
        if (enumDeclarations.IsDefaultOrEmpty)
        {
            return;
        }

        var enumsToGenerate = GetTypesToGenerate(compilation, enumDeclarations, ctx.CancellationToken);
        if (enumsToGenerate.Count == 0)
        {
            return;
        }

        var result = GenerateMatchExtensionClasses(enumsToGenerate, ctx.CancellationToken);
        foreach (var (fileHint, source) in result)
        {
            ctx.AddSource(fileHint, source);
        }
    }

    /// <summary>
    ///     Generates the extension classes.
    /// </summary>
    /// <param name="enumsToGenerate"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private static IReadOnlyList<(string FileHint, string Source)> GenerateMatchExtensionClasses(
        IReadOnlyList<EnumToGenerate> enumsToGenerate,
        CancellationToken ct)
    {
        var result = new List<(string, string)>(enumsToGenerate.Count);
        foreach (var enumToGenerate in enumsToGenerate)
        {
            ct.ThrowIfCancellationRequested();
            result.Add(
                ($"{enumToGenerate.HintName}.g.cs",
                    GenerateMatchExtensionClass(enumToGenerate)));
        }

        return result;
    }

    /// <summary>
    ///     Transforms the enum declarations to a simple structure.
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="enums"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private static IReadOnlyList<EnumToGenerate> GetTypesToGenerate(
        Compilation compilation,
        IEnumerable<EnumDeclarationSyntax?> enums,
        CancellationToken ct)
    {
        var enumsToGenerate = new List<EnumToGenerate>();
        foreach (var enumDecl in enums)
        {
            ct.ThrowIfCancellationRequested();
            if (enumDecl is null)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(enumDecl.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(enumDecl, ct) is not INamedTypeSymbol enumSymbol)
            {
                continue;
            }

            // Get the full type name, including any outer type declaration.
            var enumName = enumSymbol.ToDisplayString();

            // Hint name is used for the generated file, and must be unique.
            var hintName = enumSymbol.GetFullyQualifiedMetadataName();

            // Get the enum fields.
            var enumMembers = enumSymbol.GetMembers();
            var members = new List<string>(enumMembers.Length);

            foreach (var member in enumMembers)
            {
                if (member is IFieldSymbol)
                {
                    members.Add(member.Name);
                }
            }

            enumsToGenerate.Add(
                new EnumToGenerate(
                    name: enumName,
                    hintName: hintName,
                    identifier: enumDecl.Identifier.Text,
                    containingNamespace: enumSymbol.ContainingNamespace.ToDisplayString(),
                    members: members));
        }

        return enumsToGenerate;
    }

    /// <summary>
    ///     Generate the extension class.
    /// </summary>
    /// <param name="enumToGenerate"></param>
    /// <returns></returns>
    private static string GenerateMatchExtensionClass(EnumToGenerate enumToGenerate)
    {
        var sb = new StringBuilder(256);
        sb.AppendLine(SourceGenerationHelper.AutoGeneratedHeader);
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.Append("namespace ").Append(enumToGenerate.ContainingNamespace).AppendLine(";");
        sb.Append(
                @"
public static partial class ")
            .Append(enumToGenerate.Identifier)
            .Append(
                @"MatchExtension
{
    /// <summary>
    ///     Perform an exhaustive match on the enum value.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static TMatchResult Match<TMatchResult>(
        this ")
            .Append(enumToGenerate.Name)
            .Append(" source,");

        // Generate a parameter for each enum field.
        var lastIndex = enumToGenerate.Members.Count - 1;
        for (var index = 0; index < enumToGenerate.Members.Count; index++)
        {
            var member = enumToGenerate.Members[index];
            sb.Append(
                    @"
        Func<TMatchResult> ")
                .Append(member);
            if (index != lastIndex)
            {
                sb.Append(',');
            }
        }

        sb.AppendLine(
            @") => source switch
        {");

        foreach (var member in enumToGenerate.Members)
        {
            sb.Append(@"            ")
                .Append(enumToGenerate.Name)
                .Append('.')
                .Append(member)
                .Append(" => ")
                .Append(member)
                .AppendLine("(),");
        }

        sb.Append(
                @"            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };

    /// <summary>
    ///     Perform an exhaustive match on the enum value.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static TMatchResult Match<TMatchResult>(
        this ")
            .Append(enumToGenerate.Name)
            .Append(" source,");

        // Generate a parameter for each enum field.
        for (var index = 0; index < enumToGenerate.Members.Count; index++)
        {
            var member = enumToGenerate.Members[index];
            sb.Append(
                    @"
        TMatchResult ")
                .Append(member);
            if (index != lastIndex)
            {
                sb.Append(',');
            }
        }

        sb.AppendLine(
            @") => source switch
        {");

        foreach (var member in enumToGenerate.Members)
        {
            sb.Append(@"            ")
                .Append(enumToGenerate.Name)
                .Append('.')
                .Append(member)
                .Append(" => ")
                .Append(member)
                .AppendLine(",");
        }

        sb.Append(
            @"            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
}
");

        return sb.ToString();
    }

    /// <summary>
    ///     An enum to generate a Match for.
    /// </summary>
    private readonly struct EnumToGenerate(
        string name,
        string hintName,
        string identifier,
        string containingNamespace,
        IReadOnlyList<string> members)
    {
        public readonly string                Name                = name;
        public readonly string                HintName            = hintName;
        public readonly string                Identifier          = identifier;
        public readonly string                ContainingNamespace = containingNamespace;
        public readonly IReadOnlyList<string> Members             = members;
    }
}
