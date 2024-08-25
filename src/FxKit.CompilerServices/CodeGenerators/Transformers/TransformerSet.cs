using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static FxKit.CompilerServices.CodeGenerators.Transformers.Helpers;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     A set of transformer methods to be put into a single generated class.
/// </summary>
/// <param name="HintName">Part of the name for the generated file.</param>
/// <param name="FunctorNamespace">The namespace the generated class will reside in.</param>
/// <param name="RequiredNamespaces">Namespaces to include.</param>
/// <param name="MethodsWithCollidingTypeParameters">
///     Methods containing colliding type parameters. Needed for error reporting.
/// </param>
/// <param name="TransformerMethods">Transformer methods.</param>
internal sealed record TransformerSet(
    string HintName,
    string GeneratedClassName,
    string FunctorNamespace,
    EquatableArray<string> RequiredNamespaces,
    EquatableArray<FunctorMethodDescriptor> MethodsWithCollidingTypeParameters,
    EquatableArray<FunctorTransformerMethodDescriptor> TransformerMethods)
{
    /// <summary>
    ///     Creates a set of transformer methods for the given inner functor and
    ///     all the outer functors
    /// </summary>
    /// <param name="allOuterContainers">All known outer functor types.</param>
    /// <param name="fullFunctorMetadataName">Full metadata name of the functor that appears "on the inside".</param>
    /// <param name="functorName">The name of the functor that appears "on the inside".</param>
    /// <param name="functorNamespace">The namespace of the inner functor.</param>
    /// <param name="methods">The methods associated with the inner functor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A set of transformer methods.</returns>
    public static TransformerSet Create(
        IReadOnlyList<Functor> allOuterContainers,
        string fullFunctorMetadataName,
        string functorName,
        string functorNamespace,
        IReadOnlyList<FunctorMethodDescriptor> methods,
        CancellationToken cancellationToken)
    {
        var namespaceList = new HashSet<string>();
        var methodsWithCollidingTypeParams = new List<FunctorMethodDescriptor>();
        var transformerMethods = new List<FunctorTransformerMethodDescriptor>(capacity: methods.Count);

        // We don't want to nest one functor within the same functor.
        var methodGroupFunctorFullyQualifiedName = FullyQualifiedName(
            functorNamespace,
            functorName);
        var outerContainers = allOuterContainers.Where(
            o => o.FullyQualifiedName != methodGroupFunctorFullyQualifiedName);

        foreach (var outer in outerContainers)
        {
            namespaceList.Add(outer.ContainingNamespace);
            namespaceList.Add(outer.FunctorImplementationNamespace);

            foreach (var method in methods)
            {
                cancellationToken.ThrowIfCancellationRequested();
                namespaceList.AddRange(method.RequiredNamespaces);

                // Skip "SelectMany" for now since it requires a different implementation.
                // We can come back to this later.
                if (method.Name == LinqMonadicBind)
                {
                    continue;
                }

                // Special provisions for `Traverse`/`Sequence`.
                if (method.Name is Traverse or Sequence)
                {
                    // Skip over `Traverse`/`Sequence` methods that would nest two of the same functors.
                    if (method.ReturnType is ConcreteOrConstructedType.Constructed f &&
                        f.ConstructedType.FullyQualifiedName == outer.FullyQualifiedName)
                    {
                        continue;
                    }

                    // Generate additional overloads of `Traverse`/`Sequence` that
                    // accept `IReadOnlyList`, since C# variance sucks.
                    // For example, doing a `SequenceT` on a `Task<IReadOnlyList<Validation<..>>>` would
                    // not be allowed because `SequenceT`'s input is `Task<IEnumerable<Validation<..>>>`,
                    // and C# isn't smart enough to see that yes, an `IReadOnlyList` can in fact be
                    // a `Task<IEnumerable>`...
                    if (method.Functor.FullyQualifiedName == IEnumerableFullyQualifiedName)
                    {
                        // Skip generating `IReadOnlyList<IReadOnlyList<..>>`.
                        if (outer.FullyQualifiedName == IReadOnlyListFullyQualifiedName)
                        {
                            continue;
                        }

                        // Used to synthesize an `IReadOnlyList` overload for methods where the functor (source) is
                        // `IEnumerable`. This is because C# does not allow a `Task<IReadOnlyList>` where
                        // a `Task<IEnumerable>` is expected.
                        var synthesized = method.ReplaceFunctor(
                            method.Functor.ReplaceReference(
                                name: "IReadOnlyList",
                                fullyQualifiedMetadataName: "System.Collections.Generic.IReadOnlyList`1",
                                containingNamespace: "System.Collections.Generic"));
                        transformerMethods.Add(
                            FunctorTransformerMethodDescriptor.From(
                                transformerMethodName: ComputeTransformerName(synthesized.Name),
                                outer: outer,
                                originalMethod: synthesized,
                                typeParameterCollisionDetected: out _));
                    }
                }

                var transformerMethod = FunctorTransformerMethodDescriptor.From(
                    transformerMethodName: ComputeTransformerName(method.Name),
                    outer: outer,
                    originalMethod: method,
                    typeParameterCollisionDetected: out var typeParameterCollisionDetected);
                transformerMethods.Add(transformerMethod);

                if (typeParameterCollisionDetected)
                {
                    methodsWithCollidingTypeParams.Add(method);
                }

                // For LINQ's Where, we want to generate a `WhereT` transformer to be
                // used with method chaining, and a `Where` transformer with the same signature
                // as `WhereT` to be used by the LINQ Query Syntax.
                if (method.Name == LinqFilterName)
                {
                    transformerMethods.Add(
                        FunctorTransformerMethodDescriptor.From(
                            transformerMethodName: method.Name,
                            outer: outer,
                            originalMethod: method,
                            typeParameterCollisionDetected: out _));
                }
            }
        }

        namespaceList.Remove(functorNamespace);

        return new TransformerSet(
            HintName: fullFunctorMetadataName,
            GeneratedClassName: $"{functorName}T",
            FunctorNamespace: functorNamespace,
            RequiredNamespaces: namespaceList.OrderBy(static x => x).ToEquatableArray(),
            MethodsWithCollidingTypeParameters: methodsWithCollidingTypeParams.ToEquatableArray(),
            TransformerMethods: transformerMethods.ToEquatableArray());
    }

    /// <summary>
    ///     Computes the name for the transformer method.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static string ComputeTransformerName(string name) => name switch
    {
        "Select" => "Select",
        _        => $"{name}T"
    };
}

