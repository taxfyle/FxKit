using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static FxKit.CompilerServices.CodeGenerators.Transformers.Helpers;
using static FxKit.CompilerServices.Utilities.TypeComposer;

// ReSharper disable CoVariantArrayConversion

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Creates transformer methods.
/// </summary>
public static class TransformerClassBuilder
{
    private const string IEnumerableFullyQualifiedName   = "System.Collections.Generic.IEnumerable";
    private const string IReadOnlyListFullyQualifiedName = "System.Collections.Generic.IReadOnlyList";

    private const string ThisParameterName = "source";
    private const string InnerFunctorName  = "inner";
    private const string FunctorMap        = "Map";
    private const string FunctorFlatten    = "Unwrap";
    private const string Traverse          = "Traverse";
    private const string Sequence          = "Sequence";

    private const string LinqMonadicBind = "SelectMany";
    private const string LinqFilterName  = "Where";
    private const string CSharpTaskMonad = "Task";

    /// <summary>
    ///     Creates the transformer class and namespace.
    /// </summary>
    /// <param name="methodGroup"></param>
    /// <param name="allOuterContainers"></param>
    /// <param name="typeParamCollisions"></param>
    /// <returns></returns>
    public static (string Name, CompilationUnitSyntax Unit) CreateTransformerFile(
        IGrouping<(string Functor, string Namespace), FunctorMethodDescriptor> methodGroup,
        IEnumerable<Functor> allOuterContainers,
        out List<FunctorMethodDescriptor> typeParamCollisions)
    {
        var namespaceList = new HashSet<string>();
        var methodsWithCollidingTypeParams = new List<FunctorMethodDescriptor>();
        var transformerMethods = new List<MethodDeclarationSyntax>();

        var namespaceDecl = NamespaceDeclaration(IdentifierName(methodGroup.Key.Namespace));
        var classDecl = ClassDeclaration($"{methodGroup.Key.Functor}T")
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.StaticKeyword)));

        // We don't want to nest one functor within the same functor.
        var methodGroupFunctorFullyQualifiedName = FullyQualifiedName(
            methodGroup.Key.Namespace,
            methodGroup.Key.Functor);
        var outerContainers = allOuterContainers.Where(
            o => o.FullyQualifiedName != methodGroupFunctorFullyQualifiedName);

        foreach (var outer in outerContainers)
        {
            namespaceList.Add(outer.ContainingNamespace);
            namespaceList.Add(outer.FunctorMethodsNamespace);

            foreach (var method in methodGroup)
            {
                namespaceList.AddRange(method.RequiredNamespaces);

                // Skip "SelectMany" for now since it requires a different implementation. 
                // We can come back to this later.
                if (method.Name == LinqMonadicBind)
                {
                    continue;
                }

                // Special provisions for `Traverse`/`Sequence`.
                if (method.Name is Traverse or Sequence)
                {
                    // Skip over `Traverse`/`Sequence` methods that would nest two of the same functors.
                    if (method.ReturnType is ReturnType.Functor f &&
                        f.FunctorReference.FullyQualifiedName == outer.FullyQualifiedName)
                    {
                        continue;
                    }

                    // Generate additional overloads of `Traverse`/`Sequence` that
                    // accept `IReadOnlyList`, since C# variance sucks.
                    // For example, doing a `SequenceT` on a `Task<IReadOnlyList<Validation<..>>>` would
                    // not be allowed because `SequenceT`'s input is `Task<IEnumerable<Validation<..>>>`,
                    // and C# isn't smart enough to see that yes, an `IReadOnlyList` can in fact be
                    // a `Task<IEnumerable>`...
                    if (method.Functor.FullyQualifiedName == IEnumerableFullyQualifiedName)
                    {
                        // Skip generating `IReadOnlyList<IReadOnlyList<..>>`.
                        if (outer.FullyQualifiedName == IReadOnlyListFullyQualifiedName)
                        {
                            continue;
                        }

                        // Used to synthesize an `IReadOnlyList` overload for methods where the functor (source) is
                        // `IEnumerable`. This is because C# does not allow a `Task<IReadOnlyList>` where
                        // a `Task<IEnumerable>` is expected.
                        var synthesized = method.ReplaceFunctor(
                            method.Functor.ReplaceReference(
                                functor: "IReadOnlyList",
                                containingNamespace: "System.Collections.Generic"));
                        transformerMethods.Add(
                            CreateTransformer(
                                name: ComputeTransformerName(synthesized.Name),
                                outer: outer,
                                method: synthesized,
                                typeParamCollision: out _));
                    }
                }

                transformerMethods.Add(
                    CreateTransformer(
                        ComputeTransformerName(method.Name),
                        outer,
                        method,
                        out var typeParamCollision));

                if (typeParamCollision)
                {
                    methodsWithCollidingTypeParams.Add(method);
                }

                // For LINQ's Where, we want to generate a `WhereT` transformer to be
                // used with method chaining, and a `Where` transformer with the same signature
                // as `WhereT` to be used by the LINQ Query Syntax.
                if (method.Name == LinqFilterName)
                {
                    transformerMethods.Add(
                        CreateTransformer(
                            method.Name,
                            outer,
                            method,
                            out _));
                }
            }
        }

        var nullableEnabledTrivia =
            TriviaList(
                Trivia(
                    NullableDirectiveTrivia(
                        Token(SyntaxKind.EnableKeyword),
                        isActive: true)));

        var usings = namespaceList.Where(n => n != methodGroup.Key.Namespace)
            .OrderBy(u => u)
            .Select(n => UsingDirective(IdentifierName(n)));
        var @namespace = namespaceDecl.AddMembers(classDecl.AddMembers(transformerMethods.ToArray()))
            .WithLeadingTrivia(new SyntaxTriviaList(nullableEnabledTrivia));

        var compilationUnit = CompilationUnit()
            .WithUsings(new SyntaxList<UsingDirectiveSyntax>(usings))
            .WithMembers(new SyntaxList<MemberDeclarationSyntax>(@namespace));

        typeParamCollisions = methodsWithCollidingTypeParams;
        return (classDecl.Identifier.ToString(), compilationUnit);
    }

    /// <summary>
    ///     Creates the transformer method.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="outer"></param>
    /// <param name="method"></param>
    /// <param name="typeParamCollision"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static MethodDeclarationSyntax CreateTransformer(
        string name,
        Functor outer,
        FunctorMethodDescriptor method,
        out bool typeParamCollision)
    {
        var stackedSource = Compose(outer, method.Functor);
        var rawReturnType = method.ReturnType switch
        {
            ReturnType.Functor functor => Compose(outer, functor.FunctorReference),
            ReturnType.Primitive primitive => Compose(outer, primitive.Type),
            _ => throw new ArgumentOutOfRangeException(nameof(method.ReturnType))
        };

        var returnType = FlattenReturnType(rawReturnType, out var flattened);

        var modifiers = TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));
        var transformer = MethodDeclaration(returnType, name)
            .WithModifiers(modifiers)
            .WithTypeParameterList(
                ComputeTransformerTypeParameters(method, outer, out typeParamCollision))
            .WithParameterList(ComputeTransformerParameters(method, stackedSource))
            .WithConstraintClauses(List(ComputeTransformerConstraintClauses(method, outer)))
            .WithExpressionBody(ComputeTransformerExpressionBody(method, flattened))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            .WithLeadingTrivia(CreateDocumentation(outer.Name, method.Functor.Name, method.Name));

        return transformer;
    }

    /// <summary>
    ///     Computes the name for the transformer method.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static string ComputeTransformerName(string name) => name switch
    {
        "Select" => "Select",
        _        => $"{name}T"
    };

    /// <summary>
    ///     Processes the raw computed return type to handle requirements for flattening.
    /// </summary>
    /// <remarks>
    ///     Currently handles cases related to <see cref="System.Threading.Tasks.Task" /> flattening.
    /// </remarks>
    /// <param name="returnType"></param>
    /// <param name="flattened"></param>
    /// <returns></returns>
    private static GenericNameSyntax FlattenReturnType(GenericNameSyntax returnType, out bool flattened)
    {
        if (returnType.Identifier.ToString() != CSharpTaskMonad)
        {
            flattened = false;
            return returnType;
        }

        //  `gns` is the nested type - i.e, `A` in `Task<A>`.
        if (returnType.TypeArgumentList.Arguments.First() is not GenericNameSyntax gns)
        {
            flattened = false;
            return returnType;
        }

        if (gns.Identifier.ToString() != returnType.Identifier.ToString())
        {
            flattened = false;
            return returnType;
        }

        flattened = true;
        return gns;
    }

    /// <summary>
    ///     Computes the required type parameters for the transformer given the method in question
    ///     and the functor type.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="outer"></param>
    /// <param name="typeParamCollision"></param>
    /// <returns></returns>
    private static TypeParameterListSyntax ComputeTransformerTypeParameters(
        FunctorMethodDescriptor method,
        Functor outer,
        out bool typeParamCollision)
    {
        // For a method of the form:
        // 
        // Result<TNewOk, TErr> Map<TOk, TErr, TNewOk>(...)
        // 
        // If we're creating a Validation of Result transformer and use the
        // original method's type parameters as well as Validation's type 
        // parameters as the transformer's type parameters, we'll end up 
        // with an unused parameter:
        // 
        // Validation<Result<TNewOk, TErr>, TInvalid> MapT<TOk, TErr, TNewOk, TValid, TInvalid>(...).
        // 
        // In this case, it was `TValid`. It's not used because `Result` gets used in its place.
        // For that reason, we want to remove the type parameter in the spot where we're
        // placing the composed type.

        var typeParams = method.TypeParameters.Concat(outer.TypeParameters.Skip(1)).ToImmutableList();

        typeParamCollision = typeParams.Select(x => x.Identifier.ToString()).Distinct().Count() !=
                             typeParams.Count;
        return TypeParameterList(SeparatedList(typeParams));
    }

    /// <summary>
    ///     Computes the constraint clauses for the method.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="outer"></param>
    /// ///
    /// <returns></returns>
    private static IEnumerable<TypeParameterConstraintClauseSyntax> ComputeTransformerConstraintClauses(
        FunctorMethodDescriptor method,
        Functor outer)
    {
        // When composing `Result<TOk, TErr>` within `Validation<TValid, TInvalid>`, the
        // `TValid` parameter will no longer be needed (because `Result<TOk, TErr>` is `TValid`).
        // Therefore, we don't want to copy its generic constraint over to the new method either.

        var parameterApplied = outer.TypeParameters.ToList().ElementAt(0);

        var clausesFromOuter = outer.ConstraintClauses.ToImmutableList()
            .Where(a => a.Name.ToString() != parameterApplied.Identifier.ToString());

        return method.ConstraintClauses.Concat(clausesFromOuter);
    }

    /// <summary>
    ///     Computes the required parameters for the transformer given the method in question
    ///     and the functor stack.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="stack"></param>
    /// <returns></returns>
    private static ParameterListSyntax ComputeTransformerParameters(
        FunctorMethodDescriptor method,
        GenericNameSyntax stack)
    {
        var thisParameter = Parameter(Identifier(ThisParameterName))
            .WithType(stack)
            .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)));

        return ParameterList(SeparatedList(ListOfOne(thisParameter).Concat(method.Parameters)));
    }

    /// <summary>
    ///     Generates the expression body for the transformer.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="flatten"></param>
    /// <returns></returns>
    private static ArrowExpressionClauseSyntax ComputeTransformerExpressionBody(
        FunctorMethodDescriptor method,
        bool flatten = false)
    {
        var innerLambdaExpression =
            SimpleLambdaExpression(Parameter(Identifier(InnerFunctorName)))
                .WithExpressionBody(
                    InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(InnerFunctorName),
                                IdentifierName(method.Name)))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList(
                                    method.Parameters.Select(
                                        x => Argument(IdentifierName(x.Identifier.ToString())))))));

        var bodyExpression = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(ThisParameterName),
                    IdentifierName(FunctorMap)))
            .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(innerLambdaExpression))));

        if (flatten)
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
