using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FxKit;

public static partial class Prelude
{
    /// <summary>
    ///     Returns the None value of an Option.
    /// </summary>
    [DebuggerHidden]
    public static Option.None None => new();

    /// <summary>
    ///     Returns an <see cref="Option{T}" /> in the Some variant.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some<T>(T value)
        where T : notnull => Option<T>.Some(value);

    /// <summary>
    ///     Creates an option from a nullable value.
    /// </summary>
    /// <param name="value">When <c>null</c>, returns <see cref="None" />; otherwise <see cref="Some{T}" /></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Optional<T>(T? value)
        where T : notnull => value is null ? None : Some(value);

    /// <summary>
    ///     Creates an option from a nullable value.
    /// </summary>
    /// <param name="value">When <c>null</c>, returns <see cref="None" />; otherwise <see cref="Some{T}" /></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Optional<T>(T? value)
        where T : struct => !value.HasValue ? None : Some(value.Value);
}
