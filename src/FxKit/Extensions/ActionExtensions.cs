namespace FxKit.Extensions;

public static class ActionExtensions
{
    /// <summary>
    ///     Converts an <see cref="Action" /> to a <see cref="Func{Unit}" />.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Func<Unit> ToUnit(this Action action) =>
        () =>
        {
            action();
            return Unit.Default;
        };

    /// <inheritdoc cref="ToUnit" />
    public static Func<T1, Unit> ToUnit<T1>(this Action<T1> action) =>
        arg1 =>
        {
            action(arg1);
            return Unit.Default;
        };

    /// <summary>
    ///     Ensures an Action is not null by falling back to a no-op.
    /// </summary>
    /// <param name="maybeAction"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Action<T> OrNoop<T>(this Action<T>? maybeAction) => maybeAction ?? (_ => { });
}
