using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Possible constraints on a type parameter.
/// </summary>
[Flags]
internal enum Constraints : byte
{
    /// <summary>
    ///     No constraints.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Must be constructable (new())
    /// </summary>
    Constructor = 1 << 0,

    /// <summary>
    ///     Must not be null (notnull).
    /// </summary>
    NotNull = 1 << 1,

    /// <summary>
    ///     Must be a reference type (class).
    /// </summary>
    ReferenceType = 1 << 2,

    /// <summary>
    ///     Must be a value type (struct).
    /// </summary>
    ValueType = 1 << 3,

    /// <summary>
    ///     When set, should check the list of constrained types.
    /// </summary>
    Types = 1 << 4
}

/// <summary>
///     Constrained to a type.
/// </summary>
internal readonly record struct TypeConstraint(string FullyQualifiedTypeName);

/// <summary>
///     Constraints on a type parameter.
/// </summary>
/// <param name="Name">Name of the type parameter.</param>
/// <param name="Constraints">Any constraints on the type parameter.</param>
/// <param name="TypeConstraints">
///     If <see cref="Constraints"/> includes <see cref="Transformers.Constraints.Types"/>,
///     will contain the list of types that the type parameter must implement.
/// </param>
internal readonly record struct TypeParameterConstraints(
    string Name,
    Constraints Constraints,
    EquatableArray<TypeConstraint> TypeConstraints)
{
    /// <summary>
    ///     Constructs a constraint syntax.
    /// </summary>
    /// <returns></returns>
    public TypeParameterConstraintClauseSyntax? ToConstraintClauseSyntax()
    {
        if (Constraints == Constraints.None)
        {
            return null;
        }

        List<TypeParameterConstraintSyntax> constraints = [];

        if (Constraints.HasFlag(Constraints.Constructor))
        {
            constraints.Add(ConstructorConstraint());
        }

        if (Constraints.HasFlag(Constraints.NotNull))
        {
            constraints.Add(TypeConstraint(IdentifierName("notnull")));
        }

        if (Constraints.HasFlag(Constraints.ReferenceType))
        {
            constraints.Add(ClassOrStructConstraint(SyntaxKind.ClassConstraint));
        }

        if (Constraints.HasFlag(Constraints.ValueType))
        {
            constraints.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint));
        }

        if (Constraints.HasFlag(Constraints.Types))
        {
            constraints.AddRange(
                TypeConstraints.Select(
                    static x =>
                    {
                        var typeSyntax = ParseTypeName(x.FullyQualifiedTypeName);
                        return TypeConstraint(typeSyntax);
                    }));
        }

        return TypeParameterConstraintClause(
            IdentifierName(Name),
            SeparatedList(constraints));
    }

    /// <summary>
    ///     Constructs a <see cref="TypeParameterConstraints"/> from a type parameter symbol.
    /// </summary>
    /// <param name="typeParameterSymbol"></param>
    /// <returns></returns>
    public static TypeParameterConstraints FromTypeParameterSymbol(
        ITypeParameterSymbol typeParameterSymbol)
    {
        var constraints = Constraints.None;
        if (typeParameterSymbol.HasConstructorConstraint)
        {
            constraints |= Constraints.Constructor;
        }

        if (typeParameterSymbol.HasNotNullConstraint)
        {
            constraints |= Constraints.NotNull;
        }

        if (typeParameterSymbol.HasReferenceTypeConstraint)
        {
            constraints |= Constraints.ReferenceType;
        }

        if (typeParameterSymbol.HasValueTypeConstraint)
        {
            constraints |= Constraints.ValueType;
        }

        var typeConstraints = EquatableArray<TypeConstraint>.Empty;
        if (!typeParameterSymbol.ConstraintTypes.IsEmpty)
        {
            constraints |= Constraints.Types;
            typeConstraints = typeParameterSymbol.ConstraintTypes
                .Select(
                    x => new TypeConstraint(x.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))
                .ToEquatableArray();
        }

        return new TypeParameterConstraints(
            Name: typeParameterSymbol.Name,
            Constraints: constraints,
            TypeConstraints: typeConstraints);
    }
}
