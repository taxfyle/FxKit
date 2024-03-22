#pragma warning disable RS2008

using Microsoft.CodeAnalysis;

namespace FxKit.CompilerServices;

/// <summary>
///     Descriptors for reporting diagnostics.
/// </summary>
public static class DiagnosticsDescriptors
{
    #region Categories

    /// <summary>
    ///     Diagnostics categories.
    /// </summary>
    private static class Categories
    {
        public const string GeneralCodeGeneration = "FxKitCodeGeneration";
    }

    #endregion

    #region Diagnostics

    /// <summary>
    ///     Type must be partial.
    /// </summary>
    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "FXKIT0001",
        title: "Type must be partial",
        messageFormat: "The type '{0}' must be partial in order to use it with '{1}'",
        category: Categories.GeneralCodeGeneration,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    ///     Missing union constituents.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingUnionConstituents = new(
        id: "FXKIT0002",
        title: "Missing union constituents",
        messageFormat:
        "The type '{0}' is marked as [Union] and should declare at least one nested 'partial record'",
        category: Categories.GeneralCodeGeneration,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    ///     Missing union constituents.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidUnionConstituentDeclaration = new(
        id: "FXKIT0003",
        title: "Invalid union constituent declaration",
        messageFormat: "Use 'partial record {0}' to declare a constituent of union type '{1}'",
        category: Categories.GeneralCodeGeneration,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    ///     Union type cannot be inherited manually.
    /// </summary>
    public static readonly DiagnosticDescriptor UnionCannotBeInheritedManually = new(
        id: "FXKIT0004",
        title: "Union type cannot be inherited manually",
        messageFormat:
        "'{0}' inherits union type '{1}' which is not allowed. Only declare constituents by nesting them inside of the union using 'partial record'.",
        category: Categories.GeneralCodeGeneration,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    ///     Union type must not declare a primary constructor.
    /// </summary>
    public static readonly DiagnosticDescriptor UnionDeclarationMustNotDeclarePrimaryConstructor = new(
        id: "FXKIT0005",
        title: "Union type must not declare a primary constructor",
        messageFormat: "'{0}' is marked as a union and cannot declare a primary constructor",
        category: Categories.GeneralCodeGeneration,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    ///     Invalid union type modifiers.
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidUnionTypeModifiers = new(
        id: "FXKIT0006",
        title: "Invalid union type modifiers",
        messageFormat:
        "'{0}' is marked as a union and cannot be marked as {1} because unions will be automatically marked as abstract",
        category: Categories.GeneralCodeGeneration,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    ///     Unions cannot inherit other types.
    /// </summary>
    public static readonly DiagnosticDescriptor UnionDeclarationCannotInheritAnotherType = new(
        id: "FXKIT0007",
        title: "Unions cannot inherit other types",
        messageFormat:
        "'{0}' is marked as a union and therefore is not allowed to inherit from other types",
        category: Categories.GeneralCodeGeneration,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    ///     Method declarations can't allow type parameter collisions in transformers.
    /// </summary>
    public static readonly DiagnosticDescriptor MethodDeclarationCannotAllowCollidingTypeParameters =
        new(
            id: "FXKIT0008",
            title: "Method declarations cannot allow colliding type parameters in transformers",
            messageFormat:
            "Type parameters collide with those of outer functors during transformer generation",
            category: Categories.GeneralCodeGeneration,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    /// <summary>
    ///     Lambda attribute can only be used on static methods.
    /// </summary>
    public static readonly DiagnosticDescriptor LambdaAttributeCannotBeUsedOnNonStaticMethods =
        new(
            id: "FXKIT0009",
            title: "Lambda attribute cannot be used on non-static methods",
            messageFormat:
            "The [Lambda] attribute is only supported on class/record/struct declarations and static methods",
            category: Categories.GeneralCodeGeneration,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    #endregion
}
