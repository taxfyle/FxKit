namespace FxKit.CompilerServices;

/// <summary>
///     Indicates that the annotated type is a functor and can be used as the outer
///     container of a stack.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class FunctorAttribute : Attribute;
