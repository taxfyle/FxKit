using FxKit.CompilerServices;

namespace FxKit;

public partial class Option
{
    /// <summary>
    ///     Runs <see cref="action" /> if the <paramref name="source" /> is Some.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [GenerateTransformer]
    public static Option<T> Do<T>(this Option<T> source, Action<T> action)
        where T : notnull
    {
        if (source.TryGet(out var value))
        {
            action(value);
        }

        return source;
    }
}
