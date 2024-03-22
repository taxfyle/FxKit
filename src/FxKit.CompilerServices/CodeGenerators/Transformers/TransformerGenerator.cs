using System.Collections.Immutable;
using FxKit.CompilerServices.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FxKit.CompilerServices.CodeGenerators.Transformers.Helpers;
using static FxKit.CompilerServices.CodeGenerators.Transformers.TransformerClassBuilder;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

[Generator]
public class TransformerGenerator : IIncrementalGenerator
{
    /// <summary>
    ///     Fully qualified name for <see cref="GenerateTransformerAttribute" />.
    /// </summary>
    private const string GenerateTransformerAttribute =
        "FxKit.CompilerServices.GenerateTransformerAttribute";

    /// <summary>
    ///     Fully qualified name for <see cref="FunctorAttribute" />.
    /// </summary>
    private const string FunctorAttribute = "FxKit.CompilerServices.FunctorAttribute";

    /// <summary>
    ///     These third-party containers will wrap others.
    /// </summary>
    private static readonly IReadOnlyList<string> thirdPartyOuterContainerNames =
    [
        "System.Threading.Tasks.Task`1",
        "System.Collections.Generic.IEnumerable`1",
        "System.Collections.Generic.IReadOnlyList`1"
    ];

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter for marked methods.
        var methodDeclarations =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: GenerateTransformerAttribute,
                predicate: static (s, _) => IsMethodSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => (MethodDeclarationSyntax)ctx.TargetNode);

        //  Filter for marked type declarations (functors).
        var functorDeclarations =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: FunctorAttribute,
                predicate: static (s, _) => IsTypeDeclarationSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.TargetNode);

        // Combine the selected methods and functors with the Compilation.
        var compilationAndMethods = context.CompilationProvider
            .Combine(methodDeclarations.Collect())
            .Combine(functorDeclarations.Collect());

        // Generate the source using the compilation and methods.
        context.RegisterSourceOutput(
            compilationAndMethods,
            static (spc, source) => Execute(source.Left.Left, source.Left.Right, source.Right, spc));
    }

    /// <summary>
    ///     Filter based on the node type. Only methods are chosen that have more than at least one
    ///     attribute and are extension methods.
    /// </summary>
    /// <param name="syntaxNode"></param>
    /// <returns></returns>
    private static bool IsMethodSyntaxTargetForGeneration(SyntaxNode syntaxNode) =>
        syntaxNode is MethodDeclarationSyntax { AttributeLists.Count: > 0 } m &&
        m.IsExtensionMethod();

    /// <summary>
    ///     Filter based on the node type. Only type declarations are chosen that have at least one
    ///     attribute.
    /// </summary>
    /// <param name="syntaxNode"></param>
    /// <returns></returns>
    private static bool IsTypeDeclarationSyntaxTargetForGeneration(SyntaxNode syntaxNode) =>
        syntaxNode is TypeDeclarationSyntax { AttributeLists.Count: > 0 };

    /// <summary>
    ///     Generates the code.
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="methodDeclarations"></param>
    /// <param name="typeDeclarations"></param>
    /// <param name="ctx"></param>
    private static void Execute(
        Compilation compilation,
        ImmutableArray<MethodDeclarationSyntax> methodDeclarations,
        ImmutableArray<TypeDeclarationSyntax> typeDeclarations,
        SourceProductionContext ctx)
    {
        if (methodDeclarations.IsEmpty || typeDeclarations.IsEmpty)
        {
            return;
        }

        // Third-party functor symbols.
        var thirdPartyOuterFunctorSymbols =
            thirdPartyOuterContainerNames
                .SelectNotNull(compilation.GetTypeByMetadataName)
                .ToImmutableList();

        // Names of all outer functors.
        var functorNames =
            typeDeclarations
                .Select(static t => t.Identifier.ToString())
                .Concat(thirdPartyOuterFunctorSymbols.Select(s => s.Name))
                .ToImmutableList();

        var functorNamespaces = GetNotNullFunctorBehaviorNamespaces(compilation, functorNames)
            .ToImmutableHashSet();

        // There's nothing we can do if the list of namespaces is empty.
        if (functorNamespaces.IsEmpty)
        {
            return;
        }

        // It'll cause problems when we try to get the namespace if the count is different.
        if (functorNamespaces.Count != functorNames.Count)
        {
            return;
        }

        // Third party container instances.
        var thirdPartyOuterFunctors =
            thirdPartyOuterFunctorSymbols
                .Select(s => Functor.From(s, GetNamespaceForFunctor(functorNamespaces, s.Name)));

        // First-party and third-party containers.
        var allOuterFunctors =
            typeDeclarations
                .SelectNotNull(t => compilation.GetSemanticModel(t.SyntaxTree).GetDeclaredSymbol(t))
                .Select(s => Functor.From(s, GetNamespaceForFunctor(functorNamespaces, s.Name)))
                .Concat(thirdPartyOuterFunctors)
                .ToImmutableList();

        // Methods grouped by their functor.
        var methodsByFunctor = methodDeclarations
            .SelectNotNull(static m => m)
            .Select(
                m =>
                {
                    var symbol = compilation
                        .GetSemanticModel(m.SyntaxTree)
                        .GetDeclaredSymbol(m)!;

                    return FunctorMethodDescriptor.From(m, symbol);
                })
            .GroupBy(static m => (Functor: m.Functor.Name, Identifier: m.Functor.ContainingNamespace))
            .ToImmutableList();

        foreach (var methodGroup in methodsByFunctor)
        {
            var transformerSet = CreateTransformerFile(
                methodGroup,
                allOuterFunctors,
                out var collisions);

            if (collisions.Count > 0)
            {
                collisions.ForEach(
                    m =>
                        ctx.ReportDiagnostic(
                            Diagnostic.Create(
                                DiagnosticsDescriptors
                                    .MethodDeclarationCannotAllowCollidingTypeParameters,
                                m.Location)));
                return;
            }

            ctx.AddSource(
                transformerSet.Name + ".Generated.cs",
                transformerSet.Unit.NormalizeWhitespace().ToFullString());
        }
    }

    /// <summary>
    ///     Gets functor behavior namespaces.
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="functorNames"></param>
    /// <returns></returns>
    private static IEnumerable<(string Functor, string Namespace)> GetNotNullFunctorBehaviorNamespaces(
        Compilation compilation,
        ImmutableList<string> functorNames)
    {
        // Namespaces that provide functor functionality to outer functors.
        var rawFunctorNamespaces = GetFunctorBehaviorNamespaces(
            compilation,
            functorNames.ToImmutableHashSet());

        foreach (var rawFunctorNamespace in rawFunctorNamespaces)
        {
            if (rawFunctorNamespace.HasValue)
            {
                yield return rawFunctorNamespace.Value;
            }
        }
    }
}
