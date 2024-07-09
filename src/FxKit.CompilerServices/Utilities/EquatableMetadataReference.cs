using Microsoft.CodeAnalysis;

namespace FxKit.CompilerServices.Utilities;

/// <summary>
///     An equatable <see cref="MetadataReference"/> that weakly references a <see cref="Compilation"/>.
///     Requires that retrieving the compilation is performed immediately following a
///     change to the <see cref="MetadataReference"/> in order to guarantee
///     the <see cref="Compilation"/> has not yet been garbage-collected.
/// </summary>
public sealed class EquatableMetadataReference(
    MetadataReference reference,
    Compilation compilation)
    : IEquatable<EquatableMetadataReference>
{
    /// <summary>
    ///     Weakly references the compilation. Compilations are huge and we don't want to inadvertently
    ///     cache them.
    /// </summary>
    private readonly WeakReference<Compilation> _compilationReference = new(compilation);

    /// <summary>
    ///     Gets the weakly-referenced compilation.
    /// </summary>
    /// <exception cref="NullReferenceException">The compilation was garbage-collected.</exception>
    public Compilation Compilation
    {
        get
        {
            if (_compilationReference.TryGetTarget(out var compilation))
            {
                return compilation;
            }

            throw new NullReferenceException("The compilation was garbage-collected.");
        }
    }

    /// <summary>
    ///     The metadata reference.
    /// </summary>
    public MetadataReference Reference { get; } = reference;

    /// <inheritdoc />
    public bool Equals(EquatableMetadataReference? other)
    {
        if (other is null)
        {
            return false;
        }

        // Only PE references can be checked for equality; others are assumed not equal.
        if (this.Reference is not PortableExecutableReference left ||
            other.Reference is not PortableExecutableReference right)
        {
            return false;
        }

        return left.GetMetadataId() == right.GetMetadataId();
    }
}
