using System.Collections.Immutable;

namespace FxKit.CompilerServices.Utilities;

/// <summary>
///     Compares two sets using set equality.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class ImmutableHashSetEqualityComparer<T> : IEqualityComparer<ImmutableHashSet<T>>
{
    /// <summary>
    ///     Default instance of the <see cref="ImmutableHashSetEqualityComparer{T}" />.
    /// </summary>
    public static readonly ImmutableHashSetEqualityComparer<T> Default = new();

    /// <inheritdoc />
    public bool Equals(ImmutableHashSet<T> x, ImmutableHashSet<T> y) => x.SetEquals(y);

    /// <inheritdoc />
    public int GetHashCode(ImmutableHashSet<T> obj) => throw new NotImplementedException();
}
