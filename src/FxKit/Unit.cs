using System.Diagnostics;

namespace FxKit;

/// <summary>
///     A unit type is a type that allows only one value, but no information.
/// </summary>
[DebuggerDisplay("()")]
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    ///     The unit value.
    /// </summary>
    public static readonly Unit Default = new();

    /// <inheritdoc />
    public override int GetHashCode() =>
        0;

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Unit;

    /// <inheritdoc />
    public override string ToString() =>
        "()";

    /// <inheritdoc />
    public bool Equals(Unit other) =>
        true;

    /// <summary>
    ///     Checks for equality.
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator ==(Unit lhs, Unit rhs) =>
        true;

    /// <summary>
    ///     Checks for inequality.
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator !=(Unit lhs, Unit rhs) =>
        false;
}

public static partial class Prelude
{
    /// <summary>
    ///     Returns the unit value.
    /// </summary>
    public static Unit Unit() => FxKit.Unit.Default;

    /// <summary>
    ///     Ignores the input argument and returns <see cref="Unit" />.
    /// </summary>
    /// <param name="_"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Unit Ignore<T>(T _) => FxKit.Unit.Default;
}
