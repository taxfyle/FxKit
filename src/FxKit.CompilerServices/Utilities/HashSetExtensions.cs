namespace FxKit.CompilerServices.Utilities;

public static class HashSetExtensions
{
    /// <summary>
    ///     Adds a range of items to a <see cref="HashSet{T}" />.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="rest"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static HashSet<T> AddRange<T>(this HashSet<T> source, IEnumerable<T> rest)
    {
        foreach (var element in rest)
        {
            source.Add(element);
        }

        return source;
    }
}
