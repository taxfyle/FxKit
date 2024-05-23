using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

// ReSharper disable InconsistentNaming

namespace FxKit;

/// <summary>
///     A structure that holds either an "Ok" value or an "Error" value.
/// </summary>
/// <remarks>
///     An effort has been made to make equality checks "Just Work", however there are
///     cases when checking equality for base vs derived classes held in the result
///     reporting the wrong values.
/// </remarks>
/// <typeparam name="TOk"></typeparam>
/// <typeparam name="TErr"></typeparam>
[DebuggerStepThrough]
[DebuggerDisplay("{ToString()}")]
[Functor]
public readonly struct Result<TOk, TErr> : IEquatable<Result<TOk, TErr>>
    where TOk : notnull
    where TErr : notnull
{
    /// <summary>
    ///     Used to generate hash codes.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static readonly object OkHashcodeMarker = new();

    /// <summary>
    ///     Used to generate hash codes.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static readonly object ErrHashcodeMarker = new();

    /// <summary>
    ///     If the result is in the Ok state, holds the Ok value.
    /// </summary>
    private readonly TOk _ok;

    /// <summary>
    ///     If the result is in the Error state, holds the Error value.
    /// </summary>
    private readonly TErr _error;

    /// <summary>
    ///     Internal constructor.
    /// </summary>
    /// <param name="ok"></param>
    /// <param name="error"></param>
    /// <param name="isOk"></param>
    private Result(TOk ok, TErr error, bool isOk)
    {
        _ok = ok;
        _error = error;
        IsOk = isOk;
    }

    /// <summary>
    ///     Whether this <see cref="Result{T,E}" /> is an Ok.
    /// </summary>
    public bool IsOk { get; }

    /// <summary>
    ///     Matches on the result, calling either <paramref name="Ok" /> or <paramref name="Err" />
    ///     depending on the state of the result.
    /// </summary>
    /// <param name="Ok"></param>
    /// <param name="Err"></param>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public R Match<R>(Func<TOk, R> Ok, Func<TErr, R> Err) =>
        IsOk
            ? Ok(_ok)
            : Err(_error);

    /// <summary>
    ///     Asynchronously matches on the result, calling either <paramref name="Ok" /> or <paramref name="Err" />
    ///     depending on the state of the result.
    /// </summary>
    /// <param name="Ok"></param>
    /// <param name="Err"></param>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<R> MatchAsync<R>(Func<TOk, Task<R>> Ok, Func<TErr, Task<R>> Err) =>
        IsOk
            ? Ok(_ok)
            : Err(_error);

    /// <inheritdoc cref="object.Equals(object?)" />
    public bool Equals(Result<TOk, TErr> other)
    {
        if (IsOk != other.IsOk)
        {
            return false;
        }

        if (IsOk)
        {
            return EqualityComparer<TOk>.Default.Equals(_ok, other._ok);
        }

        return EqualityComparer<TErr>.Default.Equals(_error, other._error);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj switch
    {
        Result<TOk, TErr> result => Equals(result),
        _                        => false
    };

    /// <inheritdoc />
    public override int GetHashCode() => IsOk
        ? HashCode.Combine(OkHashcodeMarker, _ok)
        : HashCode.Combine(ErrHashcodeMarker, _error);

    /// <inheritdoc />
    public override string ToString() => IsOk ? $"Ok({_ok})" : $"Err({_error})";

    /// <summary>
    ///     Returns <c>true</c> if the result is in an Ok state and assigns
    ///     the <paramref name="ok" /> parameter; otherwise returns <c>false</c>
    ///     and assigns the <paramref name="error" /> parameter.
    /// </summary>
    /// <remarks>
    ///     This is Jamie's least favorite method of all time.
    /// </remarks>
    /// <param name="ok"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet([NotNullWhen(true)] out TOk? ok, [NotNullWhen(false)] out TErr? error)
    {
        ok = _ok;
        error = _error;
        return IsOk;
    }

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Result<TOk, TErr> left, Result<TOk, TErr> right) =>
        left.Equals(right);

    /// <summary>
    ///     Checks for inequality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Result<TOk, TErr> left, Result<TOk, TErr> right) => !(left == right);

    /// <summary>
    ///     Creates a <see cref="Result{T,E}" /> in the Ok state.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TOk, TErr> Ok(TOk value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "An Ok cannot be constructed with a null value");
        }

        return new Result<TOk, TErr>(value, default!, isOk: true);
    }

    /// <summary>
    ///     Creates a <see cref="Result{T,E}" /> in the Err state.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TOk, TErr> Err(TErr value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "An Err cannot be constructed with a null value");
        }

        return new Result<TOk, TErr>(default!, value, isOk: false);
    }

    /// <summary>
    ///     Implicitly converts to a result.
    /// </summary>
    /// <param name="ok"></param>
    /// <returns></returns>
    public static implicit operator Result<TOk, TErr>(TOk ok) =>
        new(ok, default!, isOk: true);

    /// <summary>
    ///     Implicitly converts to a result.
    /// </summary>
    /// <param name="err"></param>
    /// <returns></returns>
    public static implicit operator Result<TOk, TErr>(TErr err) =>
        new(default!, err, isOk: false);
}

/// <summary>
///     Result variants and extensions.
/// </summary>
[DebuggerStepThrough]
public static partial class Result
{
    /// <summary>
    ///     Returns a <see cref="Result{T,E}" /> that will be <c>Ok</c>
    ///     if the <paramref name="value" /> is <c>true</c>; otherwise returns
    ///     <c>Err</c>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Unit, Unit> RequireTrue(bool? value) =>
        value is true ? Ok<Unit, Unit>(Unit()) : Err<Unit, Unit>(Unit());

    /// <summary>
    ///     Returns a <see cref="Result{T,E}" /> that will be <c>Ok</c>
    ///     if the <paramref name="value" /> is <c>true</c>; otherwise returns
    ///     <c>Err</c>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<Unit, Unit> RequireTrue(bool value) =>
        value ? Ok<Unit, Unit>(Unit()) : Err<Unit, Unit>(Unit());
}
