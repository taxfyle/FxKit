using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Represents a method return type - either a concrete type or a constructed
///     type from a generic type.
/// </summary>
internal abstract record ReturnType
{
    /// <summary>
    ///     A constructed generic return type (e.g <c>Option&lt;int&gt;</c>).
    /// </summary>
    internal sealed record Constructed(ConstructedType ConstructedType) : ReturnType
    {
        public static ReturnType Of(ConstructedType constructedType) => new Constructed(constructedType);
    }

    /// <summary>
    ///     A concrete return type (e.g <c>int</c>).
    /// </summary>
    internal sealed record Concrete(ConcreteType Type) : ReturnType
    {
        public static ReturnType Of(ConcreteType type) => new Concrete(type);
    }
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
    ///     Creates a <see cref="ConcreteType" /> from a <see cref="TypeSyntax" />.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static ConcreteType From(TypeSyntax type) => new(type.ToString());
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
    public EquatableArray<string> TypeArguments { get; }

    /// <summary>
    ///     The namespace that the generic type declaration is contained within.
    /// </summary>
    public string ContainingNamespace { get; }

    private ConstructedType(
        string name,
        EquatableArray<string> typeArguments,
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
        var parsedTypeArgs = TypeArguments.Select(static t => ParseTypeName(t)).ToArray();
        return GenericName(Name)
            .WithTypeArgumentList(TypeArgumentList(SeparatedList(parsedTypeArgs)));
    }

    /// <summary>
    ///     Creates a <see cref="ConstructedType" />.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="containingNamespace"></param>
    /// <returns></returns>
    public static ConstructedType From(GenericNameSyntax value, string containingNamespace) =>
        new(
            name: value.Identifier.ToString(),
            typeArguments: value.TypeArgumentList.Arguments
                .Select(static a => a.ToString())
                .ToEquatableArray(),
            containingNamespace: containingNamespace);
}
