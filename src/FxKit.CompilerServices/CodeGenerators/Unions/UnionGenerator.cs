using System.Runtime.CompilerServices;
using FxKit.CompilerServices.Models;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FxKit.CompilerServices.CodeGenerators.Unions;

/// <summary>
///     Generates a Discriminated Union using nested records inside a partial record tagged
///     with the [Union] attribute.
/// </summary>
[Generator]
public class UnionGenerator : IIncrementalGenerator
{
    /// <summary>
    ///     The fully qualified name of the Union attribute.
    /// </summary>
    public const string UnionAttrName = "FxKit.CompilerServices.UnionAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Transform unions to generate.
        var unionsToGenerate =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: UnionAttrName,
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (ctx, ct) => TransformUnionGeneration(ctx, ct));

        // Generate source.
        context.RegisterSourceOutput(
            unionsToGenerate,
            static (ctx, union) => Execute(ctx, union));
    }

    /// <summary>
    ///     Transforms the context into a union to generate.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static UnionGeneration? TransformUnionGeneration(
        GeneratorAttributeSyntaxContext ctx,
        CancellationToken cancellationToken)
    {
        var recordDeclaration = Unsafe.As<RecordDeclarationSyntax>(ctx.TargetNode);
        var recordSymbol = Unsafe.As<INamedTypeSymbol>(ctx.TargetSymbol);

        // Check that the record does not declare a base type.
        // The fastest way to do this is to check if there are any nodes in the base type list,
        // it is not 100% accurate however it is fast to check.
        if (recordDeclaration.BaseList is { Types.Count: > 0 })
        {
            // If the base type is not System.Object, then it is something user-defined.
            if (recordSymbol is { BaseType.SpecialType: not SpecialType.System_Object })
            {
                return null;
            }
        }

        using var constructors = new ImmutableArrayBuilder<UnionMember>();

        foreach (var nestedRecordDecl in recordDeclaration.Members.OfType<RecordDeclarationSyntax>())
        {
            // Only include if partial.
            if (!nestedRecordDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                continue;
            }

            var constructorParameters = nestedRecordDecl is { ParameterList.Parameters.Count: 0 }
                ? EquatableArray<BasicParameter>.Empty
                : CollectRecordConstructorParameters(
                    nestedRecordDecl,
                    ctx.SemanticModel,
                    cancellationToken);

            constructors.Add(
                new UnionMember(
                    MemberName: nestedRecordDecl.Identifier.ValueText,
                    MemberNameWithSignature: TypeHierarchyHelper.GetIdentifierWithSignature(
                        nestedRecordDecl),
                    Parameters: constructorParameters));
        }

        var ancestorTypeHierarchy = TypeHierarchyHelper.GetTypeHierarchy(recordDeclaration);

        return new UnionGeneration(
            Accessibility: SyntaxFacts.GetText(recordSymbol.DeclaredAccessibility),
            UnionName: recordSymbol.Name,
            UnionNameWithSignature: TypeHierarchyHelper.GetIdentifierWithSignature(recordDeclaration),
            HintName: recordSymbol.GetFullyQualifiedMetadataName(),
            UnionNamespace: recordSymbol.ContainingNamespace.ToDisplayString(),
            AncestorTypeHierarchy: ancestorTypeHierarchy,
            Members: new EquatableArray<UnionMember>(constructors.ToArray()));
    }

    /// <summary>
    ///     Collects the record's primary constructor parameters.
    /// </summary>
    /// <param name="recordDeclarationSyntax"></param>
    /// <param name="semanticModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private static EquatableArray<BasicParameter> CollectRecordConstructorParameters(
        RecordDeclarationSyntax recordDeclarationSyntax,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var symbol = semanticModel.GetDeclaredSymbol(recordDeclarationSyntax, cancellationToken);

        var constructor = symbol?.InstanceConstructors.FirstOrDefault(
            static c => c.DeclaredAccessibility == Accessibility.Public);
        return constructor is null
            ? EquatableArray<BasicParameter>.Empty
            // First parameter is the type itself.
            : constructor.Parameters.Select(BasicParameter.FromSymbol).ToEquatableArray();
    }

    /// <summary>
    ///     Only include type declarations that are records which have the "partial" modifier
    ///     and do not have the "sealed" modifier.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        if (node is not RecordDeclarationSyntax
            {
                // Less than or exactly 2 modifiers (access + partial).
                Modifiers.Count: <= 2
            } recordDecl)
        {
            return false;
        }

        // Don't allow parameters in the primary constructor.
        if (recordDecl.ParameterList?.Parameters.Count > 0)
        {
            return false;
        }

        var isPartial = false;
        foreach (var modifier in recordDecl.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PartialKeyword))
            {
                isPartial = true;
            }

            // Sealed? Bail.
            if (modifier.IsKind(SyntaxKind.SealedKeyword))
            {
                return false;
            }
        }

        // It only matches if the type is marked as partial.
        return isPartial;
    }

    /// <summary>
    ///     Generates the source code for the union.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="union"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void Execute(SourceProductionContext ctx, UnionGeneration? union)
    {
        if (union is null)
        {
            return;
        }

        var source = UnionSyntaxBuilder.GenerateUnionMembers(union);
        ctx.AddSource($"{union.HintName}.g.cs", source);
    }
}

/// <summary>
///     A union to generate.
/// </summary>
/// <param name="Accessibility">
///     Declared accessibility of the union.
/// </param>
/// <param name="UnionName">
///     The name of the union type.
/// </param>
/// <param name="UnionNameWithSignature">
///     The name of the union type, including any type parameters.
/// </param>
/// <param name="UnionNamespace">
///     The containing namespace of the union.
/// </param>
/// <param name="HintName">
///     A hint name used for the generated file.
/// </param>
/// <param name="AncestorTypeHierarchy">
///     The type hierarchy of the union, excluding the union itself.
/// </param>
/// <param name="Members">
///     The union member constructors.
/// </param>
internal record UnionGeneration(
    string Accessibility,
    string UnionName,
    string UnionNameWithSignature,
    string UnionNamespace,
    string HintName,
    EquatableArray<TypeHierarchyNode> AncestorTypeHierarchy,
    EquatableArray<UnionMember> Members);

/// <summary>
///     A union member.
/// </summary>
internal record UnionMember(
    string MemberName,
    string MemberNameWithSignature,
    EquatableArray<BasicParameter> Parameters);