/// <summary>
///     Descriptor for a transformer method.
/// </summary>
/// <param name="TransformerMethodName">The transformer method's name.</param>
/// <param name="InnerMethodName">The name of the method being called by the transformer method.</param>
/// <param name="OuterFunctorName">The name of the outer functor for this transformer method.</param>
/// <param name="InnerFunctorName">The name of the inner functor for this transformer method.</param>
/// <param name="TypeParameterNames">Type parameters needed.</param>
/// <param name="TypeParameterConstraints">Constraints for type parameters.</param>
/// <param name="StackedSource">The stacked source (<c>this</c>) parameter.</param>
/// <param name="Parameters">Parameters for the method (not including `this`).</param>
/// <param name="ReturnType">The transformer method's return type.</param>
/// <param name="RequiresFlattening">
///     Whether the implementation of the transformer needs to
///     flatten the result.
/// </param>
internal sealed record FunctorTransformerMethodDescriptor(
    string TransformerMethodName,
    string InnerMethodName,
    string OuterFunctorName,
    string InnerFunctorName,
    EquatableArray<string> TypeParameterNames,
    EquatableArray<TypeParameterConstraints> TypeParameterConstraints,
    ConstructedFunctor StackedSource,
    EquatableArray<FunctorMethodParameter> Parameters,
    ConstructedFunctor ReturnType,
    bool RequiresFlattening)
{
    /// <summary>
    ///     Creates a transformer method descriptor from the given
    ///     outer functor and original method.
    /// </summary>
    /// <param name="transformerMethodName">The name of the returned transformer method.</param>
    /// <param name="outer">The outer functor.</param>
    /// <param name="originalMethod">The original method having a transformer generated for it.</param>
    /// <param name="typeParameterCollisionDetected">
    ///     Will be set to <c>true</c> if a type parameter collision was detected.
    /// </param>
    /// <returns>The transformer method descriptor.</returns>
    public static FunctorTransformerMethodDescriptor From(
        string transformerMethodName,
        Functor outer,
        FunctorMethodDescriptor originalMethod,
        out bool typeParameterCollisionDetected)
    {
        // Create a stacked source type.
        // For example, if the outer functor is `Task<T>`, and
        // the original method's functor (source parameter) is `Option<T>`,
        // then our stacked source becomes `Task<Option<T>>`.
        var stackedSource = ConstructedFunctor.From(
            outer: outer,
            inner: ConcreteOrConstructedType.Constructed.Of(originalMethod.Functor));

        // Similarly, create the return type.
        var rawReturnType = ConstructedFunctor.From(
            outer: outer,
            inner: originalMethod.ReturnType);

        // Check if we need to flatten the return type.
        // This would be the case when the return type is `Task<Task<Result<TOk, TErr>>>`, but what we
        // want is `Task<Result<TOk, TErr>>`.
        var returnType = MaybeFlattenReturnType(rawReturnType, out var requiresFlattening);

        // Compute the type parameters needed for the transformer method.
        var typeParameters = ComputeTypeParameters(
            outer: outer,
            method: originalMethod,
            typeParameterCollisionDetected: out typeParameterCollisionDetected);
        var typeParameterConstraints = ComputeTypeParameterConstraints(
            outer: outer,
            method: originalMethod);

        return new FunctorTransformerMethodDescriptor(
            TransformerMethodName: transformerMethodName,
            InnerMethodName: originalMethod.Name,
            OuterFunctorName: outer.Name,
            InnerFunctorName: originalMethod.Functor.Name,
            TypeParameterNames: typeParameters,
            TypeParameterConstraints: typeParameterConstraints,
            StackedSource: stackedSource,
            Parameters: originalMethod.Parameters,
            ReturnType: returnType,
            RequiresFlattening: requiresFlattening);
    }

    /// <summary>
    ///     Computes the required type parameters for the transformer given the method in question
    ///     and the functor type.
    /// </summary>
    /// <param name="outer">The outer functor type for the generated transformer.</param>
    /// <param name="method">The method being wrapped in a transformer.</param>
    /// <param name="typeParameterCollisionDetected">
    ///     Will be set according to whether a type parameter name collision was detected.
    /// </param>
    /// <returns>The type parameters in the order they should appear.</returns>
    private static EquatableArray<string> ComputeTypeParameters(
        Functor outer,
        FunctorMethodDescriptor method,
        out bool typeParameterCollisionDetected)
    {
        // Consider a method of the form:
        //
        //  Result<TNewOk, TErr> Map<TOk, TErr, TNewOk>(...)
        //
        // If we're creating a Validation of Result transformer and use the
        // original method's type parameters as well as Validation's type
        // parameters as the transformer's type parameters, we'll end up
        // with an unused parameter:
        //
        //  Validation<Result<TNewOk, TErr>, TInvalid> MapT<TOk, TErr, TNewOk, TValid, TInvalid>(...).
        //
        // In this case, it was `TValid`. It's not used because `Result` gets used in its place.
        // For that reason, we want to remove the type parameter in the spot where we're
        // placing the composed type.
        //
        // Additionally, we'll need to detect whether the composition of the functors
        // will result in a type parameter collision.
        var typeParameters = method.TypeParameters
            .Concat(outer.TypeParameters.Skip(1))
            .ToEquatableArray();

        typeParameterCollisionDetected = typeParameters.Distinct().Count() != typeParameters.Length;
        return typeParameters;
    }

    /// <summary>
    ///     Computes the constraints for the transformer method's type parameters.
    /// </summary>
    /// <param name="outer">The outer functor type for the generated transformer.</param>
    /// <param name="method">The method being wrapped in a transformer.</param>
    /// <returns>The combined constraints.</returns>
    private static EquatableArray<TypeParameterConstraints> ComputeTypeParameterConstraints(
        Functor outer,
        FunctorMethodDescriptor method)
    {
        // When composing `Result<TOk, TErr>` within `Validation<TValid, TInvalid>`, the
        // `TValid` parameter will no longer be needed (because `Result<TOk, TErr>` is `TValid`).
        // Therefore, we don't want to copy its generic constraint over to the new method either.
        // See `ComputeTypeParameters` for a more elaborate explanation of this.
        var parameterApplied = outer.TypeParameters[0];

        var clausesFromFunctorDefinition = outer.ConstraintClauses
            .Where(c => c.Name != parameterApplied);

        return method.ConstraintClauses.Concat(clausesFromFunctorDefinition).ToEquatableArray();
    }

    /// <summary>
    ///     Flattens the given return type if needed.
    /// </summary>
    /// <param name="returnType">The return type to flatten, if needed.</param>
    /// <param name="requiresFlattening">Will be set to <c>true</c> if the return type was flattened.</param>
    /// <returns></returns>
    private static ConstructedFunctor MaybeFlattenReturnType(
        ConstructedFunctor returnType,
        out bool requiresFlattening)
    {
        // Bail if the outer functor isn't `Task`.
        if (returnType.FunctorName is not CSharpTaskMonad)
        {
            requiresFlattening = false;
            return returnType;
        }

        // Bail if the first type argument to the return type isn't `Task`
        if (returnType.TypeArguments[0] is not
            ConcreteOrConstructedType.Constructed({ Name: CSharpTaskMonad } firstTypeArgument))
        {
            requiresFlattening = false;
            return returnType;
        }

        requiresFlattening = true;
        return new ConstructedFunctor(
            FunctorName: CSharpTaskMonad,
            TypeArguments: new []
            {
                firstTypeArgument.TypeArguments[0]
            }.ToEquatableArray());
    }
}

