using System.Runtime.CompilerServices;
using FxKit.CompilerServices.Models;

namespace FxKit.CompilerServices.Utilities;

/// <summary>
///     Extensions for <see cref="IndentedTextWriter"/>.
/// </summary>
internal static class IndentedTextWriterExtensions
{
    /// <summary>
    ///     Writes the names of the parameters with comma separation.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="parameters"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void WriteParameterNames(
        this IndentedTextWriter writer,
        EquatableArray<BasicParameter> parameters)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            writer.Write(parameters[i].Identifier);
            if (i < parameters.Length - 1)
            {
                writer.Write(", ");
            }
        }
    }
}
