namespace FxKit;

public static partial class Validation
{
    /// <summary>
    /// Calls the <paramref name="callback"/> if the <see cref="Validation{TValid,TInvalid}"/> is
    /// in a Valid state.
    /// </summary>
    /// <param name="source">The source validation.</param>
    /// <param name="callback">The callback to execute if the validation is valid.</param>
    /// <typeparam name="TValid">The type of the valid value.</typeparam>
    /// <typeparam name="TInvalid">The type of the invalid value.</typeparam>
    /// <returns>The original validation.</returns>
    public static Validation<TValid, TInvalid> Do<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        Action<TValid> callback)
        where TValid : notnull
        where TInvalid : notnull
    {
        if (source.TryGet(out var valid, out _))
        {
            callback(valid);
        }

        return source;
    }

    /// <summary>
    /// Calls the <paramref name="callback"/> if the <see cref="Validation{TValid,TInvalid}"/> is in an Invalid state.
    /// </summary>
    /// <param name="source">The source validation.</param>
    /// <param name="callback">The callback to execute if the validation is invalid.</param>
    /// <typeparam name="TValid">The type of the valid value.</typeparam>
    /// <typeparam name="TInvalid">The type of the invalid value.</typeparam>
    /// <returns>The original validation.</returns>
    public static Validation<TValid, TInvalid> DoInvalid<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        Action<IEnumerable<TInvalid>> callback)
        where TValid : notnull
        where TInvalid : notnull
    {
        if (!source.TryGet(out _, out var invalid))
        {
            callback(invalid);
        }

        return source;
    }

    /// <summary>
    /// Like <see cref="Do{TValid,TInvalid}"/> but awaits the <paramref name="callback"/> and turns the overall result into a task.
    /// </summary>
    /// <param name="source">The source validation.</param>
    /// <param name="callback">The callback to execute if the validation is valid.</param>
    /// <typeparam name="TValid">The type of the valid value.</typeparam>
    /// <typeparam name="TInvalid">The type of the invalid value.</typeparam>
    /// <returns>The original validation.</returns>
    public static async Task<Validation<TValid, TInvalid>> DoAsync<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        Func<TValid, Task> callback)
        where TValid : notnull
        where TInvalid : notnull
    {
        if (source.TryGet(out var valid, out _))
        {
            await callback(valid);
        }

        return source;
    }

    /// <summary>
    /// Like <see cref="DoInvalid{TValid,TInvalid}"/> but awaits the <paramref name="callback"/> and turns the overall result into a task.
    /// </summary>
    /// <param name="source">The source validation.</param>
    /// <param name="callback">The callback to execute if the validation is invalid.</param>
    /// <typeparam name="TValid">The type of the valid value.</typeparam>
    /// <typeparam name="TInvalid">The type of the invalid value.</typeparam>
    /// <returns>The original validation.</returns>
    public static async Task<Validation<TValid, TInvalid>> DoInvalidAsync<TValid, TInvalid>(
        this Validation<TValid, TInvalid> source,
        Func<IEnumerable<TInvalid>, Task> callback)
        where TValid : notnull
        where TInvalid : notnull
    {
        if (!source.TryGet(out _, out var invalid))
        {
            await callback(invalid);
        }

        return source;
    }
}
