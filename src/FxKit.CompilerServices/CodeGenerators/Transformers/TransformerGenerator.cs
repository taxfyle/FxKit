using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices.Extensions;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FxKit.CompilerServices.CodeGenerators.Transformers.Helpers;
using static FxKit.CompilerServices.CodeGenerators.Transformers.TransformerClassBuilder;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable LoopCanBeConvertedToQuery

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

[Generator]
public class TransformerGenerator : IIncrementalGenerator
{
    /// <summary>
    ///     Fully qualified name for <see cref="GenerateTransformerAttribute" />.
    /// </summary>
    public const string GenerateTransformerAttribute =
        "FxKit.CompilerServices.GenerateTransformerAttribute";

    /// <summary>
    ///     Fully qualified name for <see cref="ContainsFunctorsAttribute" />.
    /// </summary>
    public const string ContainsFunctorsAttribute =
        "FxKit.CompilerServices.ContainsFunctorsAttribute";

    /// <summary>
    ///     Fully qualified name for <see cref="FunctorAttribute" />.
    /// </summary>
    public const string FunctorAttribute = "FxKit.CompilerServices.FunctorAttribute";

    /// <summary>
    ///     These intrinsic functors will wrap others.
    /// </summary>
    private static readonly IReadOnlyList<string>
        intrinsicFunctorMetadataNames =
        [
            "System.Threading.Tasks.Task`1",
            "System.Collections.Generic.IEnumerable`1",
            "System.Collections.Generic.IReadOnlyList`1"
        ];

