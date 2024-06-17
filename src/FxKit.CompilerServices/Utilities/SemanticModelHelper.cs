using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator

namespace FxKit.CompilerServices.Utilities;

internal  static class SemanticModelHelper
{
    /// <summary>
    ///     Whether the attribute list contains the exact attribute we are looking for.
    /// </summary>
    /// <param name="attrListSyntax"></param>
    /// <param name="semanticModel"></param>
    /// <param name="attrFullyQualifiedName"></param>
    /// <returns></returns>
    public static bool ContainsExactAttribute(
        SyntaxList<AttributeListSyntax> attrListSyntax,
        SemanticModel semanticModel,
        string attrFullyQualifiedName)
    {
        // We use plain foreach here because perf.
        foreach (var attrList in attrListSyntax)
        {
            foreach (var attr in attrList.Attributes)
            {
                if (IsExactAttribute(semanticModel, attr, attrFullyQualifiedName))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Whether the attribute list contains any of the attributes we are looking for.
    /// </summary>
    /// <param name="attrListSyntax"></param>
    /// <param name="attrFullyQualifiedNames"></param>
    /// <param name="semanticModel"></param>
    /// <param name="foundAttribute">The attribute syntax, if found.</param>
    /// <returns></returns>
    public static bool ContainsAnyAttribute(
        SyntaxList<AttributeListSyntax> attrListSyntax,
        SemanticModel semanticModel,
        IReadOnlyCollection<string> attrFullyQualifiedNames,
        out AttributeSyntax? foundAttribute)
    {
        // We use plain foreach here because perf.
        foreach (var attrList in attrListSyntax)
        {
            foreach (var attr in attrList.Attributes)
            {
                foreach (var attrFullyQualifiedName in attrFullyQualifiedNames)
                {
                    if (!IsExactAttribute(semanticModel, attr, attrFullyQualifiedName))
                    {
                        continue;
                    }

                    foundAttribute = attr;
                    return true;
                }
            }
        }

        foundAttribute = null;
        return false;
    }

    /// <summary>
    ///     Check whether the given attribute syntax represents
    ///     the type of <paramref name="attrFullyQualifiedName" />.
    /// </summary>
    /// <remarks>
    ///     It is not enough to check the syntax attribute node name, as it could have been
    ///     aliased. We use the semantic model to determine if the actual type is the union
    ///     type we declared.
    /// </remarks>
    /// <param name="semanticModel"></param>
    /// <param name="attr"></param>
    /// <param name="attrFullyQualifiedName"></param>
    /// <returns></returns>
    public static bool IsExactAttribute(
        SemanticModel semanticModel,
        AttributeSyntax attr,
        string attrFullyQualifiedName)
    {
        var attrSymbol = semanticModel.GetSymbolInfo(attr).Symbol;
        if (attrSymbol is null)
        {
            return false;
        }

        var attributeContainingTypeSymbol = attrSymbol.ContainingType;
        var fullName = attributeContainingTypeSymbol.ToDisplayString();

        // Is this the attribute we are looking for?
        return fullName == attrFullyQualifiedName;
    }
}
