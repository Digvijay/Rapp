using System.Buffers;
using MemoryPack;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Rapp;

namespace Rapp.AotProbe;

/// <summary>
/// A Native AOT probe for Rapp's shipping surface.
/// </summary>
/// <remarks>
/// <para>
/// This program is published with <c>PublishAot=true</c> and then executed by CI. Publishing alone
/// proves only that ILC was willing to compile the code; running it is what proves the reflective
/// fallbacks inside MemoryPack are never actually reached, because if they were, a Native AOT
/// binary would throw rather than silently degrade.
/// </para>
/// <para>
/// Everything here goes through the public package surface: the <c>[RappCache]</c> attribute, the
/// generated <c>UseRappFor…</c> registration, the generated serializer, and <c>HybridCache</c>.
/// Nothing reaches into Rapp's internals, because the point is to exercise what a consumer gets.
/// </para>
/// </remarks>
internal static class Program
{
    private static int _failures;

    private static async Task<int> Main()
    {
        Console.WriteLine("Rapp Native AOT probe");
        Console.WriteLine("=====================");
        Console.WriteLine();

        await ProbeHybridCacheRoundTripAsync().ConfigureAwait(false);
        ProbeGeneratedSerializerRoundTrip();
        ProbeSchemaHashHeader();
        ProbeEmptyAndBoundaryValues();

        Console.WriteLine();
        if (_failures == 0)
        {
            Console.WriteLine("All AOT probes passed.");
            return 0;
        }

        Console.Error.WriteLine($"{_failures} AOT probe(s) failed.");
        return 1;
    }

    /// <summary>
    /// Registers the generated serializer with HybridCache and round-trips a value through it.
    /// </summary>
    private static async Task ProbeHybridCacheRoundTripAsync()
    {
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForProbePayload();

        using var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var created = await cache.GetOrCreateAsync(
            "probe-1",
            _ => ValueTask.FromResult(NewPayload())).ConfigureAwait(false);

        Check("HybridCache populates on miss", Matches(created, NewPayload()));

        // The second call must come back from the cache, which means it was serialized by the
        // generated serializer and deserialized again — the path this probe exists to exercise.
        var cached = await cache.GetOrCreateAsync(
            "probe-1",
            _ => ValueTask.FromResult(new ProbePayload { Id = -1, Name = "factory ran again" }))
            .ConfigureAwait(false);

        Check("HybridCache round-trips through the Rapp serializer", Matches(cached, NewPayload()));
    }

    /// <summary>
    /// Uses the generated serializer directly, the way a consumer holding one would.
    /// </summary>
    private static void ProbeGeneratedSerializerRoundTrip()
    {
        var serializer = new ProbePayloadRappSerializer();
        var original = NewPayload();

        var buffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(original, buffer);

        var restored = serializer.Deserialize(new ReadOnlySequence<byte>(buffer.WrittenMemory));

        Check("Generated serializer round-trips", Matches(restored, original));
    }

    /// <summary>
    /// Confirms the eight-byte schema hash header is present and precedes the payload.
    /// </summary>
    private static void ProbeSchemaHashHeader()
    {
        var serializer = new ProbePayloadRappSerializer();
        var buffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(NewPayload(), buffer);

        var written = buffer.WrittenSpan;
        Check("Payload carries the 8-byte schema header", written.Length > 8);

        var body = MemoryPackSerializer.Serialize(NewPayload());
        Check(
            "Header is exactly 8 bytes ahead of the MemoryPack body",
            written.Length == body.Length + 8);
    }

    /// <summary>
    /// Exercises the values most likely to fall back to a reflective formatter.
    /// </summary>
    /// <remarks>
    /// Empty strings, empty collections and default values are where a serializer is most likely to
    /// reach for a generic formatter it did not source-generate. Under Native AOT that is a crash
    /// rather than a warning, which is precisely why this runs rather than only compiles.
    /// </remarks>
    private static void ProbeEmptyAndBoundaryValues()
    {
        var serializer = new ProbePayloadRappSerializer();

        var edge = new ProbePayload
        {
            Id = 0,
            Name = string.Empty,
            Tags = [],
            CreatedAt = default,
        };

        var buffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(edge, buffer);
        var restored = serializer.Deserialize(new ReadOnlySequence<byte>(buffer.WrittenMemory));

        Check("Empty and default values round-trip", Matches(restored, edge));

        var many = new ProbePayload
        {
            Id = int.MaxValue,
            Name = new string('x', 4096),
            Tags = [.. Enumerable.Range(0, 256).Select(i => $"tag-{i}")],
            CreatedAt = DateTime.UnixEpoch,
        };

        var manyBuffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(many, manyBuffer);
        var manyRestored = serializer.Deserialize(new ReadOnlySequence<byte>(manyBuffer.WrittenMemory));

        Check("Large payloads and collections round-trip", Matches(manyRestored, many));
    }

    private static ProbePayload NewPayload() => new()
    {
        Id = 42,
        Name = "probe",
        Tags = ["alpha", "beta"],
        CreatedAt = DateTime.UnixEpoch,
    };

    private static bool Matches(ProbePayload? actual, ProbePayload expected) =>
        actual is not null
        && actual.Id == expected.Id
        && actual.Name == expected.Name
        && actual.CreatedAt == expected.CreatedAt
        && actual.Tags.Count == expected.Tags.Count
        && !actual.Tags.Where((t, i) => t != expected.Tags[i]).Any();

    private static void Check(string description, bool condition)
    {
        if (condition)
        {
            Console.WriteLine($"  PASS  {description}");
            return;
        }

        _failures++;
        Console.Error.WriteLine($"  FAIL  {description}");
    }
}

/// <summary>
/// The payload the probe serializes. Deliberately covers a value type, a string, a collection and
/// a <see cref="DateTime"/>, which between them cover the formatter shapes a consumer will hit.
/// </summary>
[RappCache]
[MemoryPackable]
public partial class ProbePayload
{
    /// <summary>Gets or sets an integral value.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets a string value.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a collection, the shape most likely to need a generic formatter.</summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>Gets or sets a struct value.</summary>
    public DateTime CreatedAt { get; set; }
}