    /// <summary>
    ///     A cached array of intrinsic functor candidates.
    ///     This avoids having to extract them for each run, as they will never change.
    /// </summary>
    private static FunctorCandidate[]? cachedIntrinsicFunctorCandidates;

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter for marked methods and transform them to functor method descriptors.
        var methodsByFunctor = context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: GenerateTransformerAttribute,
                predicate: static (s, _) => IsMethodSyntaxTargetForGeneration(s),
                transform: static (ctx, _) =>
                {
                    // SAFETY: `IsMethodSyntaxTargetForGeneration` already asserted on the fact that
                    // it's a method; therefore the symbol must also be a method.
                    Debug.Assert(
                        ctx.TargetNode is MemberDeclarationSyntax,
                        "ctx.TargetNode is MemberDeclarationSyntax");
                    var m = Unsafe.As<MethodDeclarationSyntax>(ctx.TargetNode);
                    var symbol = Unsafe.As<IMethodSymbol>(ctx.TargetSymbol);
                    return FunctorMethodDescriptor.From(m, symbol);
                })
            .Collect()
            .WithTrackingName("MethodsByFunctor");

        // Get symbols for intrinsic outer functors (IEnumerable, IReadOnlyList, etc).
        var functorCandidatesFromIntrinsicFunctors = context.CompilationProvider
            .Select(
                static (compilation, _) =>
                {
                    // NOTE: These never change between runs, as they are based on a static
                    // list of known types
                    if (cachedIntrinsicFunctorCandidates != null)
                    {
                        return new EquatableArray<FunctorCandidate>(cachedIntrinsicFunctorCandidates);
                    }

                    var result =
                        new List<FunctorCandidate>(capacity: intrinsicFunctorMetadataNames.Count);
                    foreach (var metadataName in intrinsicFunctorMetadataNames)
                    {
                        var symbol = compilation.GetTypeByMetadataName(metadataName);
                        if (symbol is null)
                        {
                            continue;
                        }

                        result.Add(FunctorCandidate.From(symbol));
                    }

                    var array = result.ToArray();
                    cachedIntrinsicFunctorCandidates = array;
                    return new EquatableArray<FunctorCandidate>(array);
                })
            .WithTrackingName("FunctorCandidatesFromIntrinsicFunctors");

        // Find marked type declarations (functors).
        var functorCandidatesFromSyntax =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                    fullyQualifiedMetadataName: FunctorAttribute,
                    predicate: static (s, _) => IsTypeDeclarationSyntaxTargetForGeneration(s),
                    transform: static (ctx, _) =>
                    {
                        Debug.Assert(
                            ctx.TargetSymbol is INamedTypeSymbol,
                            "ctx.TargetSymbol is INamedTypeSymbol");
                        var symbol = Unsafe.As<INamedTypeSymbol>(ctx.TargetSymbol);
                        return FunctorCandidate.From(symbol);
                    })
                .Collect()
                .WithTrackingName("FunctorCandidatesFromSyntax");

        // Find functor candidates and implementations in referenced assemblies.
        var referencedFunctors = context.MetadataReferencesProvider
            .Where(ContainsFunctors)
            .Combine(context.CompilationProvider)
            .Select(
                static (tuple, ct) =>
                {
                    var (reference, compilation) = tuple;
                    // SAFETY: nulls are filtered out in the subsequent step.
                    return ReferencedFunctors.GetFunctorsAndBehaviorFromReference(
                        reference: reference,
                        compilation: compilation,
                        cancellationToken: ct)!;
                })
            .Where(static x => x is not null)
            .Collect()
            .WithTrackingName("ReferencedFunctors");

        // Extract functor candidates found in referenced assemblies.
        var functorCandidatesFromReferences = referencedFunctors
            .SelectMany(static (r, _) => r.SelectMany(static x => x.Candidates))
            .Collect()
            .WithTrackingName("FunctorCandidatesFromReferences");

        // All known functor names.
        var allFunctorFullyQualifiedMetadataNames = functorCandidatesFromIntrinsicFunctors
            .Combine(functorCandidatesFromSyntax)
            .Combine(functorCandidatesFromReferences)
            .SelectMany(
                static (tuple, _) =>
                {
                    var ((intrinsic, syntax), refs) = tuple;
                    return intrinsic
                        .Concat(syntax)
                        .Concat(refs)
                        .Select(static c => c.FullyQualifiedMetadataName);
                })
            .Collect()
            .Select(static (x, _) => x.ToImmutableHashSet())
            .WithComparer(ImmutableHashSetEqualityComparer<string>.Default);

        // Functor `Map` implementations found in syntax.
        var functorImplementationLocatorsFromSyntax =
            context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (node, _) => IsFunctorMapMethodSyntaxCandidate(node),
                    transform: static (ctx, ct) =>
                    {
                        // SAFETY: `IsFunctorMapMethodSyntaxCandidate` already asserted on the fact that
                        // it's a method; therefore the symbol must also be a method.
                        Debug.Assert(
                            ctx.Node is MemberDeclarationSyntax,
                            "ctx.TargetNode is MemberDeclarationSyntax");
                        var methodSyntax = Unsafe.As<MethodDeclarationSyntax>(ctx.Node);
                        var symbol = Unsafe.As<IMethodSymbol>(
                            ctx.SemanticModel.GetDeclaredSymbol(methodSyntax, ct));
                        if (symbol is null)
                        {
                            // SAFETY: Filtered out in next step.
                            return null!;
                        }

                        return new FunctorImplementationLocator(
                            FunctorFullyQualifiedMetadataName: symbol.Parameters.First()
                                .Type.GetFullyQualifiedMetadataName(),
                            Namespace: symbol.ContainingNamespace.ToString());
                    })
                .Where(static x => x is not null)
                .Collect()
                .WithTrackingName("FunctorImplementationLocatorsFromSyntax");

        // Functor `Map` implementations found in referenced assemblies.
        var functorImplementationLocatorsFromReferences = referencedFunctors
            .Combine(allFunctorFullyQualifiedMetadataNames)
            .SelectMany(
                static (tuple, ct) =>
                {
                    var (referencedFunctors, allFunctorFullyQualifiedMetadataNames) = tuple;
                    return FilterLocatorsBasedOnKnownFunctors(
                        referencedFunctors,
                        allFunctorFullyQualifiedMetadataNames,
                        ct);
                })
            .Collect();

        // Combine all functor implementation locators.
        var functorImplementationLocators = functorImplementationLocatorsFromReferences
            .Combine(functorImplementationLocatorsFromSyntax)
            .SelectMany(static (tuple, _) => tuple.Left.Concat(tuple.Right))
            .Collect()
            .WithTrackingName("FunctorImplementationLocators");

        // Match all the functors to generate transformers for with their `Map` implementation.
        var allOuterFunctors = functorCandidatesFromSyntax
            .Combine(functorCandidatesFromIntrinsicFunctors)
            .Combine(functorCandidatesFromReferences)
            .Combine(functorImplementationLocators)
            .SelectMany(
                static (input, _) =>
                {
                    var (((syntax, intrinsic), refs), functorImplementationLocators) = input;
                    var candidates = syntax.Concat(intrinsic).Concat(refs);
                    var result = new List<Functor>(
                        capacity: syntax.Length + intrinsic.Length + refs.Length);
                    foreach (var functorCandidate in candidates)
                    {
                        var namespaceForFunctorImplementation =
                            functorImplementationLocators
                                .FirstOrDefault(
                                    t => t.FunctorFullyQualifiedMetadataName ==
                                         functorCandidate.FullyQualifiedMetadataName)
                                ?.Namespace;
                        if (namespaceForFunctorImplementation is null)
                        {
                            continue;
                        }

                        result.Add(functorCandidate.ToFunctor(namespaceForFunctorImplementation));
                    }

                    return result;
                })
            .Collect();

        // Generate the transformers.
        context.RegisterSourceOutput(
            source: allOuterFunctors.Combine(methodsByFunctor),
            action: static (spc, source) => Execute(source.Left, source.Right, spc));

        // If there are any functors defined in syntax, then we want to
        // add the `ContainsFunctors` attribute.
        context.RegisterSourceOutput(
            source: functorCandidatesFromSyntax.Select(static (list, _) => list.Length > 0),
            action: static (spc, containsFunctors) =>
            {
                if (containsFunctors)
                {
                    spc.AddSource(
                        hintName: "ContainsFunctors.g.cs",
                        "[assembly: FxKit.CompilerServices.ContainsFunctors]");
                }
            });
    }

    /// <summary>
    ///     Filter based on the node type. Only methods are chosen that have at least one
    ///     attribute, is an extension method, and has a first parameter with a declared type
    ///     being generic.
    /// </summary>
    /// <param name="syntaxNode"></param>
    /// <returns></returns>
    private static bool IsMethodSyntaxTargetForGeneration(SyntaxNode syntaxNode) =>
        syntaxNode is MethodDeclarationSyntax
        {
            AttributeLists.Count: > 0,
            ParameterList.Parameters.Count: > 0
        } m &&
        m.IsExtensionMethod() &&
        m.ParameterList.Parameters[0].Type is GenericNameSyntax;

    /// <summary>
    ///     Filter based on the node type. Only type declarations are chosen that have at least one
    ///     attribute.
    /// </summary>
    /// <param name="syntaxNode"></param>
    /// <returns></returns>
    private static bool IsTypeDeclarationSyntaxTargetForGeneration(SyntaxNode syntaxNode) =>
        syntaxNode is TypeDeclarationSyntax { AttributeLists.Count: > 0 };

    /// <summary>
    ///     Indicates whether the given syntax node *could* be a functor `Map` method declaration
    ///     that operators on a functor.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static bool IsFunctorMapMethodSyntaxCandidate(SyntaxNode node) =>
        node is MethodDeclarationSyntax m &&
        m.Identifier.ToString() == "Map" &&
        m.IsExtensionMethod() &&
        m.ParameterList.Parameters.Count > 0 &&
        m.ParameterList.Parameters[0].Type is GenericNameSyntax;

    /// <summary>
    ///     Generates the code.
    /// </summary>
    /// <param name="functorMethods"></param>
    /// <param name="ctx"></param>
    /// <param name="allOuterFunctors"></param>
    private static void Execute(
        ImmutableArray<Functor> allOuterFunctors,
        ImmutableArray<FunctorMethodDescriptor> functorMethods,
        SourceProductionContext ctx)
    {
        // Methods grouped by their functor.
        var methodsByFunctor = functorMethods
            .GroupBy(static m => (Functor: m.Functor.Name, Namespace: m.Functor.ContainingNamespace))
            .ToImmutableList();

        foreach (var methodGroup in methodsByFunctor)
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();
            var transformerSet = CreateTransformerFile(
                methodGroup,
                allOuterFunctors,
                out var collisions);

            if (collisions.Count > 0)
            {
                foreach (var m in collisions)
                {
                    var descriptor = DiagnosticsDescriptors
                        .MethodDeclarationCannotAllowCollidingTypeParameters;
                    var diagnostic = Diagnostic.Create(
                        descriptor: descriptor,
                        location: m.Location?.ToLocation());
                    ctx.ReportDiagnostic(diagnostic);
                }

                return;
            }

            ctx.AddSource(
                transformerSet.Name + ".Generated.cs",
                transformerSet.Unit.NormalizeWhitespace().ToFullString());
        }
    }
}
