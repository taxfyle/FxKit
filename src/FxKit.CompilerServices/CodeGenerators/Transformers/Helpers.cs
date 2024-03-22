using FxKit.CompilerServices.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Helpers.
/// </summary>
public static class Helpers
{
    /// <summary>
    ///     Gets the namespaces providing functor behavior for the functor names specified.
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="functorNames"></param>
    /// <returns></returns>
    public static IEnumerable<(string Functor, string Namespace)?> GetFunctorBehaviorNamespaces(
        Compilation compilation,
        ISet<string> functorNames)
    {
        return compilation.SyntaxTrees.SelectMany(
                st => st
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>())
            .Where(IsRelevantFunctorMethod)
            .Select(m => compilation.GetSemanticModel(m.SyntaxTree).GetDeclaredSymbol(m))
            .Select<IMethodSymbol?, (string, string)?>(
                s => s is null
                    ? null
                    : (s.Parameters.First().Type.Name, s.ContainingNamespace.ToDisplayString()));

        // Indicates whether the given method is a relevant functor method.
        bool IsRelevantFunctorMethod(MethodDeclarationSyntax m) =>
            m.Identifier.ToString() == "Map" &&
            m.IsExtensionMethod() &&
            m.ParameterList.Parameters[0].Type is GenericNameSyntax gns &&
            functorNames.Contains(gns.Identifier.ToString());
    }

    /// <summary>
    ///     Gets the namespace for the given functor.
    /// </summary>
    /// <param name="namespaces"></param>
    /// <param name="functorName"></param>
    /// <returns></returns>
    public static string GetNamespaceForFunctor(
        ISet<(string Functor, string Namespace)> namespaces,
        string functorName) =>
        namespaces.First(t => t.Functor == functorName).Namespace;

    /// <summary>
    ///     Converts an enumerable of <see cref="ITypeParameterSymbol" />s to an enumerable of
    ///     <see cref="TypeParameterSyntax" />.
    /// </summary>
    /// <param name="symbols"></param>
    /// <returns></returns>
    public static IEnumerable<TypeParameterSyntax> ToTypeParameterSyntax(
        IEnumerable<ITypeParameterSymbol> symbols)
        => symbols.Select(
            s => TypeParameter(s.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));

    /// <summary>
    ///     Converts an enumerable of <see cref="ITypeParameterSymbol" /> to an enumerable of
    ///     <see cref="TypeParameterConstraintClauseSyntax" />.
    /// </summary>
    /// <param name="symbols"></param>
    /// <returns></returns>
    public static IEnumerable<TypeParameterConstraintClauseSyntax> ToConstraintClauseSyntax(
        IEnumerable<ITypeParameterSymbol> symbols)
    {
        foreach (var typeParameterSymbol in symbols)
        {
            List<TypeParameterConstraintSyntax> constraints = new();

            if (typeParameterSymbol.HasConstructorConstraint)
            {
                constraints.Add(ConstructorConstraint());
            }

            if (typeParameterSymbol.HasNotNullConstraint)
            {
                constraints.Add(TypeConstraint(IdentifierName("notnull")));
            }

            if (typeParameterSymbol.HasReferenceTypeConstraint)
            {
                constraints.Add(ClassOrStructConstraint(SyntaxKind.ClassConstraint));
            }

            if (typeParameterSymbol.HasValueTypeConstraint)
            {
                constraints.Add(ClassOrStructConstraint(SyntaxKind.StructConstraint));
            }

            if (!typeParameterSymbol.ConstraintTypes.IsEmpty)
            {
                constraints.AddRange(
                    typeParameterSymbol.ConstraintTypes.Select(
                        x => TypeConstraint(ParseTypeName(x.Name))));
            }

            yield return TypeParameterConstraintClause(
                IdentifierName(typeParameterSymbol.Name),
                SeparatedList(constraints));
        }
    }

    /// <summary>
    ///     Creates a list of one item.
    /// </summary>
    /// <param name="first"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> ListOfOne<T>(T first) => new List<T>
    {
        first
    };

    /// <summary>
    ///     Computes a type's fully qualified name.
    /// </summary>
    /// <param name="containingNamespace"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static string FullyQualifiedName(string containingNamespace, string typeName) =>
        $"{containingNamespace}.{typeName}";
}
