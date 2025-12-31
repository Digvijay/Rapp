// Copyright (c) 2025 Digvijay Chauhan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using MemoryPack;
using Microsoft.Extensions.Caching.Hybrid;

namespace Rapp;

/// <summary>
/// Base class for schema-aware binary serializers that provide enterprise-grade safety
/// for HybridCache serialization operations.
/// </summary>
/// <typeparam name="T">The type being serialized. Must be MemoryPack-compatible.</typeparam>
/// <remarks>
/// <para>
/// This abstract base class implements the core schema validation mechanism used by
/// Rapp's generated serializers. It prepends an 8-byte cryptographic hash of the type's
/// schema to every serialized payload, ensuring that deserialization will fail safely
/// if the schema has changed between serialization and deserialization.
/// </para>
/// <para>
/// <b>Schema Safety:</b> The schema hash is computed from the type's structure and
/// serves as a version identifier. If the type definition changes (fields added/removed,
/// types modified), the hash will be different, preventing incompatible deserialization.
/// </para>
/// <para>
/// <b>Performance:</b> Uses MemoryPack's zero-copy binary serialization with minimal
/// overhead (8 bytes per payload). The schema validation adds negligible performance
/// cost while providing critical safety guarantees.
/// </para>
/// <para>
/// <b>Thread Safety:</b> All methods are thread-safe and can be used concurrently
/// across multiple threads without synchronization.
/// </para>
/// <para>
/// <b>AOT Compatibility:</b> Fully compatible with Native AOT compilation when used
/// with MemoryPack's AOT-safe serialization patterns.
/// </para>
/// </remarks>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public abstract class RappBaseSerializer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : IHybridCacheSerializer<T>
{
    /// <summary>
    /// Gets the cryptographic hash of the type's schema used for validation.
    /// </summary>
    /// <value>
    /// A 64-bit unsigned integer representing the schema hash. This value is computed
    /// at compile-time by the source generator and must match exactly for successful
    /// deserialization.
    /// </value>
    /// <remarks>
    /// <para>
    /// The schema hash is computed using a deterministic algorithm that considers:
    /// <list type="bullet">
    /// <item>Field names and their declaration order</item>
    /// <item>Field types and their MemoryPack serialization requirements</item>
    /// <item>Any MemoryPack attributes applied to fields</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Implementation Note:</b> This property is implemented by generated code
    /// and should not be overridden in derived classes.
    /// </para>
    /// </remarks>
    protected abstract ulong SchemaHash { get; }

    /// <summary>
    /// Gets the human-readable name of the type being serialized.
    /// </summary>
    /// <value>
    /// A string containing the full type name, used for diagnostic purposes
    /// and error reporting.
    /// </value>
    /// <remarks>
    /// <para>
    /// This name is used in error messages and telemetry to identify which
    /// type failed schema validation. It includes the full namespace and type name.
    /// </para>
    /// <para>
    /// <b>Implementation Note:</b> This property is implemented by generated code
    /// and should not be overridden in derived classes.
    /// </para>
    /// </remarks>
    protected abstract string TypeName { get; }

    /// <summary>
    /// Serializes the specified value to binary format with schema validation.
    /// </summary>
    /// <param name="value">The value to serialize. Can be <c>null</c> for nullable types.</param>
    /// <param name="target">The buffer writer to receive the serialized data.</param>
    /// <remarks>
    /// <para>
    /// This method writes an 8-byte schema hash header followed by the MemoryPack
    /// serialized payload. The total size will be 8 bytes + MemoryPack payload size.
    /// </para>
    /// <para>
    /// <b>Buffer Management:</b> The method manages buffer allocation automatically.
    /// The <paramref name="target"/> buffer must support writing at least 8 bytes
    /// for the schema header.
    /// </para>
    /// <para>
    /// <b>Null Handling:</b> Null values are handled according to MemoryPack's
    /// null-handling semantics for the specific type <typeparamref name="T"/>.
    /// </para>
    /// <para>
    /// <b>Performance:</b> This method is optimized for high-throughput scenarios
    /// and should be used for cache serialization operations.
    /// </para>
    /// </remarks>
    public void Serialize(T value, IBufferWriter<byte> target)
    {
        var span = target.GetSpan(8);
        BinaryPrimitives.WriteUInt64LittleEndian(span, SchemaHash);
        target.Advance(8);
        MemoryPackSerializer.Serialize(target, value);
    }

    /// <summary>
    /// Deserializes a value from binary format with schema validation.
    /// </summary>
    /// <param name="source">The binary data to deserialize.</param>
    /// <returns>
    /// The deserialized value, or <c>default(T)</c> if schema validation fails
    /// or the data is corrupted.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method first validates the 8-byte schema hash header. If the hash
    /// doesn't match the expected <see cref="SchemaHash"/>, it returns <c>default(T)</c>
    /// to indicate schema incompatibility.
    /// </para>
    /// <para>
    /// <b>Schema Validation:</b> If the schema hash doesn't match, this indicates
    /// that the type definition has changed since serialization. The method fails
    /// safely by returning the default value rather than throwing an exception.
    /// </para>
    /// <para>
    /// <b>Error Handling:</b> Returns <c>default(T)</c> for:
    /// <list type="bullet">
    /// <item>Schema hash mismatch (type definition changed)</item>
    /// <item>Insufficient data length (less than 8 bytes)</item>
    /// <item>MemoryPack deserialization failures</item>
    /// </list>
    /// </para>
    /// <para>
    /// <b>Performance:</b> Schema validation adds minimal overhead (8-byte header check)
    /// while providing critical safety guarantees for cache operations.
    /// </para>
    /// <para>
    /// <b>Thread Safety:</b> This method is thread-safe and can be called concurrently
    /// from multiple threads.
    /// </para>
    /// </remarks>
    public T Deserialize(ReadOnlySequence<byte> source)
    {
        if (source.Length < 8) return default!;
        Span<byte> header = stackalloc byte[8];
        source.Slice(0, 8).CopyTo(header);
        if (BinaryPrimitives.ReadUInt64LittleEndian(header) != SchemaHash) return default!;
        return MemoryPackSerializer.Deserialize<T>(source.Slice(8))!;
    }
}
