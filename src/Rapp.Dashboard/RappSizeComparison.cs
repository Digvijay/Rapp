using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Rapp.Dashboard;

/// <summary>
/// Opt-in measurement of how much smaller Rapp's binary payload is than the JSON equivalent,
/// feeding the <c>rapp_bytes_total</c> and <c>json_bytes_equivalent</c> counters the dashboard
/// displays.
/// </summary>
/// <remarks>
/// <para>
/// This lives here, in a diagnostic package, and not in the serializer, because measuring it costs
/// a second serialization plus a reflection-based JSON serialization. Rapp shipped that cost inside
/// every cache operation from 1.1.0 to 1.2.0; see entry 9 in <c>docs/known-issues.md</c>. A cost
/// that large has to be asked for at a call site the author can see, on the values they choose,
/// rather than paid silently on every write and every read.
/// </para>
/// <para>
/// Call it from a cache factory, where it runs once per miss rather than once per operation.
/// </para>
/// </remarks>
public static class RappSizeComparison
{
    /// <summary>
    /// Measures <paramref name="value"/> both ways and records the pair.
    /// </summary>
    /// <typeparam name="T">The cached type.</typeparam>
    /// <param name="value">The value about to be cached.</param>
    /// <param name="serializer">The generated Rapp serializer for <typeparamref name="T"/>.</param>
    /// <returns><paramref name="value"/>, so this can be used inline in a cache factory.</returns>
    [RequiresUnreferencedCode("The JSON half of the comparison serializes by reflection. Measure only in diagnostic builds, or supply a JsonTypeInfo overload.")]
    [RequiresDynamicCode("The JSON half of the comparison serializes by reflection and is not Native AOT safe.")]
    public static T Record<T>(T value, RappBaseSerializer<T> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        var buffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(value, buffer);

        RappMetrics.RecordSerializationSize(
            buffer.WrittenCount,
            JsonSerializer.SerializeToUtf8Bytes(value).Length);

        return value;
    }

    /// <summary>
    /// Native AOT safe overload: the caller supplies the source-generated JSON contract.
    /// </summary>
    /// <typeparam name="T">The cached type.</typeparam>
    /// <param name="value">The value about to be cached.</param>
    /// <param name="serializer">The generated Rapp serializer for <typeparamref name="T"/>.</param>
    /// <param name="jsonTypeInfo">The source-generated JSON contract for <typeparamref name="T"/>.</param>
    /// <returns><paramref name="value"/>, so this can be used inline in a cache factory.</returns>
    public static T Record<T>(T value, RappBaseSerializer<T> serializer, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(serializer);

        var buffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(value, buffer);

        RappMetrics.RecordSerializationSize(
            buffer.WrittenCount,
            JsonSerializer.SerializeToUtf8Bytes(value, jsonTypeInfo).Length);

        return value;
    }
}
