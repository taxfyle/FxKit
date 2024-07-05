using System.Runtime.CompilerServices;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Represents a descriptor for a method that operates on a functor.
/// </summary>
internal sealed record FunctorMethodDescriptor
{
    /// <summary>
    ///     The parent functor reference this method operates upon.
    /// </summary>
    public ConstructedType Functor { get; }

    /// <summary>
    ///     The return type of this method.
    /// </summary>
    public ConcreteOrConstructedType ReturnType { get; }

    /// <summary>
    ///     The name of this method.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The Type Parameters that this method takes (name only).
    /// </summary>
    public EquatableArray<string> TypeParameters { get; }

    /// <summary>
    ///     The constraint clauses present on this method.
    /// </summary>
    public EquatableArray<TypeParameterConstraints> ConstraintClauses { get; }

    /// <summary>
    ///     The namespaces that the method uses.
    /// </summary>
    public EquatableArray<string> RequiredNamespaces { get; }

    /// <summary>
    ///     Location of the method in source code, if we have it.
    /// </summary>
    public LocationInfo? Location { get; }

    /// <summary>
    ///     The parameters this method takes, as strings that need to be parsed.
    /// </summary>
    /// <remarks>
    ///     Does not include the "source" "this" argument from the extension method.
    /// </remarks>
    public EquatableArray<FunctorMethodParameter> Parameters { get; }

    private FunctorMethodDescriptor(
        ConstructedType functor,
        ConcreteOrConstructedType returnType,
        string name,
        EquatableArray<string> typeParameters,
        EquatableArray<FunctorMethodParameter> parameters,
        EquatableArray<TypeParameterConstraints> constraintClauses,
        EquatableArray<string> requiredNamespaces,
        LocationInfo? location)
    {
        Functor = functor;
        ReturnType = returnType;
        Name = name;
        TypeParameters = typeParameters;
        Parameters = parameters;
        ConstraintClauses = constraintClauses;
        RequiredNamespaces = requiredNamespaces;
        Location = location;
    }

    /// <summary>
    ///     Creates a copy of this functor method descriptor but uses the specified
    ///     constructed type in place of the current one (which references the functor).
    /// </summary>
    /// <param name="constructedType"></param>
    /// <returns></returns>
    public FunctorMethodDescriptor ReplaceFunctor(ConstructedType constructedType)
    {
        return new FunctorMethodDescriptor(
            functor: constructedType,
            returnType: ReturnType,
            name: Name,
            typeParameters: TypeParameters,
            parameters: Parameters,
            constraintClauses: ConstraintClauses,
            requiredNamespaces: RequiredNamespaces
                .Append(constructedType.ContainingNamespace)
                .ToEquatableArray(),
            location: Location);
    }

    /// <summary>
    ///     Creates a <see cref="FunctorMethodDescriptor" /> from
    ///     a <see cref="MethodDeclarationSyntax" /> node and its symbol.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="symbol"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FunctorMethodDescriptor From(MethodDeclarationSyntax method, IMethodSymbol symbol)
    {
        if (method.TypeParameterList is null || !method.TypeParameterList.Parameters.Any())
        {
            throw new InvalidOperationException(
                $"Can't create a {nameof(FunctorMethodDescriptor)} from a method with no parameters.");
        }

        var thisParamContainingNamespace =
            symbol.Parameters[0].Type.ContainingNamespace.ToDisplayString();

        // The first parameter to a method is always the functor itself.
        var containerReference = ConstructedType.From(
            value: Unsafe.As<GenericNameSyntax>(method.ParameterList.Parameters.First().Type!),
            containingNamespace: thisParamContainingNamespace);

        var returnType = method.ReturnType switch
        {
            GenericNameSyntax gns => ConcreteOrConstructedType.Constructed.Of(
                constructedType: ConstructedType.From(
                    value: gns,
                    containingNamespace: symbol.ReturnType.ContainingNamespace.ToDisplayString())),
            _ => ConcreteOrConstructedType.Concrete.Of(type: ConcreteType.From(method.ReturnType))
        };

        // The namespaces that the method uses.
        var requiredNamespaces = symbol.Parameters
            .SelectMany(static p => TypeSymbolHelper.GetRequiredNamespaces(p.Type))
            .Concat(TypeSymbolHelper.GetRequiredNamespaces(symbol.ReturnType))
            .Distinct()
            .ToEquatableArray();

        var parameters = symbol.Parameters.Zip(
                method.ParameterList.Parameters,
                static (parameterSymbol, syntax) => (Symbol: parameterSymbol, Syntax: syntax))
            .Skip(1)
            .Select(
                static tuple => new FunctorMethodParameter(
                    TypeFullName: tuple.Symbol.Type.ToDisplayString(
                        // We can use minimal qualified format since we
                        // track and include namespaces.
                        format: SymbolDisplayFormat.MinimallyQualifiedFormat),
                    Name: tuple.Symbol.Name,
                    Default: tuple.Syntax.Default?.Value.ToString()))
            .ToEquatableArray();

        return new FunctorMethodDescriptor(
            functor: containerReference,
            returnType: returnType,
            name: method.Identifier.ToString(),
            typeParameters: method.TypeParameterList.Parameters
                .Select(static p => p.Identifier.ValueText)
                .ToEquatableArray(),
            parameters: parameters,
            constraintClauses: symbol.TypeParameters
                .Select(TypeParameterConstraints.FromTypeParameterSymbol)
                .ToEquatableArray(),
            requiredNamespaces: requiredNamespaces,
            location: LocationInfo.CreateFrom(method));
    }
}

/// <summary>
///     A parameter to a functor method.
/// </summary>
/// <param name="TypeFullName">The type.</param>
/// <param name="Name">The parameter name.</param>
/// <param name="Default">The default value (if any).</param>
public sealed record FunctorMethodParameter(
    string TypeFullName,
    string Name,
    string? Default);
