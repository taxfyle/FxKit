using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using FxKit.CompilerServices.CodeGenerators.Lambdas;
using FxKit.CompilerServices.CodeGenerators.Unions;
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
    private static void AnalyzeNode(SyntaxNodeAnalysisContext ctx)
    {
        var typeDecl = Unsafe.As<TypeDeclarationSyntax>(ctx.Node);

        // No attributes, so no reason to analyze.
        if (typeDecl.AttributeLists.Count == 0)
        {
            return;
        }

        var semanticModel = ctx.SemanticModel;
        if (!SemanticModelHelper.ContainsAnyAttribute(
                typeDecl.AttributeLists,
                semanticModel,
                attrsThatRequirePartial,
                out var foundAttribute,
                ctx.CancellationToken))
        {
            return;
        }

        var nodesMissingPartial =
            typeDecl.AncestorsAndSelf()
                .OfType<TypeDeclarationSyntax>()
                .Where(static t => t.Modifiers.Any(SyntaxKind.PartialKeyword) == false)
                .ToArray();

        // The type and it's ancestry is partial, so we can exit now.
        if (nodesMissingPartial.Length == 0)
        {
            return;
        }

        // It contains an attribute that requires partials, and
        // a type was not declared as a partial; report it.
        foreach (var missingPartial in nodesMissingPartial)
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();
            ctx.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticsDescriptors.MustBePartial,
                    missingPartial.Identifier.GetLocation(),
                    missingPartial.Identifier.Text,
                    // Dammit-operator here because the project can't use NRT analysis attributes.
                    foundAttribute!.Name.ToString()));
        }
    }
}
