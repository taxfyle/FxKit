using System.Diagnostics;

namespace FxKit;

/// <summary>
///     Simple wrapper for value objects.
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("{ToString()}")]
public abstract class ValueOf<T> : IEquatable<ValueOf<T>>, IEquatable<T>
    where T : notnull
{
    /// <summary>
    ///     The underlying value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    ///     Internal constructor.
    /// </summary>
    /// <param name="value"></param>
    protected ValueOf(T value)
    {
        Value = value;
    }

    /// <summary>
    ///     Checks for equality on the inner value.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(T? other) =>
        other is not null && EqualityComparer<T>.Default.Equals(Value, other);

    /// <inheritdoc />
    public bool Equals(ValueOf<T>? other)
    {
        if (ReferenceEquals(objA: null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    /// <inheritdoc />
    public override string? ToString() => Value.ToString() ?? base.ToString();

    /// <inheritdoc />
    public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(objA: null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is T typed)
        {
            return Equals(typed);
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ValueOf<T>)obj);
    }

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(ValueOf<T>? left, ValueOf<T>? right) => Equals(left, right);

    /// <summary>
    ///     Checks for inequality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(ValueOf<T>? left, ValueOf<T>? right) => !Equals(left, right);

    /// <summary>
    ///     Implicitly converts the value to its' underlying type.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static implicit operator T(ValueOf<T> source) => source.Value;
}
