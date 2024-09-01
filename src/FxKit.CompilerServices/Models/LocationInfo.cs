using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace FxKit.CompilerServices.Models;

/// <summary>
///     Slimmed down location information with equality semantics.
/// </summary>
/// <param name="FilePath"></param>
/// <param name="TextSpan"></param>
/// <param name="LineSpan"></param>
internal record LocationInfo(string FilePath, TextSpan TextSpan, LinePositionSpan LineSpan)
{
    /// <summary>
    ///     Converts this <see cref="LocationInfo" /> to a <see cref="Location" />.
    /// </summary>
    /// <returns></returns>
    public Location ToLocation()
        => Location.Create(FilePath, TextSpan, LineSpan);

    /// <summary>
    ///     Creates a <see cref="LocationInfo" /> from a <see cref="SyntaxNode" />.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static LocationInfo? CreateFrom(SyntaxNode node)
        => CreateFrom(node.GetLocation());

    /// <summary>
    ///     Creates a <see cref="LocationInfo" /> from a <see cref="Location" />.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static LocationInfo? CreateFrom(Location location)
    {
        if (location.SourceTree is null)
        {
            return null;
        }

        return new LocationInfo(
            location.SourceTree.FilePath,
            location.SourceSpan,
            location.GetLineSpan().Span);
    }
}
