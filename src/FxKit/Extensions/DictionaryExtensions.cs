namespace FxKit.Extensions;

/// <summary>
///     Extensions for dictionaries.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    ///     Gets an item if it exists.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="key"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Option<T> Get<TKey, T>(
        this IReadOnlyDictionary<TKey, T> source,
        TKey key)
        where T : notnull =>
        source.TryGetValue(key, out var value) ? Some(value) : None;

    /// <summary>
    ///     Gets the first item that matches the given predicate.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="predicate"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Option<T> Find<TKey, T>(
        this IReadOnlyDictionary<TKey, T> source,
        Func<T, bool> predicate)
        where T : class =>
        source.Values.FirstOrDefault(predicate);

    /// <summary>
    ///     Gets multiple items from a dictionary.
    ///     The result only contains found items, and this method won't
    ///     throw if the keys are not in the dictionary.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="keys"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetMultiple<TKey, T>(
        this IReadOnlyDictionary<TKey, T> source,
        IEnumerable<TKey> keys)
        where T : notnull
    {
        foreach (var key in keys)
        {
            if (source.TryGetValue(key, out var value))
            {
                yield return value;
            }
        }
    }
}
