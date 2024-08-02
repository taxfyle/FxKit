using System.Diagnostics;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;
using FxKit.Extensions;

namespace FxKit;

public static partial class Validation
{
    /// <summary>
    ///     Applies the given argument to the Validation's inner unary function and executes it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Applicative behavior of the <see cref="Validation{T,E}" /> Monad is implemented in a manner
    ///         that will harvest and coalesce errors.
    ///     </para>
    ///     <para>
    ///         You can use this behavior to eagerly run multiple validators and collect the validation
    ///         failures from each into one <see cref="Validation{T,E}" /> containing either the validated
    ///         value or the various validation errors that occurred throughout the process.
    ///     </para>
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="arg"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TNewValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<TNewValid, TInvalid> Apply<TValid, TNewValid, TInvalid>(
        this Validation<Func<TValid, TNewValid>, TInvalid> source,
        Validation<TValid, TInvalid> arg)
        where TValid : notnull
        where TNewValid : notnull
        where TInvalid : notnull
        => source.Match(
            Valid: f => arg.Match(
                Valid: t => Validation<TNewValid, TInvalid>.Valid(f(t)),
                Invalid: err => Validation<TNewValid, TInvalid>.Invalid(err)),
            Invalid: sourceErr => arg.Match(
                Valid: _ => Validation<TNewValid, TInvalid>.Invalid(sourceErr),
                Invalid: argErr => Validation<TNewValid, TInvalid>.Invalid(sourceErr.Concat(argErr))));

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<Func<T2, R>, TInvalid> Apply<T1, T2, R, TInvalid>(
        this Validation<Func<T1, T2, R>, TInvalid> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull
        => source.Map(f => f.Curry()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<Func<T2, T3, R>, TInvalid> Apply<T1, T2, T3, R, TInvalid>(
        this Validation<Func<T1, T2, T3, R>, TInvalid> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, T3, T4, R>, TInvalid> Apply<T1, T2, T3, T4, R, TInvalid>(
        this Validation<Func<T1, T2, T3, T4, R>, TInvalid> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<Func<T2, T3, T4, T5, R>, TInvalid> Apply<T1, T2, T3, T4, T5, R, TInvalid>(
        this Validation<Func<T1, T2, T3, T4, T5, R>, TInvalid> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<Func<T2, T3, T4, T5, T6, R>, TInvalid> Apply<T1, T2, T3, T4, T5, T6, R,
        TInvalid>(
        this Validation<Func<T1, T2, T3, T4, T5, T6, R>, TInvalid> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<Func<T2, T3, T4, T5, T6, T7, R>, TInvalid> Apply<T1, T2, T3, T4, T5, T6, T7,
        R, TInvalid>(
        this Validation<Func<T1, T2, T3, T4, T5, T6, T7, R>, TInvalid> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<Func<T2, T3, T4, T5, T6, T7, T8, R>, TInvalid>
        Apply<T1, T2, T3, T4, T5, T6, T7, T8, R, TInvalid>(
            this Validation<Func<T1, T2, T3, T4, T5, T6, T7, T8, R>, TInvalid> source,
            Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<Func<T2, T3, T4, T5, T6, T7, T8, T9, R>, TInvalid>
        Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R, TInvalid>(
            this Validation<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>, TInvalid> source,
            Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GenerateTransformer]
    public static Validation<Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, R>, TInvalid>
        Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R, TInvalid>(
            this Validation<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>, TInvalid> source,
            Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => source.Map(f => f.CurryFirst()).Apply(arg);

    // Summary: You don't need to worry about these. They were added to solve specific type inference
    // edge cases and everything should just work.
    //
    // These `Apply` methods are accessible on the `Validation.Valid<T>` variant and are used for type
    // inference purposes.
    //
    // For each arity, there are two overloads - one that works on `Validation<T, E>` in cases
    // where the error type is known, and one that works in terms of `Validation.Valid<T>`, which
    // assumes you are passing a valid argument into a valid function.
    //
    // The former type act in the same way as `BakeErr`, and prevent needing to specify the error type.
    // These methods construct a `Validation<T, E>` instance and delegate to the instance's
    // `Apply` implementation.
    //
    // The latter type are used when you want to pass a valid value as the first argument for a factory
    // without going through a validation function. This is a special case of the above.

    #region Valid-Side Apply Helper

    /// <summary>
    ///     Applies the given argument to the Validation's inner unary function and executes it.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Applicative behavior of the <see cref="Validation{T,E}" /> Monad is implemented in a manner
    ///         that will harvest and coalesce errors.
    ///     </para>
    ///     <para>
    ///         You can use this behavior to eagerly run multiple validators and collect the validation
    ///         failures from each into one <see cref="Validation{T,E}" /> containing either the validated
    ///         value or the various validation errors that occurred throughout the process.
    ///     </para>
    /// </remarks>
    /// <param name="source"></param>
    /// <param name="arg"></param>
    /// <typeparam name="TValid"></typeparam>
    /// <typeparam name="TNewValid"></typeparam>
    /// <typeparam name="TInvalid"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<TNewValid, TInvalid> Apply<TValid, TNewValid, TInvalid>(
        this Valid<Func<TValid, TNewValid>> source,
        Validation<TValid, TInvalid> arg)
        where TValid : notnull
        where TNewValid : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Applies the argument to the given function.
    /// </summary>
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<R> Apply<T, R>(
        this Valid<Func<T, R>> source,
        Valid<T> arg)
        where T : notnull
        where R : notnull
        => source.Map(f => f(arg.Value));

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, R>, TInvalid> Apply<T1, T2, R, TInvalid>(
        this Valid<Func<T1, T2, R>> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<Func<T2, R>> Apply<T1, T2, R>(
        this Valid<Func<T1, T2, R>> source,
        Valid<T1> arg)
        where T1 : notnull
        => source.Map(f => f.Curry()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, T3, R>, TInvalid> Apply<T1, T2, T3, R, TInvalid>(
        this Valid<Func<T1, T2, T3, R>> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<Func<T2, T3, R>> Apply<T1, T2, T3, R>(
        this Valid<Func<T1, T2, T3, R>> source,
        Valid<T1> arg)
        where T1 : notnull
        => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, T3, T4, R>, TInvalid> Apply<T1, T2, T3, T4, R, TInvalid>(
        this Valid<Func<T1, T2, T3, T4, R>> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<Func<T2, T3, T4, R>> Apply<T1, T2, T3, T4, R>(
        this Valid<Func<T1, T2, T3, T4, R>> source,
        Valid<T1> arg)
        where T1 : notnull
        => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, T3, T4, T5, R>, TInvalid> Apply<T1, T2, T3, T4, T5, R, TInvalid>(
        this Valid<Func<T1, T2, T3, T4, T5, R>> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<Func<T2, T3, T4, T5, R>> Apply<T1, T2, T3, T4, T5, R>(
        this Valid<Func<T1, T2, T3, T4, T5, R>> source,
        Valid<T1> arg)
        where T1 : notnull
        => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, T3, T4, T5, T6, R>, TInvalid> Apply<T1, T2, T3, T4, T5, T6, R,
        TInvalid>(
        this Valid<Func<T1, T2, T3, T4, T5, T6, R>> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<Func<T2, T3, T4, T5, T6, R>> Apply<T1, T2, T3, T4, T5, T6, R>(
        this Valid<Func<T1, T2, T3, T4, T5, T6, R>> source,
        Valid<T1> arg)
        where T1 : notnull
        => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, T3, T4, T5, T6, T7, R>, TInvalid> Apply<T1, T2, T3, T4, T5, T6, T7,
        R, TInvalid>(
        this Valid<Func<T1, T2, T3, T4, T5, T6, T7, R>> source,
        Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<Func<T2, T3, T4, T5, T6, T7, R>> Apply<T1, T2, T3, T4, T5, T6, T7, R>(
        this Valid<Func<T1, T2, T3, T4, T5, T6, T7, R>> source,
        Valid<T1> arg)
        where T1 : notnull
        => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, T3, T4, T5, T6, T7, T8, R>, TInvalid>
        Apply<T1, T2, T3, T4, T5, T6, T7, T8, R, TInvalid>(
            this Valid<Func<T1, T2, T3, T4, T5, T6, T7, T8, R>> source,
            Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<Func<T2, T3, T4, T5, T6, T7, T8, R>>
        Apply<T1, T2, T3, T4, T5, T6, T7, T8, R>(
            this Valid<Func<T1, T2, T3, T4, T5, T6, T7, T8, R>> source,
            Valid<T1> arg)
        where T1 : notnull
        => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, T3, T4, T5, T6, T7, T8, T9, R>, TInvalid>
        Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R, TInvalid>(
            this Valid<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>> source,
            Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    /// ///
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<Func<T2, T3, T4, T5, T6, T7, T8, T9, R>>
        Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(
            this Valid<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>> source,
            Valid<T1> arg)
        where T1 : notnull
        => source.Map(f => f.CurryFirst()).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Validation<Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, R>, TInvalid>
        Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R, TInvalid>(
            this Valid<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>> source,
            Validation<T1, TInvalid> arg)
        where T1 : notnull
        where TInvalid : notnull => BakeErr<TInvalid>.Valid(source.Value).Apply(arg);

    /// <summary>
    ///     Partially applies the given argument to the inner function.
    /// </summary>
    /// ///
    /// <remarks>
    ///     Can aid type inference in some instances when passing a default valid value.
    /// </remarks>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Valid<Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, R>>
        Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(
            this Valid<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>> source,
            Valid<T1> arg)
        where T1 : notnull
        => source.Map(f => f.CurryFirst()).Apply(arg);


    #endregion
}
