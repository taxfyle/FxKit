using System.Text;
using Microsoft.CodeAnalysis;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace FxKit.CompilerServices.Utilities;

/// <summary>
///     Helpers for type symbols.
/// </summary>
internal static class TypeSymbolHelper
{
    /// <summary>
    ///     Gets the required namespaces to be able to reference this type.
    ///     Does not deduplicate.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetRequiredNamespaces(ITypeSymbol symbol)
    {
        if (!symbol.ContainingNamespace.IsGlobalNamespace)
        {
            yield return symbol.ContainingNamespace.ToDisplayString();
        }

        if (symbol is not INamedTypeSymbol namedTypeSymbol)
        {
            yield break;
        }

        foreach (var typeArgument in namedTypeSymbol.TypeArguments)
        {
            if (typeArgument.ContainingNamespace.IsGlobalNamespace)
            {
                yield return typeArgument.ContainingNamespace.ToDisplayString();
            }
        }
    }

    /// <summary>
    ///     Gets the fully qualified metadata name for the type.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    public static string GetFullyQualifiedMetadataName(this ISymbol symbol)
    {
        var ns = symbol.ContainingNamespace.ToDisplayString();
        if (symbol.ContainingType is null)
        {
            // Fast path - the symbol is not contained in another type.
            return $"{ns}.{symbol.MetadataName}";
        }

        // Check ancestry.
        var sb = new StringBuilder();
        sb.Append($"{symbol.MetadataName}");

        var parent = symbol.ContainingType;
        do
        {
            sb.Insert(index: 0, $"{parent.MetadataName}.");
            parent = parent.ContainingType;
        } while (parent != null);

        sb.Insert(index: 0, $"{ns}.");
        return sb.ToString();
    }

    /// <summary>
    ///     Recursively extracts types from a symbol.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static IEnumerable<INamedTypeSymbol> GetTypesRecursive(
        ISymbol symbol,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (symbol is INamespaceSymbol ns)
        {
            foreach (var member in ns.GetMembers())
            {
                foreach (var inner in GetTypesRecursive(member, cancellationToken))
                {
                    yield return inner;
                }
            }
            yield break;
        }

        if (symbol is INamedTypeSymbol nt)
        {
            yield return nt;
        }
    }
}
