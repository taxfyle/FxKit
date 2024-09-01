namespace FxKit.CompilerServices.Models;

/// <summary>
///     A node in a type hierarchy.
/// </summary>
/// <param name="Modifiers">
///     The modifiers of the type, such as `partial`, `static` etc.
/// </param>
/// <param name="Keyword">
///     The type keyword, such as `class`, `record` etc.
/// </param>
/// <param name="IdentifierWithSignature">
///     The identifier of the type, including any generic parameters.
/// </param>
internal sealed record TypeHierarchyNode(
    string Modifiers,
    string Keyword,
    string IdentifierWithSignature);
