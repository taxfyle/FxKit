using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FxKit.CompilerServices.CodeGenerators;

/// <summary>
///     Generates a Discriminated Union using nested records inside of a partial record tagged
///     with the [Union] attribute.
/// </summary>
/// <remarks>
///     Does not supported nesting the record in other types.
///     If this is ever needed, there is a way to do it:
///     https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarchy/
/// </remarks>
[Generator]
public class UnionGenerator : IIncrementalGenerator
{
    /// <summary>
    ///     The fully qualified name of the Union attribute.
    /// </summary>
    public const string UnionAttrName = "FxKit.CompilerServices.UnionAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filters record declarations with the
        var recordDeclarations =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: UnionAttrName,
                predicate: static (node, _) => IsSyntaxTargetForGeneration(node),
                transform: static (ctx, _) => FilterSemanticTargetForGeneration(ctx));

        // Combine with the compilation.
        var compilationAndInterfaceDeclarations =
            context.CompilationProvider.Combine(recordDeclarations.Collect());

        // Register the source generation.
        context.RegisterSourceOutput(
            compilationAndInterfaceDeclarations,
            static (spc, source) => Execute(spc, source.Left, source.Right));
    }

    /// <summary>
    ///     Only include type declarations that are classes, records or structs
    ///     which have at least one attribute, have the "partial" modifier and not have the
    ///     "abstract" or "sealed" modifier.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        if (node is not RecordDeclarationSyntax
            {
                // Less than or exactly 2 modifiers (access + partial).
                Modifiers.Count: <= 2
            } recordDecl)
        {
            return false;
        }

        // Don't allow parameters in the primary constructor.
        if (recordDecl.ParameterList?.Parameters.Count > 0)
        {
            return false;
        }

        var isPartial = false;
        foreach (var modifier in recordDecl.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PartialKeyword))
            {
                isPartial = true;
            }

            // Abstract or sealed? Bail.
            if (modifier.IsKind(SyntaxKind.AbstractKeyword) || modifier.IsKind(SyntaxKind.SealedKeyword))
            {
                return false;
            }
        }

        // It only matches if the type is marked as partial.
        return isPartial;
    }

    /// <summary>
    ///     Only include the type declarations with the expected attribute.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    private static RecordDeclarationSyntax? FilterSemanticTargetForGeneration(
        GeneratorAttributeSyntaxContext ctx)
    {
        var recordDecl = (RecordDeclarationSyntax)ctx.TargetNode;

        // Check that the record does not declare a base type.
        // The fastest way to do this is to check if there are any nodes in the base type list,
        // it is not 100% accurate however it is fast to check.
        if (recordDecl.BaseList is { Types.Count: > 0 })
        {
            // A base type has been defined in the syntax, now we get the type info
            // from the semantic model as the chances are very high that we'll be
            // inheriting from a type.
            var typeInfo = ctx.SemanticModel.GetDeclaredSymbol(recordDecl);

            // If the base type is not System.Object, then it is something user-defined.
            if (typeInfo is { BaseType.SpecialType: not SpecialType.System_Object })
            {
                return null;
            }
        }

        return recordDecl;
    }

    /// <summary>
    ///     Generates the implementation.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="compilation"></param>
    /// <param name="recordDeclarations"></param>
    private static void Execute(
        SourceProductionContext ctx,
        Compilation compilation,
        ImmutableArray<RecordDeclarationSyntax?> recordDeclarations)
    {
        if (recordDeclarations.IsDefaultOrEmpty)
        {
            return;
        }

        var unionsToGenerate = BuildUnionsToGenerate(ctx, compilation, recordDeclarations);
        foreach (var unionToGenerate in unionsToGenerate)
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();
            var (hint, generatedSource) = UnionSyntaxBuilder.GenerateUnionMembers(unionToGenerate);
            ctx.AddSource(hint, generatedSource);
        }
    }

    /// <summary>
    ///     Constructs the unions to generate.
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="compilation"></param>
    /// <param name="recordDeclarations"></param>
    /// <returns></returns>
    private static ImmutableArray<UnionToGenerate> BuildUnionsToGenerate(
        SourceProductionContext ctx,
        Compilation compilation,
        ImmutableArray<RecordDeclarationSyntax?> recordDeclarations)
    {
        var unionsToGenerate = new List<UnionToGenerate>(recordDeclarations.Length);
        foreach (var recordDeclaration in recordDeclarations)
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();
            if (recordDeclaration is null)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(recordDeclaration.SyntaxTree);
            var recordSymbol = semanticModel.GetDeclaredSymbol(recordDeclaration);
            if (recordSymbol is null)
            {
                continue;
            }

            var unionConstructors = GatherUnionConstructors(
                semanticModel,
                recordDeclaration);

            var namespacesToInclude = unionConstructors
                .SelectMany(static c => c.Parameters)
                .Select(static p => p.TypeContainingNamespace)
                .ToImmutableSortedSet();

            unionsToGenerate.Add(
                new UnionToGenerate(
                    accessibility: SyntaxFacts.GetText(recordSymbol.DeclaredAccessibility),
                    unionName: recordSymbol.Name,
                    unionNamespace: recordSymbol.ContainingNamespace.ToDisplayString(),
                    namespacesToInclude: namespacesToInclude,
                    constructors: unionConstructors));
        }

        return [..unionsToGenerate];
    }

    /// <summary>
    ///     Gathers the union constructors.
    /// </summary>
    /// <param name="recordDeclaration"></param>
    /// <param name="semanticModel"></param>
    /// <returns></returns>
    private static ImmutableArray<UnionConstructor> GatherUnionConstructors(
        SemanticModel semanticModel,
        TypeDeclarationSyntax recordDeclaration)
    {
        var constructors = new List<UnionConstructor>(recordDeclaration.Members.Count);
        foreach (var member in recordDeclaration.Members)
        {
            // Ignore non-records, and non-partials.
            if (member is not RecordDeclarationSyntax innerRecordDecl ||
                !innerRecordDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                continue;
            }

            var constructorParameters = ImmutableArray<UnionConstructorParameter>.Empty;
            if (innerRecordDecl.ParameterList is { Parameters.Count: > 0 } parameterList)
            {
                constructorParameters = GatherUnionConstructorParameters(
                    semanticModel,
                    parameterList.Parameters);
            }

            constructors.Add(
                new UnionConstructor(
                    memberName: innerRecordDecl.Identifier.Text,
                    parameters: constructorParameters));
        }

        return [..constructors];
    }

    /// <summary>
    ///     Gathers the union constructor parameters.
    /// </summary>
    /// <param name="semanticModel"></param>
    /// <param name="parameterListSyntax"></param>
    /// <returns></returns>
    private static ImmutableArray<UnionConstructorParameter> GatherUnionConstructorParameters(
        SemanticModel semanticModel,
        SeparatedSyntaxList<ParameterSyntax> parameterListSyntax)
    {
        var constructorParameters = new List<UnionConstructorParameter>(parameterListSyntax.Count);
        foreach (var parameterSyntax in parameterListSyntax)
        {
            var symbol = semanticModel.GetDeclaredSymbol(parameterSyntax);
            if (symbol is null)
            {
                continue;
            }

            constructorParameters.Add(
                new UnionConstructorParameter(
                    typeContainingNamespace: symbol.Type.ContainingNamespace.ToDisplayString(),
                    fullyQualifiedTypeName: symbol.Type.ToDisplayString(
                        SymbolDisplayFormat.FullyQualifiedFormat),
                    parameterName: symbol.Name));
        }

        return constructorParameters.ToImmutableArray();
    }
}

