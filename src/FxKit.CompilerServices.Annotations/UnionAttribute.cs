namespace FxKit.CompilerServices;

/// <summary>
///     Marks the interface as a Discriminated Union and will have code generated for each of its'
///     nested partial records.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class UnionAttribute : Attribute;
