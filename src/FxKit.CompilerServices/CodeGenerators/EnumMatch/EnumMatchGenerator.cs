using System.Runtime.CompilerServices;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        // Transform enums to generate.
        var enumGenerations =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "FxKit.CompilerServices.EnumMatchAttribute",
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (ctx, _) => TransformEnumGeneration(ctx));

        // Generate source.
        context.RegisterSourceOutput(
            enumGenerations,
            static (spc, enumGeneration) => Execute(spc, enumGeneration));
    }

    /// <summary>
    ///     Creates an enum generation model based on the context provided.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private static EnumGeneration? TransformEnumGeneration(
        GeneratorAttributeSyntaxContext ctx)
    {
        var enumSymbol = Unsafe.As<INamedTypeSymbol>(ctx.TargetSymbol);
        var members = enumSymbol.MemberNames.ToEquatableArray();
        if (members.Length == 0)
        {
            return null;
        }

        return new EnumGeneration(
            FullyQualifiedName: enumSymbol.ToDisplayString(),
            Name: enumSymbol.Name,
            HintName: enumSymbol.GetFullyQualifiedMetadataName(),
            ContainingNamespace: enumSymbol.ContainingNamespace.ToDisplayString(),
            Accessibility: SyntaxFacts.GetText(enumSymbol.DeclaredAccessibility),
            Members: members);
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
    /// <param name="ctx"></param>
    /// <param name="enumGeneration"></param>
    private static void Execute(SourceProductionContext ctx, EnumGeneration? enumGeneration)
    {
        if (enumGeneration is null)
        {
            return;
        }

        ctx.AddSource(
            $"{enumGeneration.HintName}.g.cs",
            EnumMatchSyntaxBuilder.GenerateMatchExtensionClass(enumGeneration));
    }
}

/// <summary>
///     An enum to generate a Match for.
/// </summary>
internal sealed record EnumGeneration(
    string FullyQualifiedName,
    string Name,
    string HintName,
    string ContainingNamespace,
    string Accessibility,
    EquatableArray<string> Members);
