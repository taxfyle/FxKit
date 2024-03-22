namespace FxKit;

public static partial class Result
{
    #region LINQ

    /// <summary>
    ///     LINQ Extension Method for `MapAsyncT`.
    /// </summary>
    public static Task<Result<TNewOk, TErr>> Select<TOk, TNewOk, TErr>(
        this Task<Result<TOk, TErr>> source,
        Func<TOk, Task<TNewOk>> selector) // Notice: Maps to Task<U>
        where TOk : notnull
        where TNewOk : notnull
        where TErr : notnull
        => source.MapAsyncT(selector);

    #endregion
}
