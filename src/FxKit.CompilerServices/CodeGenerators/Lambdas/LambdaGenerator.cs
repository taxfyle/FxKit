using System.Runtime.CompilerServices;
using FxKit.CompilerServices.Models;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FxKit.CompilerServices.CodeGenerators.Lambdas;

/// <summary>
///     Generates lambda overloads for public constructors or methods marked with <c>Lambda</c> attribute
/// </summary>
[Generator]
public class LambdaGenerator : IIncrementalGenerator
{
    public const string LambdaAttrName = "FxKit.CompilerServices.LambdaAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate lambda method descriptors based on type declaration and method declaration syntax.
        var methodDescriptors =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                    fullyQualifiedMetadataName: LambdaAttrName,
                    predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                    transform: static (ctx, _) => TransformLambdaMethodDescriptor(ctx))
                .WithTrackingName("MethodDescriptors");

        // Group them by their containing type's metadata name.
        var groupedByType = methodDescriptors
            .Collect()
            .SelectMany(
                (methods, _) => methods
                    .SelectNotNull(static method => method)
                    .GroupBy(static method => method.FullyQualifiedContainingTypeMetadataName)
                    .Select(static g => new LambdaGenerationFile(g.Key, g.ToEquatableArray())))
            .WithTrackingName("Grouped");

        // Generate.
        context.RegisterSourceOutput(
            groupedByType,
            static (spc, source) => Execute(spc, source));
    }

    /// <summary>
    ///     Generates a lambda method descriptor based on the method or type the attribute
    ///     was placed on.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private static LambdaMethodDescriptor? TransformLambdaMethodDescriptor(
        GeneratorAttributeSyntaxContext ctx)
    {
        // The target can be either a type or method declaration.
        // For methods, we have already checked that they're static, and the type hierarchy is partial.
        // For types, we've filtered for only partial types and ancestries.

        // Methods
        if (ctx.TargetSymbol is IMethodSymbol methodSymbol)
        {
            var containingType = methodSymbol.ContainingType;
            var fullyQualifiedContainingTypeMetadataName =
                containingType.GetFullyQualifiedMetadataName();
            var parameters = methodSymbol.Parameters.Select(BasicParameter.FromSymbol)
                .ToEquatableArray();
            var methodSyntax = Unsafe.As<MethodDeclarationSyntax>(ctx.TargetNode);
            var hierarchy = TypeHierarchyHelper.GetTypeHierarchy(methodSyntax);

            // If the return type is the containing type, we don't need to use the FQN.
            var returnType = SymbolEqualityComparer.Default.Equals(
                x: methodSymbol.ReturnType,
                y: methodSymbol.ContainingType)
                ? methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                : methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            return new LambdaMethodDescriptor(
                FullyQualifiedContainingTypeMetadataName: fullyQualifiedContainingTypeMetadataName,
                Namespace: containingType.ContainingNamespace.ToDisplayString(),
                TypeHierarchy: hierarchy,
                Target: LambdaTarget.Method,
                TypeOrMethodName: methodSymbol.Name,
                Parameters: parameters,
                ReturnType: returnType);
        }

        // Type declaration.
        if (ctx.TargetSymbol is INamedTypeSymbol typeSymbol)
        {
            var typeDeclarationSyntax = Unsafe.As<TypeDeclarationSyntax>(ctx.TargetNode);
            var fullyQualifiedContainingTypeMetadataName =
                typeSymbol.GetFullyQualifiedMetadataName();

            // Get parameters from the first public constructor.
            var constructor = typeSymbol.InstanceConstructors.FirstOrDefault(
                static c => c.DeclaredAccessibility == Accessibility.Public);

            if (constructor is null)
            {
                return null;
            }

            var parameters = constructor.Parameters.Select(BasicParameter.FromSymbol).ToEquatableArray();
            var returnType = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var hierarchy = TypeHierarchyHelper.GetTypeHierarchy(
                node: typeDeclarationSyntax,
                includeSelf: true);

            return new LambdaMethodDescriptor(
                FullyQualifiedContainingTypeMetadataName: fullyQualifiedContainingTypeMetadataName,
                Namespace: typeSymbol.ContainingNamespace.ToDisplayString(),
                TypeHierarchy: hierarchy,
                Target: LambdaTarget.Constructor,
                TypeOrMethodName: typeSymbol.Name,
                Parameters: parameters,
                ReturnType: returnType);
        }

        return null;
    }

    /// <summary>
    ///     Generates the source code for the lambda methods.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="lambdaGeneration"></param>
    private static void Execute(SourceProductionContext ctx, LambdaGenerationFile lambdaGeneration)
    {
        var source = LambdaSyntaxBuilder.Generate(lambdaGeneration);
        ctx.AddSource(
            hintName: $"{lambdaGeneration.FullyQualifiedContainingTypeMetadataName}.g.cs",
            source: source);
    }

    /// <summary>
    ///     Only include type declarations that are classes, records or structs and are partials.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
        // Declared on a type with partial ancestors.
        node is TypeDeclarationSyntax typeDecl and (ClassDeclarationSyntax
            or RecordDeclarationSyntax
            or StructDeclarationSyntax) &&
        typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword) &&
        typeDecl.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .All(static p => p.Modifiers.Any(SyntaxKind.PartialKeyword)) ||
        // Declared on a static method with partial ancestors.
        node is MethodDeclarationSyntax methodDecl &&
        methodDecl.Modifiers.Any(SyntaxKind.StaticKeyword) &&
        methodDecl.Ancestors()
            .OfType<TypeDeclarationSyntax>()
            .All(static p => p.Modifiers.Any(SyntaxKind.PartialKeyword));
}
