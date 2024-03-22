using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FxKit.CompilerServices.CodeGenerators;

/// <summary>
///     Generates lambda overloads for public constructors or methods marked with <c>Lambda</c> attribute
/// </summary>
[Generator]
public class LambdaGenerator : IIncrementalGenerator
{
    public const string LambdaAttrName = "FxKit.CompilerServices.LambdaAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter for class, records and structs.
        var typeDeclarations =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: LambdaAttrName,
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (ctx, _) => FilterSemanticTargetForGeneration(ctx));

        // Combine the selected type declarations with the Compilation.
        var compilationAndTypeDecls = context.CompilationProvider.Combine(typeDeclarations.Collect());

        // Generate the the source using the compilation and type declarations.
        context.RegisterSourceOutput(
            compilationAndTypeDecls,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void GenerateLambdasForTypeDeclaration(
        SourceProductionContext context,
        Compilation compilation,
        SyntaxTree root,
        TypeDeclarationSyntax typeDeclaration)
    {
        // Check if the type declaration is a record, class, or struct with a primary constructor
        switch (typeDeclaration)
        {
            case RecordDeclarationSyntax { ParameterList: not null } recordDeclaration:
                GenerateLambdaConstructorForType(
                    context: context,
                    compilation,
                    root: root,
                    declaration: recordDeclaration,
                    parameterList: recordDeclaration.ParameterList);
                return;
            case ClassDeclarationSyntax { ParameterList: not null } classDeclaration:
                GenerateLambdaConstructorForType(
                    context: context,
                    compilation,
                    root: root,
                    declaration: classDeclaration,
                    parameterList: classDeclaration.ParameterList);
                return;
            case StructDeclarationSyntax { ParameterList: not null } structDeclaration:
                GenerateLambdaConstructorForType(
                    context: context,
                    compilation,
                    root: root,
                    declaration: structDeclaration,
                    parameterList: structDeclaration.ParameterList);
                return;
        }

        var publicConstructorDeclaration = typeDeclaration.Members.OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault(
                static member => member.Modifiers.Any(static m => m.IsKind(SyntaxKind.PublicKeyword)));

        if (publicConstructorDeclaration is not null)
        {
            GenerateLambdaConstructorForType(
                context: context,
                compilation: compilation,
                root: root,
                declaration: typeDeclaration,
                parameterList: publicConstructorDeclaration.ParameterList);
        }
    }

    /// <summary>
    ///     Only include type declarations that are classes, records or structs and are partials.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
        // Declared on a type
        node is TypeDeclarationSyntax typeDecl and (ClassDeclarationSyntax
            or RecordDeclarationSyntax
            or StructDeclarationSyntax) &&
        typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword) ||
        // Declared on a static method
        node is MethodDeclarationSyntax methodDecl &&
        methodDecl.Modifiers.Any(SyntaxKind.StaticKeyword);

    /// <summary>
    ///     Only include the type declarations and methods with the expected attribute.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private static SyntaxNode FilterSemanticTargetForGeneration(
        GeneratorAttributeSyntaxContext ctx) =>
        ctx.TargetNode switch
        {
            TypeDeclarationSyntax typeDecl => typeDecl,
            var methodDecl                 => (MethodDeclarationSyntax)methodDecl
        };

    /// <summary>
    ///     Generates the code.
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="declarations"></param>
    /// <param name="ctx"></param>
    private static void Execute(
        Compilation compilation,
        ImmutableArray<SyntaxNode> declarations,
        SourceProductionContext ctx)
    {
        if (declarations.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var declarationSyntax in declarations)
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();

            var tree = declarationSyntax.SyntaxTree;
            if (declarationSyntax is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                if (!typeDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    continue;
                }

                GenerateLambdasForTypeDeclaration(ctx, compilation, tree, typeDeclarationSyntax);
            }

            else if (declarationSyntax is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                GenerateLambdaForMethodDeclaration(ctx, compilation, tree, methodDeclarationSyntax);
            }
        }
    }

    private static void GenerateLambdaConstructorForType(
        SourceProductionContext context,
        Compilation compilation,
        SyntaxTree root,
        TypeDeclarationSyntax declaration,
        ParameterListSyntax parameterList)
    {
        var semanticModel = compilation.GetSemanticModel(root);
        var originalSymbol = semanticModel.GetDeclaredSymbol(declaration);

        if (originalSymbol is null)
        {
            return;
        }

        var paramSymbols = parameterList.Parameters
            .SelectNotNull(p => semanticModel.GetDeclaredSymbol(p))
            .ToArray();

        var originalTypeNamespace = originalSymbol.ContainingNamespace.ToString();
        if (paramSymbols.Length == 0)
        {
            return;
        }

        var namespaces = new HashSet<string>
        {
            "System"
        };
        var funcTypeParams = string.Join(
            ", ",
            paramSymbols.Select(
                static s => s.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        var paramNames = string.Join(
            ", ",
            paramSymbols.Select(static s => s.Name));

        foreach (var symbol in paramSymbols)
        {
            var ns = symbol.Type.ContainingNamespace.ToDisplayString();
            if (ns == originalTypeNamespace)
            {
                continue;
            }

            namespaces.Add(ns);
        }

        var sb = new StringBuilder();
        foreach (var ns in namespaces)
        {
            sb.Append("using ").Append(ns).AppendLine(";");
        }

        sb.AppendLine();
        sb.Append("namespace ").Append(originalTypeNamespace).Append(';').AppendLine();

        // Respects the type hierarchy.
        var parentCount = 0;
        foreach (var ancestor in declaration.Ancestors()
                     .OfType<TypeDeclarationSyntax>()
                     .Reverse())
        {
            sb.Append(ancestor.Modifiers.ToString())
                .Append(' ')
                .Append(ancestor.Keyword.Text)
                .Append(' ')
                .Append(ancestor.Identifier.Text)
                .Append(" {")
                .AppendLine();
            parentCount++;
        }

        sb.AppendLine();
        sb.Append(declaration.Modifiers)
            .Append(' ')
            .Append(declaration.Keyword)
            .Append(' ')
            .Append(originalSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        sb.AppendLine().Append('{').AppendLine();
        sb.Append("    /// <summary>").AppendLine();
        sb.Append("    ///     The ")
            .Append(declaration.Identifier.Text)
            .Append(" constructor as a Func.")
            .AppendLine();
        sb.AppendLine("    /// </summary>");
        sb.Append("    public static readonly Func<");
        sb.Append(funcTypeParams);
        sb.Append(", ");
        sb.Append(originalSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        sb.Append("> λ = (").Append(paramNames).Append(") => new(").Append(paramNames).AppendLine(");");
        sb.Append('}').AppendLine();

        while (parentCount-- > 0)
        {
            sb.AppendLine("}");
        }

        context.AddSource(
            $"{declaration.Identifier.Text}.Generated.cs",
            source: sb.ToString());
    }

    private static void GenerateLambdaForMethodDeclaration(
        SourceProductionContext context,
        Compilation compilation,
        SyntaxTree root,
        MethodDeclarationSyntax methodDeclaration)
    {
        // TODO: Make this generator nicer and combine into the same generated file.
        var semanticModel = compilation.GetSemanticModel(root);
        var originalSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);

        if (originalSymbol is null)
        {
            return;
        }

        var typeDeclaration = methodDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>(
            t => t.Modifiers.Any(SyntaxKind.PartialKeyword),
            ascendOutOfTrivia: true);
        if (typeDeclaration is null)
        {
            return;
        }

        var typeDeclarationSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
        if (typeDeclarationSymbol is null)
        {
            return;
        }

        var paramSymbols = methodDeclaration.ParameterList.Parameters
            .SelectNotNull(p => semanticModel.GetDeclaredSymbol(p))
            .ToArray();

        var originalTypeNamespace = originalSymbol.ContainingNamespace.ToString();
        if (paramSymbols.Length == 0)
        {
            return;
        }

        var returnTypeSymbol = semanticModel.GetSymbolInfo(methodDeclaration.ReturnType).Symbol;
        if (returnTypeSymbol is null)
        {
            return;
        }

        var namespaces = new HashSet<string>
        {
            "System"
        };
        var funcTypeParams = string.Join(
            ", ",
            paramSymbols.Select(
                static s => s.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        var paramNames = string.Join(
            ", ",
            paramSymbols.Select(static s => s.Name));

        foreach (var symbol in paramSymbols)
        {
            var ns = symbol.Type.ContainingNamespace.ToDisplayString();
            if (ns == originalTypeNamespace)
            {
                continue;
            }

            namespaces.Add(ns);
        }

        var sb = new StringBuilder();
        foreach (var ns in namespaces)
        {
            sb.Append("using ").Append(ns).AppendLine(";");
        }

        sb.AppendLine();
        sb.Append("namespace ").Append(originalTypeNamespace).Append(';').AppendLine();

        // Respects the type hierarchy.
        var parentCount = 0;
        foreach (var ancestor in typeDeclaration.Ancestors()
                     .OfType<TypeDeclarationSyntax>()
                     .Reverse())
        {
            sb.Append(ancestor.Modifiers.ToString())
                .Append(' ')
                .Append(ancestor.Keyword.Text)
                .Append(' ')
                .Append(ancestor.Identifier.Text)
                .Append(" {")
                .AppendLine();
            parentCount++;
        }

        sb.AppendLine();
        sb.Append(typeDeclaration.Modifiers)
            .Append(' ')
            .Append(typeDeclaration.Keyword)
            .Append(' ')
            .Append(typeDeclarationSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        sb.AppendLine().Append('{').AppendLine();
        sb.Append("    /// <summary>").AppendLine();
        sb.Append("    ///     The ")
            .Append(methodDeclaration.Identifier.Text)
            .Append(" method as a Func.")
            .AppendLine();
        sb.AppendLine("    /// </summary>");
        sb.Append("    public static readonly Func<");
        sb.Append(funcTypeParams);
        sb.Append(", ");
        sb.Append(returnTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        sb.Append("> ");
        sb.Append(methodDeclaration.Identifier.Text);
        sb.Append("λ = (")
            .Append(paramNames)
            .Append(") => ")
            .Append(methodDeclaration.Identifier.Text)
            .Append("(")
            .Append(paramNames)
            .AppendLine(");");
        sb.Append('}').AppendLine();

        while (parentCount-- > 0)
        {
            sb.AppendLine("}");
        }

        context.AddSource(
            $"{typeDeclaration.Identifier.Text}.{methodDeclaration.Identifier.Text}.Generated.cs",
            source: sb.ToString());
    }
}
