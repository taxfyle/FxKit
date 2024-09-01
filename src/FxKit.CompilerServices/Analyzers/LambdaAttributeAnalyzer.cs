using System.Collections.Immutable;
using FxKit.CompilerServices.CodeGenerators.Lambdas;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FxKit.CompilerServices.Analyzers;

/// <summary>
///     Analyzer that checks whether any of the codegen attributes are used on non-partial types.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LambdaAttributeAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticsDescriptors.LambdaAttributeCannotBeUsedOnNonStaticMethods];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeNode,
            SyntaxKind.MethodDeclaration);
    }

    /// <summary>
    ///     Analyzes the node to check if it contains any attribute that requires it to be marked
    ///     as a partial type.
    /// </summary>
    /// <param name="ctx"></param>
    private void AnalyzeNode(SyntaxNodeAnalysisContext ctx)
    {
        var methodDecl = (MethodDeclarationSyntax)ctx.Node;

        // It has the static keyword, so even if it's not a lambda, we don't have
        // anything to report.
        if (methodDecl.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        // No attributes, so no reason to analyze.
        if (methodDecl.AttributeLists.Count == 0)
        {
            return;
        }

        var semanticModel = ctx.SemanticModel;
        if (SemanticModelHelper.ContainsExactAttribute(
                methodDecl.AttributeLists,
                semanticModel,
                LambdaGenerator.LambdaAttrName))
        {
            ctx.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticsDescriptors.LambdaAttributeCannotBeUsedOnNonStaticMethods,
                    methodDecl.Identifier.GetLocation(),
                    DiagnosticSeverity.Error));
        }
    }
}