/// <summary>
///     A union to generate.
/// </summary>
internal readonly struct UnionToGenerate(
    string accessibility,
    string unionName,
    string unionNamespace,
    ImmutableSortedSet<string> namespacesToInclude,
    ImmutableArray<UnionConstructor> constructors)
{
    /// <summary>
    ///     Namespaces to include in the generated file.
    /// </summary>
    private static readonly ImmutableSortedSet<string> systemNamespaces = ImmutableSortedSet.Create(
        "System",                           // Needed for Func
        "System.Diagnostics",               // Needed for [DebuggerHidden]
        "System.Diagnostics.CodeAnalysis",  // Needed for [ExcludeFromCodeCoverage]
        "System.Runtime.CompilerServices"); // Needed for [MethodImpl]

    public readonly string Accessibility  = accessibility;
    public readonly string UnionName      = unionName;
    public readonly string UnionNamespace = unionNamespace;

    public readonly ImmutableSortedSet<string> NamespacesToInclude =
        namespacesToInclude.Union(systemNamespaces);

    public readonly ImmutableArray<UnionConstructor> Constructors = constructors;
}

/// <summary>
///     A union member.
/// </summary>
internal readonly struct UnionConstructor(
    string memberName,
    ImmutableArray<UnionConstructorParameter> parameters)
{
    public readonly string                                    MemberName = memberName;
    public readonly ImmutableArray<UnionConstructorParameter> Parameters = parameters;
}

/// <summary>
///     A union constructor parameter.
/// </summary>
internal readonly struct UnionConstructorParameter(
    string typeContainingNamespace,
    string fullyQualifiedTypeName,
    string parameterName)
{
    public readonly string TypeContainingNamespace = typeContainingNamespace;
    public readonly string FullyQualifiedTypeName  = fullyQualifiedTypeName;
    public readonly string ParameterName           = parameterName;
}
