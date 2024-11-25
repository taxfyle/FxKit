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
    /// <param name="newlineSeparated"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void WriteParameterNames(
        this IndentedTextWriter writer,
        EquatableArray<BasicParameter> parameters,
        bool newlineSeparated = false)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            if (newlineSeparated)
            {
                writer.WriteLine();
            }

            writer.Write(parameters[i].Identifier);
            if (i < parameters.Length - 1)
            {
                writer.Write(newlineSeparated ? "," : ", ");
            }
        }
    }

    /// <summary>
    ///     Writes the types of the parameters with comma separation.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="parameters"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void WriteParameterTypes(
        this IndentedTextWriter writer,
        EquatableArray<BasicParameter> parameters)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            writer.Write(parameter.FullyQualifiedTypeName);
            if (parameter.HasNullableAnnotation)
            {
                writer.Write("?");
            }

            if (i < parameters.Length - 1)
            {
                writer.Write(", ");
            }
        }
    }
}
