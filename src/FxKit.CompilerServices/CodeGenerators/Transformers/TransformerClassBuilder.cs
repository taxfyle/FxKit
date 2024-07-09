using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static FxKit.CompilerServices.CodeGenerators.Transformers.Helpers;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Creates transformer methods.
/// </summary>
internal static class TransformerClassBuilder
{
    /// <summary>
    ///     Creates the transformer class and namespace.
    /// </summary>
    /// <param name="transformerSet"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static (string Name, CompilationUnitSyntax Unit) CreateTransformerFile(
        TransformerSet transformerSet,
        CancellationToken cancellationToken)
    {
        var transformerMethods = new List<MethodDeclarationSyntax>();

        var namespaceDecl = NamespaceDeclaration(IdentifierName(transformerSet.FunctorNamespace));
        var classDecl = ClassDeclaration(transformerSet.GeneratedClassName)
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword)));

        foreach (var methodDescriptor in transformerSet.TransformerMethods)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var returnType = methodDescriptor.ReturnType.ToGenericNameSyntax();
            var modifiers = TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));
            var typeParameters = TypeParameterList(
                SeparatedList(methodDescriptor.TypeParameterNames.Select(static t => TypeParameter(t))));

            var transformer = MethodDeclaration(returnType, methodDescriptor.TransformerMethodName)
                .WithModifiers(modifiers)
                .WithTypeParameterList(typeParameters)
                .WithParameterList(ComputeTransformerParameters(methodDescriptor))
                .WithConstraintClauses(
                    List(
                        methodDescriptor.TypeParameterConstraints.SelectNotNull(
                            static t => t.ToConstraintClauseSyntax())))
                .WithExpressionBody(ComputeTransformerExpressionBody(methodDescriptor))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(
                    CreateDocumentation(
                        methodDescriptor.OuterFunctorName,
                        methodDescriptor.InnerFunctorName,
                        methodDescriptor.InnerMethodName));

            transformerMethods.Add(transformer);
        }


        var nullableEnabledTrivia =
            TriviaList(
                Trivia(
                    NullableDirectiveTrivia(
                        Token(SyntaxKind.EnableKeyword),
                        isActive: true)));

        var usings = transformerSet.RequiredNamespaces
            .Select(n => UsingDirective(IdentifierName(n)));
        var @namespace = namespaceDecl.AddMembers(classDecl.AddMembers([..transformerMethods]))
            .WithLeadingTrivia(new SyntaxTriviaList(nullableEnabledTrivia));

        var compilationUnit = CompilationUnit()
            .WithUsings(new SyntaxList<UsingDirectiveSyntax>(usings))
            .WithMembers(new SyntaxList<MemberDeclarationSyntax>(@namespace));

        return (classDecl.Identifier.ToString(), compilationUnit);
    }

    /// <summary>
    ///     Computes the required parameters for the transformer given the method in question
    ///     and the functor stack.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    private static ParameterListSyntax ComputeTransformerParameters(
        FunctorTransformerMethodDescriptor method)
    {
        var thisParameter = Parameter(Identifier(ThisParameterName))
            .WithType(method.StackedSource.ToGenericNameSyntax())
            .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)));

        var parameters = method.Parameters
            .Select(
                static p =>
                {
                    var parameter = Parameter(Identifier(p.Name))
                        .WithType(ParseTypeName(p.TypeFullName));

                    return p.Default is not null
                        ? parameter.WithDefault(EqualsValueClause(ParseExpression(p.Default)))
                        : parameter;
                });

        return ParameterList(SeparatedList(SingletonList(thisParameter).Concat(parameters)));
    }

    /// <summary>
    ///     Generates the expression body for the transformer.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    private static ArrowExpressionClauseSyntax ComputeTransformerExpressionBody(
        FunctorTransformerMethodDescriptor method)
    {
        var innerLambdaExpression =
            SimpleLambdaExpression(Parameter(Identifier(InnerFunctorName)))
                .WithExpressionBody(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(InnerFunctorName),
                                IdentifierName(method.InnerMethodName)))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList(
                                    method.Parameters.Select(x => Argument(IdentifierName(x.Name)))))));

        var bodyExpression = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(ThisParameterName),
                    IdentifierName(FunctorMap)))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(innerLambdaExpression))));

        if (method.RequiresFlattening)
        {
            bodyExpression = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    bodyExpression,
                    IdentifierName(FunctorFlatten)));
        }

        return ArrowExpressionClause(bodyExpression);
    }

    /// <summary>
    ///     Creates the XML Documentation Comment for the Transformer Method.
    /// </summary>
    /// <param name="outerName"></param>
    /// <param name="innerName"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    private static SyntaxTriviaList CreateDocumentation(
        string outerName,
        string innerName,
        string methodName)
        =>
            TriviaList(
                Trivia(
                    DocumentationCommentTrivia(
                        SyntaxKind.SingleLineDocumentationCommentTrivia,
                        List(
                            new XmlNodeSyntax[]
                            {
                                XmlText()
                                    .WithTextTokens(
                                        TokenList(
                                            XmlTextLiteral(
                                                TriviaList(DocumentationCommentExterior("///")),
                                                " ",
                                                " ",
                                                TriviaList()))),
                                XmlExampleElement(
                                        SingletonList<XmlNodeSyntax>(
                                            XmlText()
                                                .WithTextTokens(
                                                    TokenList(
                                                        XmlTextNewLine(
                                                            TriviaList(),
                                                            "\n",
                                                            "\n",
                                                            TriviaList()),
                                                        XmlTextLiteral(
                                                            TriviaList(
                                                                DocumentationCommentExterior("///")),
                                                            $"     <c>{outerName}</c> of <c>{innerName}</c> Transformer Method for <c>{methodName}</c>.",
                                                            $"     <c>{outerName}</c> of <c>{innerName}</c> Transformer Method for <c>{methodName}</c>.",
                                                            TriviaList()),
                                                        XmlTextNewLine(
                                                            TriviaList(),
                                                            "\n",
                                                            "\n",
                                                            TriviaList()),
                                                        XmlTextLiteral(
                                                            TriviaList(
                                                                DocumentationCommentExterior("///")),
                                                            " ",
                                                            " ",
                                                            TriviaList())))))
                                    .WithStartTag(XmlElementStartTag(XmlName(Identifier("summary"))))
                                    .WithEndTag(XmlElementEndTag(XmlName(Identifier("summary")))),
                                XmlText()
                                    .WithTextTokens(
                                        TokenList(
                                            XmlTextNewLine(
                                                TriviaList(),
                                                "\n",
                                                "\n",
                                                TriviaList())))
                            }))));
}
