using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Option
{
    /// <summary>
    ///     If <see cref="source" /> is in a Some state, invokes the <paramref name="predicate" />
    ///     and if it returns <c>false</c>, then the result will be None; Some otherwise.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<T> Where<T>(
        this Option<T> source,
        Func<T, bool> predicate)
        where T : notnull =>
        source.TryGet(out var value) && predicate(value)
            ? source
            : Option<T>.None;
}
