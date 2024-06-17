namespace FxKit.CompilerServices;

/// <summary>
///     Indicates that the assembly may contain functor definitions and/or behavior.
///     Used by the transformer generator to quickly filter referenced assemblies
///     for inspection.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class ContainsFunctorsAttribute : Attribute;
