using FxKit.CompilerServices.Models;

namespace FxKit.CompilerServices.Utilities;

/// <summary>
///     Manages writing the type hierarchy to an <see cref="IndentedTextWriter"/>.
/// </summary>
internal class TypeHierarchyWriter : IDisposable
{
    /// <summary>
    ///     The underlying writer.
    /// </summary>
    private readonly IndentedTextWriter _writer;

    /// <summary>
    ///     How many blocks of nesting are in use.
    /// </summary>
    private readonly int _blocks;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="blocks"></param>
    private TypeHierarchyWriter(IndentedTextWriter writer, int blocks)
    {
        _writer = writer;
        _blocks = blocks;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        for (var i = 0; i < _blocks; i++)
        {
            _writer.WriteLine();
            _writer.DecreaseIndent();
            _writer.Write("}");
        }
    }

    /// <summary>
    ///     Writes the type hierarchy to the given writer, and returns
    ///     an instance of <see cref="TypeHierarchyWriter"/> that should be disposed
    ///     when the trailing `}`s should be written.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="entries"></param>
    /// <returns></returns>
    internal static TypeHierarchyWriter WriteTypeHierarchy(
        IndentedTextWriter writer,
        IEnumerable<TypeHierarchyNode> entries)
    {
        var blocks = 0;
        foreach (var entry in entries)
        {
            writer.WriteLine($"{entry.Modifiers} {entry.Keyword} {entry.IdentifierWithSignature}");
            writer.WriteLine("{");
            writer.IncreaseIndent();
            blocks++;
        }

        return new TypeHierarchyWriter(writer, blocks);
    }
}
