using System.Collections.Immutable;
using FxKit.CompilerServices.CodeGenerators;
using FxKit.CompilerServices.CodeGenerators.Lambdas;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator

namespace FxKit.CompilerServices.Analyzers;

/// <summary>
///     Analyzer that checks whether any of the codegen attributes are used on non-partial types.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MustBePartialAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     Attributes that require the use of a partial.
    /// </summary>
    private static readonly ImmutableHashSet<string> attrsThatRequirePartial =
        ImmutableHashSet.Create(
            LambdaGenerator.LambdaAttrName,
            UnionGenerator.UnionAttrName);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticsDescriptors.MustBePartial);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeNode,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration);
    }

    /// <summary>
    ///     Analyzes the node to check if it contains any attribute that requires it to be marked
    ///     as a partial type.
    /// </summary>
    /// <param name="ctx"></param>
    private void AnalyzeNode(SyntaxNodeAnalysisContext ctx)
    {
        var typeDecl = (TypeDeclarationSyntax)ctx.Node;

        // The type is a partial, so we can exit now.
        if (typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        // No attributes, so no reason to analyze.
        if (typeDecl.AttributeLists.Count == 0)
        {
            return;
        }

        var semanticModel = ctx.SemanticModel;
        if (SemanticModelHelper.ContainsAnyAttribute(
                typeDecl.AttributeLists,
                semanticModel,
                attrsThatRequirePartial,
                out var foundAttribute))
        {
            // It contains an attribute that requires partials, and
            // the type was not declared as a partial; report it.
            ctx.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticsDescriptors.MustBePartial,
                    typeDecl.Identifier.GetLocation(),
                    typeDecl.Identifier.Text,
                    // Dammit-operator here because the project can't use NRT analysis attributes.
                    foundAttribute!.Name.ToString()));
        }
    }
}
