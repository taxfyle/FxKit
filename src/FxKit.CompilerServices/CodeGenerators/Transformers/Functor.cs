using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static FxKit.CompilerServices.CodeGenerators.Transformers.Helpers;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Represents an outer container functor type.
/// </summary>
public readonly struct Functor
{
    /// <summary>
    ///     The name of the functor (its identifier).
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The functor's fully qualified name.
    /// </summary>
    public string FullyQualifiedName => FullyQualifiedName(ContainingNamespace, Name);

    /// <summary>
    ///     The functor's signature.
    /// </summary>
    public string Signature { get; }

    /// <summary>
    ///     The namespace the functor is contained within.
    /// </summary>
    public string ContainingNamespace { get; }

    /// <summary>
    ///     The namespace that contains this functor's functor behavior.
    /// </summary>
    public string FunctorMethodsNamespace { get; }

    /// <summary>
    ///     The functor's type parameters.
    /// </summary>
    public IEnumerable<TypeParameterSyntax> TypeParameters { get; }

    /// <summary>
    ///     The container's constraint clauses.
    /// </summary>
    public IEnumerable<TypeParameterConstraintClauseSyntax> ConstraintClauses { get; }

    private Functor(
        string name,
        string signature,
        string containingContainingNamespace,
        IEnumerable<TypeParameterSyntax> typeParameters,
        IEnumerable<TypeParameterConstraintClauseSyntax> constraintClauses,
        string functorMethodsNamespace)
    {
        Name = name;
        Signature = signature;
        ContainingNamespace = containingContainingNamespace;
        TypeParameters = typeParameters;
        ConstraintClauses = constraintClauses;
        FunctorMethodsNamespace = functorMethodsNamespace;
    }

    /// <summary>
    ///     Creates a <see cref="Functor" />.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="functorNamespace"></param>
    /// <returns></returns>
    public static Functor From(INamedTypeSymbol symbol, string functorNamespace)
    {
        return new Functor(
            name: symbol.Name,
            signature: symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            containingContainingNamespace: symbol.ContainingNamespace.ToDisplayString(),
            typeParameters: ToTypeParameterSyntax(symbol.TypeParameters),
            constraintClauses: ToConstraintClauseSyntax(symbol.TypeParameters),
            functorMethodsNamespace: functorNamespace);
    }

    /// <summary>
    ///     Implicitly converts this <see cref="Functor" /> to a <see cref="GenericNameSyntax" />.
    /// </summary>
    /// <param name="functor"></param>
    /// <returns></returns>
    public static implicit operator GenericNameSyntax(Functor functor) =>
        (GenericNameSyntax)ParseName(functor.Signature);
}

/// <summary>
///     Represents a reference in code to a functor.
/// </summary>
public readonly struct FunctorReference
{
    /// <summary>
    ///     The name of the referenced functor.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The fully qualified name of the referenced functor.
    /// </summary>
    public string FullyQualifiedName => FullyQualifiedName(ContainingNamespace, Name);

    /// <summary>
    ///     The referenced functors signature including its arguments.
    /// </summary>
    public string Signature { get; }

    /// <summary>
    ///     The functor reference's type arguments.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TypeSyntax> TypeArguments { get; }

    /// <summary>
    ///     The namespace that this functor's type declaration is contained within.
    /// </summary>
    public string ContainingNamespace { get; }

    /// <summary>
    ///     The underlying value.
    /// </summary>
    private GenericNameSyntax Value { get; }

    private FunctorReference(
        string name,
        string signature,
        IEnumerable<TypeSyntax> typeArguments,
        string containingNamespace,
        GenericNameSyntax value)
    {
        Name = name;
        Signature = signature;
        TypeArguments = typeArguments;
        ContainingNamespace = containingNamespace;
        Value = value;
    }

    /// <summary>
    ///     Clones the reference but replaces the functor name with the new one.
    /// </summary>
    /// <param name="functor"></param>
    /// <param name="containingNamespace"></param>
    /// <returns></returns>
    public FunctorReference ReplaceReference(string functor, string containingNamespace)
    {
        var value = GenericName(functor)
            .WithTypeArgumentList(TypeArgumentList(SeparatedList(TypeArguments)));
        return From(value, containingNamespace);
    }

    /// <summary>
    ///     Creates a <see cref="FunctorReference" />.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="containingNamespace"></param>
    /// <returns></returns>
    public static FunctorReference From(GenericNameSyntax value, string containingNamespace)
    {
        return new FunctorReference(
            name: value.Identifier.ToString(),
            signature: value.Identifier.ToFullString(),
            typeArguments: value.TypeArgumentList.Arguments,
            containingNamespace: containingNamespace,
            value: value);
    }

    /// <summary>
    ///     Implicitly converts this <see cref="FunctorReference" /> to a <see cref="GenericNameSyntax" />.
    /// </summary>
    /// <param name="adt"></param>
    /// <returns></returns>
    public static implicit operator GenericNameSyntax(FunctorReference adt) => adt.Value;
}
