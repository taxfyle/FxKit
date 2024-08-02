namespace FxKit.Extensions;

/// <summary>
///     Extensions for <c>Func</c>.
/// </summary>
public static class FuncExtensions
{
    #region Composition

    /// <summary>
    ///     Composes the source function <c>f</c> with a function <c>g</c> such that a function
    ///     <c>h(x) = f(g(x))</c> is created. This composition extension method is equivalent
    ///     to <c>.</c> from Haskell and <c>&lt;&lt;</c> from F#.
    /// </summary>
    /// <returns>The composition of <paramref name="f" /> and <paramref name="g" />.</returns>
    public static Func<T, B> Compose<T, U, B>(this Func<U, B> f, Func<T, U> g) => t => f(g(t));

    #endregion

    #region Partial Application

    // @formatter:off

    /// <summary>
    ///     Partially applies <c>T1</c> onto <c>source</c>.
    /// </summary>
    public static Func<T2, R> Apply<T1, T2, R>(
        this Func<T1, T2, R> source,
        T1 t1) => t2 => source(t1, t2);

    /// <summary>
    ///     Partially applies <c>T1</c> onto <c>source</c>.
    /// </summary>
    public static Func<T2, T3, R> Apply<T1, T2, T3, R>(
        this Func<T1, T2, T3, R> source,
        T1 t1) => (t2, t3) => source(t1, t2, t3);

    /// <summary>
    ///     Partially applies <c>T1</c> onto <c>source</c>.
    /// </summary>
    public static Func<T2, T3, T4, R> Apply<T1, T2, T3, T4, R>(
        this Func<T1, T2, T3, T4, R> source,
        T1 t1) => (t2, t3, t4) => source(t1, t2, t3, t4);

    /// <summary>
    ///     Partially applies <c>T1</c> onto <c>source</c>.
    /// </summary>
    public static Func<T2, T3, T4, T5, R> Apply<T1, T2, T3, T4, T5, R>(
        this Func<T1, T2, T3, T4, T5, R> source,
        T1 t1) => (t2, t3, t4, t5) => source(t1, t2, t3, t4, t5);

    /// <summary>
    ///     Partially applies <c>T1</c> onto <c>source</c>.
    /// </summary>
    public static Func<T2, T3, T4, T5, T6, R> Apply<T1, T2, T3, T4, T5, T6, R>(
        this Func<T1, T2, T3, T4, T5, T6, R> source, T1 t1)
        => (t2, t3, t4, t5, t6) => source(t1, t2, t3, t4, t5, t6);

    /// <summary>
    ///     Partially applies <c>T1</c> onto <c>source</c>.
    /// </summary>
    public static Func<T2, T3, T4, T5, T6, T7, R> Apply<T1, T2, T3, T4, T5, T6, T7, R>(
        this Func<T1, T2, T3, T4, T5, T6, T7, R> source,
        T1 t1) => (t2, t3, t4, t5, t6, t7) => source(t1, t2, t3, t4, t5, t6, t7);

    /// <summary>
    ///     Partially applies <c>T1</c> onto <c>source</c>.
    /// </summary>
    public static Func<T2, T3, T4, T5, T6, T7, T8, R> Apply<T1, T2, T3, T4, T5, T6, T7, T8, R>(
        this Func<T1, T2, T3, T4, T5, T6, T7, T8, R> source,
        T1 t1) => (t2, t3, t4, t5, t6, t7, t8) => source(t1, t2, t3, t4, t5, t6, t7, t8);

