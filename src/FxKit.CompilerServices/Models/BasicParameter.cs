using Microsoft.CodeAnalysis;

namespace FxKit.CompilerServices.Models;

/// <summary>
///     Describes a parameter in its basic form.
/// </summary>
/// <param name="FullyQualifiedTypeName">
///     The fully qualified type name of the parameter.
/// </param>
/// <param name="Identifier">
///     The name of the parameter.
/// </param>
/// <param name="RequiresAdditionalNullableAnnotation">
///     Whether the parameter requires printing an additional `?`.
///     This may be false in case the <see cref="FullyQualifiedTypeName"/> already includes
///     the `?`, such as for value types.
/// </param>
internal sealed record BasicParameter(
    string FullyQualifiedTypeName,
    string Identifier,
    bool RequiresAdditionalNullableAnnotation)
{
    /// <summary>
    ///     Converts a <see cref="IParameterSymbol" /> to a <see cref="BasicParameter" />.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    internal static BasicParameter FromSymbol(IParameterSymbol symbol)
    {
        var fullyQualifiedTypeName =
            symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // If the type is a nullable value type, the type name will include a `?` as well, in which
        // case we won't need to write an additional token when printing.
        var requiresAdditionalNullableAnnotation =
            symbol.NullableAnnotation is NullableAnnotation.Annotated &&
            fullyQualifiedTypeName[^1] != '?';

        return new BasicParameter(
            FullyQualifiedTypeName: fullyQualifiedTypeName,
            Identifier: symbol.Name,
            RequiresAdditionalNullableAnnotation: requiresAdditionalNullableAnnotation);
    }
}
