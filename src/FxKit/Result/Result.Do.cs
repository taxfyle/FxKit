using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Result
{
    /// <summary>
    ///     Calls the <paramref name="callback" /> if the result is in an Ok state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="callback"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TOk, TErr> Do<TOk, TErr>(
        this Result<TOk, TErr> source,
        Action<TOk> callback)
        where TOk : notnull
        where TErr : notnull
    {
        if (source.TryGet(out var ok, out _))
        {
            callback(ok);
        }

        return source;
    }

    /// <summary>
    ///     Calls the <paramref name="callback" /> if the result is in an Err state.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="callback"></param>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TErr"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Result<TOk, TErr> DoErr<TOk, TErr>(
        this Result<TOk, TErr> source,
        Action<TErr> callback)
        where TOk : notnull
        where TErr : notnull
    {
        if (!source.TryGet(out _, out var err))
        {
            callback(err);
        }

        return source;
    }
}