    /// <summary>
    ///     Partially applies <c>T1</c> onto <c>source</c>.
    /// </summary>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, R> Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(
        this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R> source,
        T1 t1) => (t2, t3, t4, t5, t6, t7, t8, t9) => source(t1, t2, t3, t4, t5, t6, t7, t8, t9);

    /// <summary>
    ///     Partially applies <c>T1</c> onto <c>source</c>.
    /// </summary>
    public static Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, R> Apply<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(
        this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R> source,
        T1 t1) => (t2, t3, t4, t5, t6, t7, t8, t9, t10) => source(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

    // @formatter:on

    #endregion

    #region Currying

    // @formatter:off

    #region General Currying

    /// <summary>
    ///     Curries the function.
    /// </summary>
    public static Func<T1, Func<T2, R>> Curry<T1, T2, R>(this Func<T1, T2, R> func)
        => t1 => t2 => func(t1, t2);

    /// <summary>
    ///     Curries the function.
    /// </summary>
    public static Func<T1, Func<T2, Func<T3, R>>> Curry<T1, T2, T3, R>(this Func<T1, T2, T3, R> func)
        => t1 => t2 => t3 => func(t1, t2, t3);

    /// <summary>
    ///     Curries the function.
    /// </summary>
    public static Func<T1, Func<T2, Func<T3, Func<T4, R>>>> Curry<T1, T2, T3, T4, R>(
        this Func<T1, T2, T3, T4, R> source)
        => t1 => t2 => t3 => t4 => source(t1, t2, t3, t4);

    /// <summary>
    ///     Curries the function.
    /// </summary>
    public static Func<T1, Func<T2, Func<T3, Func<T4, Func<T5, R>>>>> Curry<T1, T2, T3, T4, T5, R>
        (this Func<T1, T2, T3, T4, T5, R> source) => t1 => t2 => t3 => t4 => t5 => source(t1, t2, t3, t4, t5);

    /// <summary>
    ///     Curries the function.
    /// </summary>
    public static Func<T1, Func<T2, Func<T3, Func<T4, Func<T5, Func<T6, R>>>>>> Curry<T1, T2, T3, T4, T5, T6, R>
        (this Func<T1, T2, T3, T4, T5, T6, R> source) => t1 => t2 => t3 => t4 => t5 => t6 => source(t1, t2, t3, t4, t5, t6);

    /// <summary>
    ///     Curries the function.
    /// </summary>
    public static Func<T1, Func<T2, Func<T3, Func<T4, Func<T5, Func<T6, Func<T7, R>>>>>>> Curry<T1, T2, T3, T4, T5, T6, T7, R>
        (this Func<T1, T2, T3, T4, T5, T6, T7, R> source) => t1 => t2 => t3 => t4 => t5 => t6 => t7 => source(t1, t2, t3, t4, t5, t6, t7);

    /// <summary>
    ///     Curries the function.
    /// </summary>
    public static Func<T1, Func<T2, Func<T3, Func<T4, Func<T5, Func<T6, Func<T7, Func<T8, R>>>>>>>> Curry<T1, T2, T3, T4, T5, T6, T7, T8, R>
        (this Func<T1, T2, T3, T4, T5, T6, T7, T8, R> source) => t1 => t2 => t3 => t4 => t5 => t6 => t7 => t8 => source(t1, t2, t3, t4, t5, t6, t7, t8);

    /// <summary>
    ///     Curries the function.
    /// </summary>
    public static Func<T1, Func<T2, Func<T3, Func<T4, Func<T5, Func<T6, Func<T7, Func<T8, Func<T9, R>>>>>>>>> Curry<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>
        (this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R> source) => t1 => t2 => t3 => t4 => t5 => t6 => t7 => t8 => t9 => source(t1, t2, t3, t4, t5, t6, t7, t8, t9);

    /// <summary>
    ///     Curries the function.
    /// </summary>
    public static Func<T1, Func<T2, Func<T3, Func<T4, Func<T5, Func<T6, Func<T7, Func<T8, Func<T9, Func<T10, R> >>>>>>>>> Curry<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>
        (this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R> source) => t1 => t2 => t3 => t4 => t5 => t6 => t7 => t8 => t9 => t10 => source(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

    #endregion

    #region Lazy Currying

    /// <summary>
    ///     Curries the first function, such that a binary function is the result.
    /// </summary>
    public static Func<T1, Func<T2, T3, R>> CurryFirst<T1, T2, T3, R>
     (this Func<T1, T2, T3, R> source) => t1 => (t2, t3) => source(t1, t2, t3);

    /// <summary>
    ///     Curries the first function, such that a trinary function is the result.
    /// </summary>
    public static Func<T1, Func<T2, T3, T4, R>> CurryFirst<T1, T2, T3, T4, R>
     (this Func<T1, T2, T3, T4, R> source) => t1 => (t2, t3, t4) => source(t1, t2, t3, t4);

    /// <summary>
    ///     Curries the first function, such that a 4-ary function is the result.
    /// </summary>
    public static Func<T1, Func<T2, T3, T4, T5, R>> CurryFirst<T1, T2, T3, T4, T5, R>
     (this Func<T1, T2, T3, T4, T5, R> source) => t1 => (t2, t3, t4, t5) => source(t1, t2, t3, t4, t5);

    /// <summary>
    ///     Curries the first function, such that a 5-ary function is the result.
    /// </summary>
    public static Func<T1, Func<T2, T3, T4, T5, T6, R>> CurryFirst<T1, T2, T3, T4, T5, T6, R>
     (this Func<T1, T2, T3, T4, T5, T6, R> source) => t1 => (t2, t3, t4, t5, t6) => source(t1, t2, t3, t4, t5, t6);

    /// <summary>
    ///     Curries the first function, such that a 6-ary function is the result.
    /// </summary>
    public static Func<T1, Func<T2, T3, T4, T5, T6, T7, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, R>
     (this Func<T1, T2, T3, T4, T5, T6, T7, R> source) => t1 => (t2, t3, t4, t5, t6, t7) => source(t1, t2, t3, t4, t5, t6, t7);

    /// <summary>
    ///     Curries the first function, such that a 7-ary function is the result.
    /// </summary>
    public static Func<T1, Func<T2, T3, T4, T5, T6, T7, T8, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, T8, R>
     (this Func<T1, T2, T3, T4, T5, T6, T7, T8, R> source) => t1 => (t2, t3, t4, t5, t6, t7, t8) => source(t1, t2, t3, t4, t5, t6, t7, t8);

    /// <summary>
    ///     Curries the first function, such that a 8-ary function is the result.
    /// </summary>
    public static Func<T1, Func<T2, T3, T4, T5, T6, T7, T8, T9, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>
     (this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, R> source) => t1 => (t2, t3, t4, t5, t6, t7, t8, t9) => source(t1, t2, t3, t4, t5, t6, t7, t8, t9);

    /// <summary>
    ///     Curries the first function, such that a 9-ary function is the result.
    /// </summary>
    public static Func<T1, Func<T2, T3, T4, T5, T6, T7, T8, T9, T10, R>> CurryFirst<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>
     (this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R> source) => t1 => (t2, t3, t4, t5, t6, t7, t8, t9, t10) => source(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);

    #endregion

    //@formatter:on

    #endregion
}
