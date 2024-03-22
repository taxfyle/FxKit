using System.Collections;

namespace FxKit.Collections;

public static class ListOf
{
    /// <summary>
    ///     Creates a read-only list with a single item.
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IReadOnlyList<T> One<T>(T item) => new SingleItemList<T>(item);

    /// <summary>
    ///     Creates a read-only list with the provided elements.
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IReadOnlyList<T> Many<T>(params T[] items) => items.ToList();

    /// <summary>
    ///     A list that only contains a single item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private readonly struct SingleItemList<T> : IReadOnlyList<T>
    {
        /// <summary>
        ///     The item.
        /// </summary>
        private readonly T _item;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="item"></param>
        public SingleItemList(T item)
        {
            _item = item;
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            yield return _item;
        }

        /// <inheritdoc />
        public override string ToString() => $"{{ {_item} }}";

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => 1;

        /// <inheritdoc />
        public T this[int index] => index == 0
            ? _item
            : throw new IndexOutOfRangeException("Only index 0 can be accessed in a single item list.");
    }
}
