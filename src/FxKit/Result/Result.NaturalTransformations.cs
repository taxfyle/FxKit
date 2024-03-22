using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

namespace FxKit;

public static partial class Result
{
    /// <summary>
    ///     Turns a <see cref="Result{T,E}" /> into a <see cref="Validation{T,E}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<T, E> ToValidation<T, E>(
        this Result<T, E> source)
        where T : notnull
        where E : notnull =>
        source.TryGet(out var ok, out var err)
            ? Valid(ok)
            : Invalid(err);

    /// <summary>
    ///     Turns a <see cref="Result{T,E}" /> into an <see cref="Option{T}" />.
    ///     If in an Ok state, the option will be in the Some variant; None otherwise.
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Option<T> ToOption<T, E>(this Result<T, E> source)
        where T : notnull
        where E : notnull =>
        source.Match(
            Some,
            _ => None);
}
