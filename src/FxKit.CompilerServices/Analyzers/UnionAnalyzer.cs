using System.Collections.Immutable;
using FxKit.CompilerServices.CodeGenerators;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable InvertIf

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable LoopCanBeConvertedToQuery

namespace FxKit.CompilerServices.Analyzers;

/// <summary>
///     Checks for correct usage of the Union attribute.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnionAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticsDescriptors.MissingUnionConstituents,
        DiagnosticsDescriptors.InvalidUnionConstituentDeclaration,
        DiagnosticsDescriptors.UnionCannotBeInheritedManually,
        DiagnosticsDescriptors.UnionDeclarationMustNotDeclarePrimaryConstructor,
        DiagnosticsDescriptors.InvalidUnionTypeModifiers,
        DiagnosticsDescriptors.UnionDeclarationCannotInheritAnotherType);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.RecordDeclaration);
    }

    /// <summary>
    ///     Analyzes the node.
    /// </summary>
    /// <param name="ctx"></param>
    private static void AnalyzeNode(SyntaxNodeAnalysisContext ctx)
    {
        var recordDeclarationSyntax = (RecordDeclarationSyntax)ctx.Node;

        // Check if declared as a union record.
        var isUnionRecord = IsDeclaredAsUnionRecord(ctx, recordDeclarationSyntax);
        if (isUnionRecord)
        {
            // There is some weird behavior in Roslyn where a RecordDeclaration is visited twice: one time
            // for the record itself and another for its' primary constructor (if one is defined).
            // To avoid reporting the error twice, make sure we only check the declaration.
            if (ctx.ContainingSymbol is IMethodSymbol)
            {
                return;
            }

            // Check if the union record is declared correctly.
            var isUnionRecordDeclaredCorrectly =
                AnalyzeIfUnionRecordDeclaredCorrectly(ctx, recordDeclarationSyntax);

            if (!isUnionRecordDeclaredCorrectly)
            {
                return;
            }

            // Check if missing constituents.
            AnalyzeIfMissingConstituents(ctx, recordDeclarationSyntax);
        }
        else
        {
            // The record is not itself a union declaration.
            // Analyze it anyway as it may be a constituent.
            ctx.CancellationToken.ThrowIfCancellationRequested();
            AnalyzeUnionConstituentDeclaration(ctx, recordDeclarationSyntax);

            // It may also be another record that is attempting to implement a union.
            ctx.CancellationToken.ThrowIfCancellationRequested();
            AnalyzeRecordIsNotImplementingUnion(ctx, recordDeclarationSyntax);
        }
    }

    /// <summary>
    ///     Analyzes if the union record is declared correctly, as in it does not
    ///     have any extra modifiers besides partial, and does not have a primary constructor.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="recordDeclarationSyntax"></param>
    /// <returns></returns>
    private static bool AnalyzeIfUnionRecordDeclaredCorrectly(
        SyntaxNodeAnalysisContext ctx,
        RecordDeclarationSyntax recordDeclarationSyntax)
    {
        var valid = true;

        // Check for invalid modifiers.
        if (recordDeclarationSyntax.Modifiers.Count > 2)
        {
            foreach (var modifier in recordDeclarationSyntax.Modifiers)
            {
                if (modifier.IsKind(SyntaxKind.SealedKeyword) ||
                    modifier.IsKind(SyntaxKind.AbstractKeyword))
                {
                    valid = false;
                    ctx.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticsDescriptors.InvalidUnionTypeModifiers,
                            modifier.GetLocation(),
                            recordDeclarationSyntax.Identifier.Text,
                            modifier.Text));
                }
            }
        }

        // Check for primary constructor.
        if (recordDeclarationSyntax.ParameterList?.Parameters.Count > 0)
        {
            valid = false;
            ctx.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticsDescriptors.UnionDeclarationMustNotDeclarePrimaryConstructor,
                    recordDeclarationSyntax.ParameterList.GetLocation(),
                    recordDeclarationSyntax.Identifier.Text));
        }

        // Check that the union does not inherit from another type.
        if (recordDeclarationSyntax.BaseList is { Types.Count: > 0 })
        {
            var recordTypeInfo = ctx.SemanticModel.GetDeclaredSymbol(recordDeclarationSyntax);
            if (recordTypeInfo is { BaseType.SpecialType: not SpecialType.System_Object })
            {
                valid = false;
                ctx.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticsDescriptors.UnionDeclarationCannotInheritAnotherType,
                        recordDeclarationSyntax.BaseList.GetLocation(),
                        recordDeclarationSyntax.Identifier.Text));
            }
        }

        return valid;
    }

    /// <summary>
    ///     Checks if the union record is missing constituents.
    ///     This is only called in a context where it has been determined that the record is
    ///     a union declaration.
    /// </summary>
    /// <example>
    ///     [Union]
    ///     public partial record MyUnion;
    ///     ^^^^^^^ Missing constituents!
    /// </example>
    /// <param name="ctx"></param>
    /// <param name="recordDecl"></param>
    /// <returns></returns>
    private static void AnalyzeIfMissingConstituents(
        SyntaxNodeAnalysisContext ctx,
        BaseTypeDeclarationSyntax recordDecl)
    {
        // Check the direct child nodes for any record declarations.
        foreach (var syntaxNode in recordDecl.ChildNodes())
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();
            if (syntaxNode is RecordDeclarationSyntax)
            {
                // We found at least one record declaration which could
                // be a constituent.
                return;
            }
        }

        // No nested record was found, report it.
        ctx.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticsDescriptors.MissingUnionConstituents,
                recordDecl.Identifier.GetLocation(),
                recordDecl.Identifier.Text));
    }

    /// <summary>
    ///     Analyze union constituents as having been declared correctly.
    /// </summary>
    /// <example>
    ///     [Union]
    ///     public partial record MyUnion
    ///     {
    ///     public record Incorrect;
    ///     ^^^^^^ Missing partial!
    ///     }
    /// </example>
    /// <param name="ctx"></param>
    /// <param name="recordDecl"></param>
    private static void AnalyzeUnionConstituentDeclaration(
        SyntaxNodeAnalysisContext ctx,
        TypeDeclarationSyntax recordDecl)
    {
        // Check if the parent node is a record declaration with the Union attribute.
        // If not, then bail.
        if (recordDecl.Parent is not RecordDeclarationSyntax parentRecordDecl)
        {
            return;
        }

        // Check if the record declaration is declared like "partial record".
        // That is, it only has 1 modifier (partial).
        if (recordDecl.Modifiers.Count == 1 && recordDecl.Modifiers[0].IsKind(SyntaxKind.PartialKeyword))
        {
            // This is correct.
            return;
        }

        // There is some weird behavior in Roslyn where a RecordDeclaration is visited twice: one time
        // for the record itself and another for its' primary constructor (if one is defined).
        // To avoid reporting the error twice, make sure we only check the declaration.
        if (ctx.ContainingSymbol is IMethodSymbol)
        {
            return;
        }

        // Since the record was nested inside of a record, check if the
        // parent record is a partial. If it isn't, then it is unlikely that it is a
        // Union type, and if it was, then the other analyzer will report it.
        if (!IsDeclaredAsUnionRecord(ctx, parentRecordDecl))
        {
            return;
        }

        // Report the issue.
        ctx.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticsDescriptors.InvalidUnionConstituentDeclaration,
                recordDecl.Keyword.GetLocation(),
                recordDecl.Identifier.Text,
                parentRecordDecl.Identifier.Text));
    }

    /// <summary>
    ///     Checks whether the given record declaration does not inherit from
    ///     a declared union. Since our analyzer does not analyze generated code, we don't need to
    ///     check for anything else.
    /// </summary>
    /// <example>
    ///     [Union]
    ///     public partial record MyUnion
    ///     {
    ///     partial record Correct;
    ///     }
    ///     public partial record Incorrect : MyUnion;
    ///     ^^^^^^^ Not allowed!
    /// </example>
    /// <param name="ctx"></param>
    /// <param name="recordDeclarationSyntax"></param>
    private static void AnalyzeRecordIsNotImplementingUnion(
        SyntaxNodeAnalysisContext ctx,
        BaseTypeDeclarationSyntax recordDeclarationSyntax)
    {
        // Check if the record inherits/implements anything.
        if (recordDeclarationSyntax.BaseList?.Types is not { Count: > 0 } baseTypes)
        {
            return;
        }

        // Check if any of them is a declared union.
        foreach (var baseTypeSyntax in baseTypes)
        {
            // Retrieve the base type symbol.
            var symbolInfo = ctx.SemanticModel.GetSymbolInfo(baseTypeSyntax.Type);
            var symbol = symbolInfo.Symbol;
            if (symbol is null)
            {
                continue;
            }

            // Retrieve the base type's attributes. If any of them is our Union attribute,
            // then it means it was used in a non-closed way.
            var attrs = symbol.GetAttributes();
            foreach (var attributeData in attrs)
            {
                var name = attributeData.AttributeClass?.ToDisplayString();
                if (name != UnionGenerator.UnionAttrName)
                {
                    continue;
                }

                // The base type was a union, meaning a type tried to inherit from it
                // which is not allowed.
                ctx.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticsDescriptors.UnionCannotBeInheritedManually,
                        baseTypeSyntax.Type.GetLocation(),
                        recordDeclarationSyntax.Identifier.Text,
                        symbol.Name));
                return;
            }
        }
    }

    /// <summary>
    ///     Checks if the record declaration is a Union. This does not mean that it is
    ///     declared correctly, only that it was a partial and that it has the attribute applied.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="recordDecl"></param>
    /// <returns></returns>
    private static bool IsDeclaredAsUnionRecord(
        SyntaxNodeAnalysisContext ctx,
        RecordDeclarationSyntax recordDecl)
    {
        // Check if its' a partial. It can't be a valid union without being a partial.
        if (!recordDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            // Not a partial, so don't bother.
            return false;
        }

        // In this case it was a partial; check if it contains the Union attribute.
        return SemanticModelHelper.ContainsExactAttribute(
            recordDecl.AttributeLists,
            ctx.SemanticModel,
            UnionGenerator.UnionAttrName);
    }
}
