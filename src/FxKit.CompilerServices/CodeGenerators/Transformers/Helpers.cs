using System.Reflection.Metadata;
using FxKit.CompilerServices.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable LoopCanBeConvertedToQuery

namespace FxKit.CompilerServices.CodeGenerators.Transformers;

/// <summary>
///     Helpers.
/// </summary>
internal static class Helpers
{
    /// <summary>
    ///     Checks whether the specified <paramref name="reference"/>
    ///     contains functor definitions and/or `Map` implementations by scanning for the
    ///     assembly-level `ContainsFunctor` attribute.
    /// </summary>
    /// <param name="reference"></param>
    /// <returns></returns>
    public static bool ContainsFunctors(MetadataReference reference)
    {
        if (reference is PortableExecutableReference pe &&
            pe.GetMetadata() is AssemblyMetadata asmFromPe)
        {
            return AssemblyContainsFunctors(asmFromPe);
        }

        if (reference is CompilationReference ce)
        {
            return AssemblyContainsFunctors(ce);
        }

        return false;
    }

    /// <summary>
    ///     Filters locators based on known functors.
    /// </summary>
    /// <param name="referencedFunctors"></param>
    /// <param name="allFunctorFullyQualifiedMetadataNames"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static IEnumerable<FunctorImplementationLocator> FilterLocatorsBasedOnKnownFunctors(
        IEnumerable<ReferencedFunctors> referencedFunctors,
        ISet<string> allFunctorFullyQualifiedMetadataNames,
        CancellationToken cancellationToken)
    {
        foreach (var referenced in referencedFunctors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var locator in referenced.Locators)
            {
                if (allFunctorFullyQualifiedMetadataNames.Contains(
                        locator.FunctorFullyQualifiedMetadataName))
                {
                    yield return locator;
                }
            }
        }
    }

    /// <summary>
    ///     Converts an enumerable of <see cref="ITypeParameterSymbol" />s to an enumerable of
    ///     <see cref="TypeParameterSyntax" />.
    /// </summary>
    /// <param name="symbols"></param>
    /// <returns></returns>
    public static EquatableArray<string> ToTypeParameterNames(
        IEnumerable<ITypeParameterSymbol> symbols)
        => symbols
            .Select(s => s.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))
            .ToEquatableArray();

    /// <summary>
    ///     Computes a type's fully qualified name without type parameters.
    /// </summary>
    /// <param name="containingNamespace"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static string FullyQualifiedName(string containingNamespace, string typeName) =>
        $"{containingNamespace}.{typeName}";

    /// <summary>
    ///     Checks a compilation reference for whether it contains any functors.
    /// </summary>
    /// <param name="compilationReference"></param>
    /// <returns></returns>
    private static bool AssemblyContainsFunctors(CompilationReference compilationReference)
    {
        foreach (var attributeData in compilationReference.Compilation.Assembly.GetAttributes())
        {
            if (attributeData.AttributeClass is not { } attr)
            {
                continue;
            }

            var name = attr.GetFullyQualifiedMetadataName();
            if (name == TransformerGenerator.ContainsFunctorsAttribute)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Checks a PE reference for whether it contains any functors.
    /// </summary>
    /// <param name="asmMeta"></param>
    /// <returns></returns>
    private static bool AssemblyContainsFunctors(
        AssemblyMetadata asmMeta)
    {
        foreach (var module in asmMeta.GetModules())
        {
            var metadataReader = module.GetMetadataReader();
            foreach (var customAttributeHandle in metadataReader.GetAssemblyDefinition()
                         .GetCustomAttributes())
            {
                var customAttribute = metadataReader.GetCustomAttribute(customAttributeHandle);
                var constructorHandle = customAttribute.Constructor;

                if (constructorHandle.Kind != HandleKind.MemberReference)
                {
                    continue;
                }

                var memberReference =
                    metadataReader.GetMemberReference((MemberReferenceHandle)constructorHandle);
                var containingType =
                    metadataReader.GetTypeReference((TypeReferenceHandle)memberReference.Parent);

                var containingTypeName = metadataReader.GetString(containingType.Name);
                var containingTypeNamespace = metadataReader.GetString(containingType.Namespace);
                var fullName = $"{containingTypeNamespace}.{containingTypeName}";
                if (fullName == TransformerGenerator.ContainsFunctorsAttribute)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
