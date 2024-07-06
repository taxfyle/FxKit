namespace FxKit;

public static partial class Result
{
    #region LINQ

    /// <summary>
    ///     LINQ Extension Method for `MapAsyncT`.
    /// </summary>
    public static async Task<Result<TNewOk, TErr>> Select<TOk, TNewOk, TErr>(
        this Task<Result<TOk, TErr>> source,
        Func<TOk, Task<TNewOk>> selector) // Notice: Maps to Task<U>
        where TOk : notnull
        where TNewOk : notnull
        where TErr : notnull
        => (await source.ConfigureAwait(false)).TryGet(out var ok, out var err)
            ? await selector(ok).ConfigureAwait(false)
            : Result<TNewOk, TErr>.Err(err);

    #endregion
}