/// <summary>
///     A functor that has been constructed with type arguments.
/// </summary>
/// <param name="FunctorName"></param>
/// <param name="TypeArguments"></param>
internal sealed record ConstructedFunctor(
    string FunctorName,
    EquatableArray<ConcreteOrConstructedType> TypeArguments)
{
    /// <summary>
    ///     Creates a <see cref="GenericNameSyntax"/> from this constructed functor.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public GenericNameSyntax ToGenericNameSyntax()
    {
        var args = SeparatedList(
            TypeArguments.Select(
                static t => t switch
                {
                    ConcreteOrConstructedType.Concrete(var type) =>
                        type.ToTypeSyntax(),
                    ConcreteOrConstructedType.Constructed(var type) =>
                        type.ToTypeSyntax(),
                    _ => throw new ArgumentOutOfRangeException(nameof(t))
                }));

        return GenericName(FunctorName).WithTypeArgumentList(TypeArgumentList(args));
    }

    /// <summary>
    ///     Creates a <see cref="ConstructedFunctor"/> by constructing a concrete functor type
    ///     using the given <paramref name="inner"/> type.
    ///
    ///     For example, given an outer functor <c>Result&lt;T, E&gt;</c> and inner type <c>Option&lt;T&gt;</c>,
    ///     returns <c>Result&lt;Option&lt;T&gt;, E&gt;</c>.
    /// </summary>
    public static ConstructedFunctor From(Functor outer, ConcreteOrConstructedType inner)
    {
        var typeArgs = outer.TypeParameters.Skip(1)
            // We know we can use a concrete type for the type argument because they
            // cannot be generic themselves (otherwise, we'd have HKTs, and we wouldn't need
            // this generator at all).
            .Select(static t => ConcreteOrConstructedType.Concrete.Of(new ConcreteType(Type: t)))
            .Prepend(inner)
            .ToEquatableArray();

        return new ConstructedFunctor(FunctorName: outer.Name, TypeArguments: typeArgs);
    }
}
