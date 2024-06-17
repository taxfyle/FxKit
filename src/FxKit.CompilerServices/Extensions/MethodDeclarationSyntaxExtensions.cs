using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FxKit.CompilerServices.Extensions;

/// <summary>
///     Extension methods for <see cref="MethodDeclarationSyntax" />.
/// </summary>
internal static class MethodDeclarationSyntaxExtensions
{
    /// <summary>
    ///     Indicates whether <see cref="m" /> is an extension method.
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    public static bool IsExtensionMethod(this MethodDeclarationSyntax m)
    {
        if (m.ParameterList.Parameters.Count == 0)
        {
            return false;
        }

        var firstParam = m.ParameterList.Parameters[0];

        return firstParam.Modifiers.Count > 0 && firstParam.Modifiers[0].IsKind(SyntaxKind.ThisKeyword);
    }
}
