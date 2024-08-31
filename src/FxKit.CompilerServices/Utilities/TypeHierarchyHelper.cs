using FxKit.CompilerServices.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FxKit.CompilerServices.Utilities;

/// <summary>
///     Helpers for type hierarchies.
/// </summary>
internal static class TypeHierarchyHelper
{
    /// <summary>
    ///     Gets the type hierarchy for a given syntax node.
    /// </summary>
    /// <param name="node">
    ///     The syntax node to get the type hierarchy for.
    /// </param>
    /// <returns>
    ///     The type hierarchy for the given syntax node. The outer-most type
    ///     appears first.
    /// </returns>
    public static EquatableArray<TypeHierarchyNode> GetTypeHierarchy(SyntaxNode node) =>
        node.AncestorsAndSelf()
            .OfType<TypeDeclarationSyntax>()
            .Select(
                static n => new TypeHierarchyNode(
                    Modifiers: n.Modifiers.ToString(),
                    Keyword: n.Keyword.ValueText,
                    IdentifierWithSignature: GetIdentifierWithSignature(n)))
            .Reverse()
            .ToEquatableArray();

    /// <summary>
    ///     Gets the identifier name including any generic parameters.
    /// </summary>
    /// <param name="typeDeclarationSyntax">
    ///     The type declaration syntax to get the identifier from.
    /// </param>
    /// <returns>
    ///     The identifier name including any generic parameters.
    /// </returns>
    private static string GetIdentifierWithSignature(TypeDeclarationSyntax typeDeclarationSyntax)
    {
        if (typeDeclarationSyntax.TypeParameterList is not { Parameters.Count: > 0 })
        {
            return typeDeclarationSyntax.Identifier.ValueText;
        }

        var typeParams =
            typeDeclarationSyntax.TypeParameterList.Parameters.Select(
                static p => p.Identifier.ValueText);
        var typeParamsJoined = string.Join(", ", typeParams);
        return $"{typeDeclarationSyntax.Identifier}<{typeParamsJoined}>";
    }
}
