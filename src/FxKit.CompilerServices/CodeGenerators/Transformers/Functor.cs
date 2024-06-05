using System.Diagnostics;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static FxKit.CompilerServices.CodeGenerators.Transformers.Helpers;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     An extraction from a referenced assembly.
/// </summary>
/// <param name="Candidates">The found functors.</param>
/// <param name="Locators">
///     Potential locators; they have not been verified yet as we need to know all functors first.
/// </param>
internal sealed record ReferencedFunctors(
    EquatableArray<FunctorCandidate> Candidates,
    EquatableArray<FunctorImplementationLocator> Locators)
{
    /// <summary>
    ///     The method name to look for that may implement functor map.
    /// </summary>
    private const string FunctorMapMethodName = "Map";

    /// <summary>
    ///     Finds functors and functor implementations in the given assembly reference.
    /// </summary>
    /// <remarks>
    ///     We're extracting both functors (typed with a `[Functor]` attribute), as well
    ///     as functor implementations (`Map` methods) in a single pass to avoid having
    ///     to iterate on references multiple times.
    /// </remarks>
    /// <param name="reference"></param>
    /// <param name="compilation"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static ReferencedFunctors? GetFunctorsAndBehaviorFromReference(
        MetadataReference reference,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol asm)
        {
            return null;
        }

        List<FunctorCandidate>? candidates = null;
        List<FunctorImplementationLocator>? locators = null;
        var typeSymbols = TypeSymbolHelper.GetTypesRecursive(asm.GlobalNamespace, cancellationToken);
        foreach (var typeSymbol in typeSymbols)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var attributeData in typeSymbol.GetAttributes())
            {
                if (attributeData.AttributeClass?.GetFullyQualifiedMetadataName() !=
                    TransformerGenerator.FunctorAttribute)
                {
                    continue;
                }

                candidates ??= [];
                candidates.Add(FunctorCandidate.From(typeSymbol));
            }

            var methodSymbols = typeSymbol.GetMembers(FunctorMapMethodName)
                .OfType<IMethodSymbol>()
                .Where(
                    static x => x is
                    {
                        IsStatic: true,
                        IsExtensionMethod: true,
                        IsGenericMethod: true
                    });

            foreach (var methodSymbol in methodSymbols)
            {
                if (methodSymbol.Parameters.Length == 0)
                {
                    continue;
                }

                var firstParamType = methodSymbol.Parameters[0].Type;
                var firstParamMetadataName = firstParamType.GetFullyQualifiedMetadataName();
                locators ??= [];
                locators.Add(
                    new FunctorImplementationLocator(
                        FunctorFullyQualifiedMetadataName: firstParamMetadataName,
                        Namespace: methodSymbol.ContainingNamespace.ToDisplayString()));
            }
        }

        // Don't return anything if we didn't find any functors or implementations
        // in this reference.
        if (candidates is null && locators is null)
        {
            return null;
        }

        return new ReferencedFunctors(
            Candidates: candidates?.ToEquatableArray() ?? EquatableArray<FunctorCandidate>.Empty,
            Locators: locators?.ToEquatableArray() ??
                      EquatableArray<FunctorImplementationLocator>.Empty);
    }
}

/// <summary>
///     Represents an outer container functor type that has not yet had its implementation located.
/// </summary>
internal sealed record FunctorCandidate
{
    /// <summary>
    ///     The name of the functor (its identifier).
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The fully qualified name, excluding the `global::`.
    /// </summary>
    public string FullyQualifiedMetadataName { get; }

    /// <summary>
    ///     The namespace the functor is contained within.
    /// </summary>
    public string ContainingNamespace { get; }

    /// <summary>
    ///     The functor's type parameters.
    /// </summary>
    public EquatableArray<string> TypeParameters { get; }

    /// <summary>
    ///     The container's constraint clauses.
    /// </summary>
    public EquatableArray<TypeParameterConstraints> ConstraintClauses { get; }

    private FunctorCandidate(
        string name,
        string fullyQualifiedMetadataName,
        string containingNamespace,
        EquatableArray<string> typeParameters,
        EquatableArray<TypeParameterConstraints> constraintClauses)
    {
        Name = name;
        FullyQualifiedMetadataName = fullyQualifiedMetadataName;
        ContainingNamespace = containingNamespace;
        TypeParameters = typeParameters;
        ConstraintClauses = constraintClauses;
    }

    /// <summary>
    ///     Creates a <see cref="FunctorCandidate" />.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static FunctorCandidate From(INamedTypeSymbol symbol)
    {
        return new FunctorCandidate(
            name: symbol.Name,
            fullyQualifiedMetadataName: symbol.GetFullyQualifiedMetadataName(),
            containingNamespace: symbol.ContainingNamespace.ToDisplayString(),
            typeParameters: ToTypeParameterNames(symbol.TypeParameters),
            constraintClauses: symbol.TypeParameters
                .Select(TypeParameterConstraints.FromTypeParameterSymbol)
                .ToEquatableArray());
    }

    /// <summary>
    ///     Creates a <see cref="Functor" /> from this <see cref="FunctorCandidate" />.
    /// </summary>
    /// <param name="functorImplementationNamespace"></param>
    /// <returns></returns>
    public Functor ToFunctor(string functorImplementationNamespace) =>
        new(
            Name,
            ContainingNamespace,
            TypeParameters,
            ConstraintClauses,
            functorImplementationNamespace);
}

/// <summary>
///     Contains information about a functor and the namespace that contains its behavior.
/// </summary>
/// <param name="FunctorFullyQualifiedMetadataName">The functor's fully qualified metadata name.</param>
/// <param name="Namespace">The namespace that provides the functor's `Map` implementation.</param>
internal sealed record FunctorImplementationLocator(
    string FunctorFullyQualifiedMetadataName,
    string Namespace);

/// <summary>
///     Represents an outer container functor type.
/// </summary>
[DebuggerDisplay("{ToString()}")]
internal sealed record Functor(
    string Name,
    string ContainingNamespace,
    EquatableArray<string> TypeParameters,
    EquatableArray<TypeParameterConstraints> ConstraintClauses,
    string FunctorImplementationNamespace)
{
    /// <summary>
    ///     The name of the functor (its identifier).
    /// </summary>
    public string Name { get; } = Name;

    /// <summary>
    ///     The functor's fully qualified name (without any type arguments).
    /// </summary>
    public string FullyQualifiedName => FullyQualifiedName(ContainingNamespace, Name);

    /// <summary>
    ///     The namespace the functor is contained within.
    /// </summary>
    public string ContainingNamespace { get; } = ContainingNamespace;

    /// <summary>
    ///     The namespace that contains this functor's implementation of `Map`.
    /// </summary>
    public string FunctorImplementationNamespace { get; } = FunctorImplementationNamespace;

    /// <summary>
    ///     The functor's type parameters.
    /// </summary>
    public EquatableArray<string> TypeParameters { get; } = TypeParameters;

    /// <summary>
    ///     The container's constraint clauses.
    /// </summary>
    public EquatableArray<TypeParameterConstraints> ConstraintClauses { get; } = ConstraintClauses;

    /// <summary>
    ///     Converts this <see cref="Functor" /> to a <see cref="GenericNameSyntax" />.
    /// </summary>
    /// <returns></returns>
    public GenericNameSyntax ToGenericNameSyntax() =>
        GenericName(
            Identifier(Name),
            TypeArgumentList(SeparatedList<TypeSyntax>(TypeParameters.Select(p => IdentifierName(p)))));

    /// <inheritdoc />
    public override string ToString() => FullyQualifiedName;
}
