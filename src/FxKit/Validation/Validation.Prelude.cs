namespace FxKit;

public static partial class Prelude
{
    /// <summary>
    ///     Creates a <see cref="Validation.Valid{T}" />.
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Validation.Valid<T> Valid<T>(T value)
        where T : notnull
        => new(value);

    /// <summary>
    ///     Creates a <see cref="Validation.Invalid{E}" />.
    /// </summary>
    /// <param name="error"></param>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static Validation.Invalid<E> Invalid<E>(E error)
        where E : notnull
        => new(error);

    /// <summary>
    ///     Creates a <see cref="Validation.Invalid{E}" />.
    /// </summary>
    /// <param name="errors"></param>
    /// <typeparam name="E"></typeparam>
    /// <returns></returns>
    public static Validation.Invalid<E> Invalid<E>(IEnumerable<E> errors)
        where E : notnull
        => new(errors);
}
