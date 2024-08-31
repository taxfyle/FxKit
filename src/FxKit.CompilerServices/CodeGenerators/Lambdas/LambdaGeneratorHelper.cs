using FxKit.CompilerServices.Models;
using FxKit.CompilerServices.Utilities;

namespace FxKit.CompilerServices.CodeGenerators.Lambdas;

/// <summary>
///     A collection of methods to generate into a single file.
/// </summary>
/// <param name="FullyQualifiedContainingTypeMetadataName">
///     The fully qualified metadata name of the type containing the methods.
/// </param>
/// <param name="Methods">
///     The methods to generate.
/// </param>
internal sealed record LambdaGenerationFile(
    string FullyQualifiedContainingTypeMetadataName,
    EquatableArray<LambdaMethodDescriptor> Methods)
{
    /// <summary>
    ///     The namespace of the type containing the methods.
    /// </summary>
    public string Namespace => Methods[0].Namespace;

    /// <summary>
    ///     The type hierarchy of the type containing the methods.
    /// </summary>
    public IEnumerable<TypeHierarchyNode> TypeHierarchy => Methods[0].TypeHierarchy;
}

/// <summary>
///     Describes whether the lambda method is calling a constructor or a method.
/// </summary>
internal enum LambdaTarget
{
    /// <summary>
    ///     The lambda method is calling a constructor.
    /// </summary>
    Constructor,

    /// <summary>
    ///     The lambda method is calling a method.
    /// </summary>
    Method
}

/// <summary>
///     Describes a lambda method.
/// </summary>
/// <param name="FullyQualifiedContainingTypeMetadataName">
///     The fully qualified metadata name of the type containing the method.
/// </param>
/// <param name="Namespace">
///     The namespace of the type containing the method.
/// </param>
/// <param name="TypeHierarchy">
///     The type hierarchy of the type containing the method.
/// </param>
/// <param name="Target">
///     Whether the lambda method is calling a constructor or a method.
/// </param>
/// <param name="TypeOrMethodName">
///     The type or method name of the lambda method being generated.
/// </param>
/// <param name="Parameters">
///     The parameters of the lambda method being generated.
/// </param>
/// <param name="ReturnType">
///     The return type of the lambda method being generated.
/// </param>
internal sealed record LambdaMethodDescriptor(
    string FullyQualifiedContainingTypeMetadataName,
    string Namespace,
    EquatableArray<TypeHierarchyNode> TypeHierarchy,
    LambdaTarget Target,
    string TypeOrMethodName,
    EquatableArray<BasicParameter> Parameters,
    // may be fqn or minimal depending on context
    string ReturnType);
