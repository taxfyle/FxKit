using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices;

// ReSharper disable InconsistentNaming
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace FxKit;

/// <summary>
///     Marker interface for an <see cref="Option{T}" />.
///     Should not be depended upon.
/// </summary>
/// <remarks>
///     This is needed in order to guarantee safe equality checks between the various types that
///     represent an <see cref="Option{T}" />.
/// </remarks>
internal interface IOption
{
    bool IsSome { get; }
}

/// <summary>
///     Represents an optional value
/// </summary>
/// <typeparam name="T">The type of the value held within; cannot be null.</typeparam>
[DebuggerDisplay("{ToString()}")]
[DebuggerStepThrough]
[Functor]
public readonly struct Option<T>
    : IOption,
        IEquatable<Option<T>>,
        IEquatable<Option.None>
    where T : notnull
{
    /// <summary>
    ///     Used to prevent hash code collisions.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static readonly object hashcodeMarker = new();

    /// <summary>
    ///     The value held, if any.
    /// </summary>
    private readonly T _value;

    /// <summary>
    ///     Whether the <see cref="Option{T}" /> holds a value.
    /// </summary>
    public bool IsSome { get; }

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSome"></param>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Option(T value, bool isSome)
    {
        _value = value;
        IsSome = isSome;
    }

    /// <summary>
    ///     Matches on the value held within the <see cref="Option{T}" />.
    /// </summary>
    /// <param name="Some"></param>
    /// <param name="None"></param>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public R Match<R>(
        Func<T, R> Some,
        // ReSharper disable once ParameterHidesMember
        Func<R> None) => IsSome ? Some(_value) : None();

    /// <summary>
    ///     Returns <c>true</c> if the option holds a value and sets it in
    ///     the <paramref name="value" />; returns <c>false</c> otherwise.
    /// </summary>
    /// <remarks>
    ///     This is Jamie's least favorite method of all time.
    /// </remarks>
    /// <param name="value"></param>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet([NotNullWhen(true)] out T? value)
    {
        value = _value;
        return IsSome;
    }

    /// <inheritdoc />
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => Match(
        Some: v => $"Some({v})",
        None: () => "None");

    /// <summary>
    ///     If source is in a Some state, and it's of type <typeparamref name="U"/>,
    ///     returns `Some` of type <typeparamref name="U"/>. Otherwise, returns None.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<U> OfType<U>()
        where U : notnull, T =>
        TryGet(out var some) && some is U cast ? Option<U>.Some(cast) : Option<U>.None;

    /// <summary>
    ///     Returns an <see cref="Option{T}" /> in the Some variant, holding a value.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    [DebuggerHidden]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(
                nameof(value),
                "A Some cannot be constructed with a null value");
        }

        return new Option<T>(value, isSome: true);
    }

    /// <summary>
    ///     Returns an <see cref="Option{T}" /> in the None variant.
    /// </summary>
    /// <returns></returns>
    [DebuggerHidden]
    public static Option<T> None => new(default!, isSome: false);

    /// <summary>
    ///     Checks whether the options are equal.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Option<T> other) =>
        IsSome == other.IsSome && EqualityComparer<T>.Default.Equals(_value, other._value);

    /// <summary>
    ///     Checks whether the option is equal to <see cref="None" />.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(Option.None other) =>
        !IsSome;

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj switch
        {
            Option<T> other  => Equals(other),
            Option.None none => Equals(none),
            _                => false
        };

    /// <inheritdoc />
    public override int GetHashCode() =>
        IsSome
            ? HashCode.Combine(hashcodeMarker, _value)
            : Option.None.GetNoneHashCode();

    /// <summary>
    ///     Implicitly converts an <see cref="Option.None" /> to an <see cref="Option{T}" />
    /// </summary>
    /// <returns></returns>
    public static implicit operator Option<T>(Option.None _) => None;

    /// <summary>
    ///     Implicitly converts a <typeparamref name="T" /> to a <see cref="Option{T}" />.
    ///     If
    ///     <param name="value"></param>
    ///     is <c>null</c>, returns None; otherwise Some.
    /// </summary>
    /// <returns></returns>
    public static implicit operator Option<T>(T? value) => Optional(value);

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);

    /// <summary>
    ///     Checks for inequality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);
}

/// <summary>
///     Option extensions.
/// </summary>
[DebuggerDisplay("{ToString()}")]
[DebuggerStepThrough]
public static partial class Option
{
    /// <summary>
    ///     Represents the None variant.
    /// </summary>
    [DebuggerStepThrough]
    public readonly struct None
    {
        /// <summary>
        ///     Used for generating a hash code.
        /// </summary>
        private static readonly object hashcodeMarker = new();

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj switch
        {
            None                      => true,
            IOption { IsSome: false } => true,
            _                         => false
        };

        /// <inheritdoc />
        public override int GetHashCode() => GetNoneHashCode();

        /// <summary>
        ///     Gets the hash code for the None variant.
        /// </summary>
        /// <returns></returns>
        internal static int GetNoneHashCode() => hashcodeMarker.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => "None";

        /// <summary>
        ///     Checks for equality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(None left, None right) => true;

        /// <summary>
        ///     Checks for inequality.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(None left, None right) => false;
    }
}
