using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace FxKit.Collections;

/// <summary>
///     A dictionary that preserves insertion order when enumerating.
///     Uses a <see cref="Dictionary{TKey,TValue}" /> under the hood.
/// </summary>
public sealed class InsertionOrderedDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    ///     The underlying dictionary.
    /// </summary>
    private readonly IDictionary<TKey, TValue> _dict;

    /// <summary>
    ///     The dictionary keys in insertion order.
    /// </summary>
    private readonly List<TKey> _keys;

    /// <summary>
    ///     The value collection that will yield items in insertion order.
    /// </summary>
    private ValueCollection? _values;

    /// <summary>
    ///     The keys as read-only.
    /// </summary>
    private ReadOnlyCollection<TKey>? _readOnlyKeys;

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}" />
    [ExcludeFromCodeCoverage]
    public int Count => _dict.Count;

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool IsReadOnly => _dict.IsReadOnly;

    /// <inheritdoc />
    public ICollection<TKey> Keys => _readOnlyKeys ??= _keys.AsReadOnly();

    /// <inheritdoc />
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    /// <inheritdoc />
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    /// <inheritdoc />
    public ICollection<TValue> Values => _values ??= new ValueCollection(this);

    /// <summary>
    ///     Initializes a new instance of the <see cref="InsertionOrderedDictionary{TKey,TValue}" /> class.
    /// </summary>
    public InsertionOrderedDictionary()
        : this(2)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InsertionOrderedDictionary{TKey,TValue}" /> class
    ///     with the specified initial capacity.
    /// </summary>
    public InsertionOrderedDictionary(int capacity)
    {
        _dict = new Dictionary<TKey, TValue>(capacity);
        _keys = new List<TKey>(capacity);
    }

    /// <inheritdoc />
    [MustDisposeResource]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return EnumerateOrdered().GetEnumerator();

        IEnumerable<KeyValuePair<TKey, TValue>> EnumerateOrdered()
        {
            foreach (var key in _keys)
            {
                yield return new KeyValuePair<TKey, TValue>(key, _dict[key]);
            }
        }
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    /// <inheritdoc />
    public void Clear()
    {
        _dict.Clear();
        _keys.Clear();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Contains(KeyValuePair<TKey, TValue> item) => _dict.Contains(item);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
        _dict.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        _dict.Add(key, value);
        _keys.Add(key);
    }

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}" />
    [ExcludeFromCodeCoverage]
    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if (_dict.Remove(key))
        {
            _keys.Remove(key);
            return true;
        }

        return false;
    }

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}" />
    [ExcludeFromCodeCoverage]
    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value) =>
        // Disabling "Parameter 'value' must have a non-null value when exiting with 'true'." because
        // it's not being picked up correctly by the compiler.
#pragma warning disable CS8762
        _dict.TryGetValue(key, out value);
#pragma warning restore CS8762

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}" />
    public TValue this[TKey key]
    {
        get => _dict[key];
        set
        {
            if (!_dict.ContainsKey(key))
            {
                _keys.Add(key);
            }

            _dict[key] = value;
        }
    }

    /// <summary>
    ///     Collection that, when enumerated, returns the values in insertion order.
    /// </summary>
    private class ValueCollection : ICollection<TValue>
    {
        private readonly InsertionOrderedDictionary<TKey, TValue> _dict;
        private readonly ICollection<TValue>                      _collection;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public int Count => _collection.Count;

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public bool IsReadOnly => _collection.IsReadOnly;

        /// <summary>
        ///     Constructor.
        /// </summary>
        public ValueCollection(InsertionOrderedDictionary<TKey, TValue> dict)
        {
            _dict = dict;
            _collection = dict._dict.Values;
        }

        /// <inheritdoc />
        [MustDisposeResource]
        public IEnumerator<TValue> GetEnumerator()
        {
            return EnumerateOrdered().GetEnumerator();

            IEnumerable<TValue> EnumerateOrdered()
            {
                foreach (var key in _dict._keys)
                {
                    yield return _dict[key];
                }
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [MustDisposeResource]
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_collection).GetEnumerator();

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public void Add(TValue item) => _collection.Add(item);

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public void Clear() => _collection.Clear();

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public bool Contains(TValue item) => _collection.Contains(item);

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public void CopyTo(TValue[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public bool Remove(TValue item) => _collection.Remove(item);
    }
}
