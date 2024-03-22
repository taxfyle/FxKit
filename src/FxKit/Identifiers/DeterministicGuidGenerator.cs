using System.Security.Cryptography;
using System.Text;

namespace FxKit.Identifiers;

// Adapted from https://github.com/SQLStreamStore/SQLStreamStore/blob/master/src/SqlStreamStore/Infrastructure/DeterministicGuidGenerator.cs
// Modified to use stack-friendly types.
// MIT Licence

/// <summary>
///     A helper utility to generate deterministic GUIDS.
/// </summary>
public class DeterministicGuidGenerator
{
    /// <summary>
    ///     The max size to use with `stackalloc`.
    /// </summary>
    private const int MaxStackSize = 256;

    /// <summary>
    ///     UTF-8-encoded characters vary in length from 1 to 4 bytes.
    /// </summary>
    private const int MaxUtf8BytesPerCharacter = 4;

    /// <summary>
    ///     The namespaces as UTF-8-encoded bytes.
    /// </summary>
    private readonly byte[] _namespaceBytes;

    /// <summary>
    ///     Initializes a new instance of <see cref="DeterministicGuidGenerator" />
    /// </summary>
    /// <param name="guidNameSpace">
    ///     A namespace that ensures that GUIDs generated with this instance
    ///     do not collide with other generators. Your application should define
    ///     its namespace as a constant.
    /// </param>
    public DeterministicGuidGenerator(Guid guidNameSpace)
    {
        _namespaceBytes = guidNameSpace.ToByteArray();
        SwapByteOrder(_namespaceBytes.AsSpan());
    }

    /// <summary>
    ///     Creates a deterministic GUID from another GUID.
    /// </summary>
    /// <param name="source">
    ///     An existing GUID.
    /// </param>
    /// <returns>
    ///     A deterministically generated GUID.
    /// </returns>
    public Guid Create(Guid source)
    {
        Span<byte> buffer = stackalloc byte[16];
        source.TryWriteBytes(buffer);
        return Create(buffer);
    }

    /// <summary>
    ///     Creates a deterministic GUID.
    /// </summary>
    /// <param name="source">
    ///     A source to generate the GUID from.
    /// </param>
    /// <returns>
    ///     A deterministically generated GUID.
    /// </returns>
    public Guid Create(ReadOnlySpan<byte> source)
    {
        var len = source.Length + _namespaceBytes.Length;

        // If the combined length is small enough, allocate
        // a buffer on the stack.
        var concatenated = len > MaxStackSize ? new byte[len] : stackalloc byte[len];

        // Copy the namespace, then the source into the buffer.
        _namespaceBytes.CopyTo(concatenated[.._namespaceBytes.Length]);
        source.CopyTo(concatenated[_namespaceBytes.Length..]);

        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(concatenated, hash);

        Span<byte> newGuid = stackalloc byte[16];

        // A SHA1 hash is 20 bytes, but a Guid is only 16,
        // so we only copy the first 16 bytes.
        hash[..16].CopyTo(newGuid);

        // Set high-nibble to 5 to indicate UUID v5.
        newGuid[6] = (byte)((newGuid[6] & 0x0F) | 0x50);

        // Set the upper 2 bits to 10
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

        SwapByteOrder(newGuid);
        return new Guid(newGuid);
    }

    /// <summary>
    ///     Creates a deterministic GUID from a string.
    /// </summary>
    /// <param name="source">
    ///     The string to generate a GUID for.
    /// </param>
    /// <returns>
    ///     A deterministically generated GUID.
    /// </returns>
    public Guid Create(string source)
    {
        if (source.Length >= MaxStackSize / MaxUtf8BytesPerCharacter)
        {
            // Slow path, allocates an array on the heap.
            return Create(Encoding.UTF8.GetBytes(source));
        }

        // Fast path, use stackalloc.
        Span<byte> buffer = stackalloc byte[source.Length * MaxUtf8BytesPerCharacter];
        var length = Encoding.UTF8.GetBytes(source, buffer);
        return Create(buffer[..length]);
    }

    /// <summary>
    ///     Swaps the byte order of the first 8 bytes
    ///     that represent the `time_low`, `time_mid` and `time_hi_and_version`
    ///     portion of the UUID. This is because Microsoft's specification
    ///     uses little-endian for those first parts of the UUID representation.
    /// </summary>
    /// <param name="guid"></param>
    private static void SwapByteOrder(Span<byte> guid)
    {
        SwapBytes(guid, left: 0, right: 3);
        SwapBytes(guid, left: 1, right: 2);
        SwapBytes(guid, left: 4, right: 5);
        SwapBytes(guid, left: 6, right: 7);
    }

    /// <summary>
    ///     Swaps bytes.
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    private static void SwapBytes(Span<byte> guid, int left, int right) =>
        (guid[left], guid[right]) = (guid[right], guid[left]);
}
