using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Represents a method return type - either a general primitive `T` or some <see cref="Transformers.Functor" />.
/// </summary>
public abstract record ReturnType
{
    /// <summary>
    ///     A functor return type.
    /// </summary>
    public sealed record Functor(FunctorReference FunctorReference) : ReturnType
    {
        public FunctorReference FunctorReference { get; } = FunctorReference;
        public static ReturnType Of(FunctorReference cr) => new Functor(cr);
    }

    /// <summary>
    ///     A primitive generic return type.
    /// </summary>
    public sealed record Primitive(TypeSyntax Type) : ReturnType
    {
        public TypeSyntax Type { get; } = Type;
        public static ReturnType Of(TypeSyntax type) => new Primitive(type);
    }
}

/// <summary>
///     Represents a descriptor for a method that operates on a functor.
/// </summary>
public readonly struct FunctorMethodDescriptor
{
    /// <summary>
    ///     The parent functor reference this method operates upon.
    /// </summary>
    public FunctorReference Functor { get; }

    /// <summary>
    ///     The return type of this method.
    /// </summary>
    public ReturnType ReturnType { get; }

    /// <summary>
    ///     The name of this method.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The Type Parameters that this method takes.
    /// </summary>
    public IEnumerable<TypeParameterSyntax> TypeParameters { get; }

    /// <summary>
    ///     The parameters this method takes.
    /// </summary>
    /// <remarks>
    ///     Does not include the "source" "this" argument from the extension method.
    /// </remarks>
    public IEnumerable<ParameterSyntax> Parameters { get; }

    /// <summary>
    ///     The constraint clauses present on this method.
    /// </summary>
    public IEnumerable<TypeParameterConstraintClauseSyntax> ConstraintClauses { get; }

    /// <summary>
    ///     The namespaces that method uses.
    /// </summary>
    public ImmutableHashSet<string> RequiredNamespaces { get; }

    /// <summary>
    ///     Leading trivia for the method.
    /// </summary>
    public SyntaxTriviaList LeadingTrivia { get; }

    /// <summary>
    ///     Location of the method in source code.
    /// </summary>
    public Location Location { get; }

    private FunctorMethodDescriptor(
        FunctorReference functor,
        ReturnType returnType,
        string name,
        IEnumerable<TypeParameterSyntax> typeParameters,
        IEnumerable<ParameterSyntax> parameters,
        IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses,
        ImmutableHashSet<string> requiredNamespaces,
        SyntaxTriviaList leadingTrivia,
        Location location)
    {
        Functor = functor;
        ReturnType = returnType;
        Name = name;
        TypeParameters = typeParameters;
        Parameters = parameters;
        ConstraintClauses = constraintClauses;
        RequiredNamespaces = requiredNamespaces;
        LeadingTrivia = leadingTrivia;
        Location = location;
    }

    /// <summary>
    ///     Creates a copy of this functor method descriptor but uses the specified
    ///     functor reference.
    /// </summary>
    /// <param name="functorReference"></param>
    /// <returns></returns>
    public FunctorMethodDescriptor ReplaceFunctor(FunctorReference functorReference)
    {
        return new FunctorMethodDescriptor(
            functor: functorReference,
            returnType: ReturnType,
            name: Name,
            typeParameters: TypeParameters,
            parameters: Parameters,
            constraintClauses: ConstraintClauses,
            requiredNamespaces: RequiredNamespaces.Add(functorReference.ContainingNamespace),
            leadingTrivia: LeadingTrivia,
            location: Location);
    }

    /// <summary>
    ///     Creates an <see cref="FunctorMethodDescriptor" /> from a <see cref="MethodDeclarationSyntax" /> node.
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
                "Can't create a FunctorMethodDescriptor from a method with no parameters.");
        }

        var thisParamContainingNamespace =
            symbol.Parameters[0].Type.ContainingNamespace.ToDisplayString();
        var containerReference = FunctorReference.From(
            (GenericNameSyntax)method.ParameterList.Parameters.First().Type!,
            thisParamContainingNamespace);

        var returnType = method.ReturnType switch
        {
            GenericNameSyntax gns => ReturnType.Functor.Of(
                FunctorReference.From(
                    gns,
                    symbol.ReturnType.ContainingNamespace.ToDisplayString())),
            _ => ReturnType.Primitive.Of(method.ReturnType)
        };

        var requiredNamespaces = symbol.Parameters.Select(p => p.ContainingNamespace)
            .Append(symbol.ReturnType.ContainingNamespace)
            .Select(n => n.ToDisplayString())
            .ToImmutableHashSet();

        return new FunctorMethodDescriptor(
            functor: containerReference,
            returnType: returnType,
            name: method.Identifier.ToString(),
            typeParameters: method.TypeParameterList.Parameters,
            parameters: method.ParameterList.Parameters.Skip(1),
            constraintClauses: method.ConstraintClauses,
            requiredNamespaces: requiredNamespaces,
            leadingTrivia: method.GetLeadingTrivia(),
            location: method.GetLocation());
    }
}
