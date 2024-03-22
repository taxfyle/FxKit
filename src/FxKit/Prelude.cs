global using static FxKit.Prelude;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FxKit;

/// <summary>
///     Used as a `using static` to provide common data type factories as top-level methods.
/// </summary>
public static partial class Prelude
{
    /// <summary>
    ///     Returns the input argument.
    /// </summary>
    /// <param name="v"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Identity<T>(T v) => v;
}
