using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace FxKit.CompilerServices.Utilities;

/// <summary>
///     Composes two types.
/// </summary>
public delegate GenericNameSyntax Composer(GenericNameSyntax outer, TypeSyntax inner);

/// <summary>
///     Composes two types.
/// </summary>
public static class TypeComposer
{
    /// <summary>
    ///     Composes the given types. For example, given <c>Result&lt;T, E&gt;</c>
    ///     and <c>Option&lt;T&gt;</c>, returns <c>Result&lt;Option&lt;T&gt;, E&gt;</c>.
    /// </summary>
    /// <remarks>
    ///     We can refactor this to a composition strategy in the future when different
    ///     types should be composed in different ways.
    /// </remarks>
    public static GenericNameSyntax Compose(GenericNameSyntax outer, TypeSyntax inner)
    {
        var args = SeparatedList(outer.TypeArgumentList.Arguments.Skip(1).Prepend(inner));
        return outer.WithTypeArgumentList(TypeArgumentList(args));
    }
}
