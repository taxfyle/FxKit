namespace FxKit.CompilerServices;

/// <summary>
///     Generate lambdas (Func) alternatives for static methods, or for public constructors.
///     For methods, the method will be suffixed with the lambda symbol "λ". If a constructor,
///     the name will be "λ".
/// </summary>
/// <remarks>
///     This is useful when you want to apply values or pass it to a method accepting a function.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Struct)]
public class LambdaAttribute : Attribute;
