namespace FxKit.CompilerServices;

/// <summary>
///     Indicates that the annotated method will have transformers generated
///     for every permutation of container stack. These transformers will be
///     named the same as the marked method, but will have a `T` suffix.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class GenerateTransformerAttribute : Attribute;
