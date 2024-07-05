using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Represents either a concrete type, or a type constructed from a generic type.
/// </summary>
internal abstract record ConcreteOrConstructedType
{
    /// <summary>
    ///     A constructed generic return type (e.g <c>Option&lt;int&gt;</c>).
    /// </summary>
    internal sealed record Constructed(ConstructedType ConstructedType) : ConcreteOrConstructedType
    {
        public static ConcreteOrConstructedType Of(ConstructedType constructedType) =>
            new Constructed(constructedType);
    }

    /// <summary>
    ///     A concrete return type (e.g <c>int</c>).
    /// </summary>
    internal sealed record Concrete(ConcreteType Type) : ConcreteOrConstructedType
    {
        public static ConcreteOrConstructedType Of(ConcreteType type) => new Concrete(type);
    }

    /// <summary>
    ///     Converts a <see cref="ConcreteOrConstructedType"/> to <see cref="TypeSyntax"/>.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public TypeSyntax ToTypeSyntax() => this switch
    {
        Concrete(var type)    => type.ToTypeSyntax(),
        Constructed(var type) => type.ToTypeSyntax(),
        _                     => throw new ArgumentOutOfRangeException()
    };

    /// <summary>
    ///     Converts a <see cref="ITypeSymbol" /> to a <see cref="ConcreteOrConstructedType" />.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static ConcreteOrConstructedType FromTypeSymbol(ITypeSymbol symbol) =>
        symbol switch
        {
            INamedTypeSymbol { TypeArguments.Length: > 0 } type =>
                Constructed.Of(constructedType: ConstructedType.From(type)),
            _ => Concrete.Of(type: ConcreteType.From(symbol))
        };
}

/// <summary>
///     Represents a concrete (non-generic) type.
/// </summary>
/// <param name="Type"></param>
internal record ConcreteType(string Type)
{
    /// <summary>
    ///     Converts a <see cref="ConcreteType" /> to a <see cref="TypeSyntax" />.
    /// </summary>
    /// <returns></returns>
    public TypeSyntax ToTypeSyntax() => ParseTypeName(Type);

    /// <summary>
    ///     Creates a <see cref="ConcreteType" /> from a <see cref="ITypeSymbol" />.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static ConcreteType From(ITypeSymbol type) => new(type.ToString());
}

/// <summary>
///     Represents a reference in code to a constructed generic type (that may or may not be a functor).
/// </summary>
internal record ConstructedType
{
    /// <summary>
    ///     The name of the generic type that the type is constructed from.
    ///     For example, the name of `Option&lt;T&gt;` would be `Option`.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The fully qualified name of the constructed type.
    /// </summary>
    public string FullyQualifiedName => Helpers.FullyQualifiedName(ContainingNamespace, Name);

    /// <summary>
    ///     The type arguments that were passed to the generic type to construct it.
    /// </summary>
    /// <returns></returns>
    public EquatableArray<ConcreteOrConstructedType> TypeArguments { get; }

    /// <summary>
    ///     The namespace that the generic type declaration is contained within.
    /// </summary>
    public string ContainingNamespace { get; }

    private ConstructedType(
        string name,
        EquatableArray<ConcreteOrConstructedType> typeArguments,
        string containingNamespace)
    {
        Name = name;
        TypeArguments = typeArguments;
        ContainingNamespace = containingNamespace;
    }

    /// <summary>
    ///     Clones the constructed type, but replaces the name of it. For example,
    ///     if the constructed type is `IEnumerable&lt;string&gt;`, and the new name is `IReadOnlyList`,
    ///     then the resulting constructed type would be `IReadOnlyList&lt;string&gt;`.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="containingNamespace"></param>
    /// <returns></returns>
    public ConstructedType ReplaceReference(string name, string containingNamespace) =>
        new(
            name: name,
            typeArguments: TypeArguments,
            containingNamespace: containingNamespace);

    /// <summary>
    ///     Converts this <see cref="ConstructedType" /> to a <see cref="TypeSyntax" />.
    /// </summary>
    /// <returns></returns>
    public TypeSyntax ToTypeSyntax()
    {
        var parsedTypeArgs = TypeArguments.Select(static t => t.ToTypeSyntax())
            .ToArray();
        return GenericName(Name)
            .WithTypeArgumentList(TypeArgumentList(SeparatedList(parsedTypeArgs)));
    }

    /// <summary>
    ///     Creates a <see cref="ConstructedType" />.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static ConstructedType From(
        INamedTypeSymbol symbol) =>
        new(
            name: symbol.Name,
            containingNamespace: symbol.ContainingNamespace.ToDisplayString(),
            typeArguments: symbol.TypeArguments.Select(ConcreteOrConstructedType.FromTypeSymbol)
                .ToEquatableArray());
}
