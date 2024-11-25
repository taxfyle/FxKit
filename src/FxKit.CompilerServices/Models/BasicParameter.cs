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
/// <param name="HasNullableAnnotation">
///     Whether the parameter has a nullable annotation.
/// </param>
internal sealed record BasicParameter(
    string FullyQualifiedTypeName,
    string Identifier,
    bool HasNullableAnnotation)
{
    /// <summary>
    ///     Converts a <see cref="IParameterSymbol" /> to a <see cref="BasicParameter" />.
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    internal static BasicParameter FromSymbol(IParameterSymbol symbol) => new(
        FullyQualifiedTypeName: symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
        Identifier: symbol.Name,
        HasNullableAnnotation: symbol.NullableAnnotation is NullableAnnotation.Annotated);
}
