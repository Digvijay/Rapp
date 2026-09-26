using System.Buffers;
using System.Diagnostics.Metrics;
using Xunit;

namespace Rapp.Tests;

/// <summary>
/// From 1.1.0 to 1.2.0 the shipped library serialized every cached value a second time, and again
/// to JSON by reflection, to populate a size-comparison counter — on every write and every read,
/// with the result discarded. See <c>docs/known-issues.md</c> entry 9.
/// </summary>
/// <remarks>
/// The allocation budgets in <c>PerformanceRegressionTests</c> would catch a regression by its
/// cost. This catches it by its signature, which is what the defect actually was: the library
/// emitting a measurement nobody asked it for.
/// </remarks>
public class TelemetryOverheadTests
{
    [Fact]
    public void The_library_never_emits_the_json_size_comparison()
    {
        var recorded = new List<(string Instrument, long Value)>();

        // A MeterListener is process-global, and RappMetricsTests exercises the public
        // RecordSerializationSize API on a parallel xUnit thread. Only measurements raised on this
        // thread can have come from the serializer calls below, because those calls are synchronous.
        var thisThread = Environment.CurrentManagedThreadId;

        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == "Rapp")
            {
                l.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, _, _) =>
        {
            if (Environment.CurrentManagedThreadId != thisThread)
            {
                return;
            }

            lock (recorded)
            {
                recorded.Add((instrument.Name, value));
            }
        });
        listener.Start();

        var serializer = new ProbeSerializer();
        var buffer = new ArrayBufferWriter<byte>();
        for (var i = 0; i < 100; i++)
        {
            buffer.Clear();
            serializer.Serialize($"payload-{i}", buffer);
            serializer.Deserialize(new ReadOnlySequence<byte>(buffer.WrittenMemory));
        }

        listener.RecordObservableInstruments();

        var sizeMetrics = recorded
            .Where(r => r.Instrument is "json_bytes_equivalent" or "rapp_bytes_total")
            .ToArray();

        Assert.True(sizeMetrics.Length == 0,
            $"Rapp emitted {sizeMetrics.Length} size measurements over 100 round trips, starting with " +
            string.Join(", ", sizeMetrics.Take(4).Select(m => $"{m.Instrument}={m.Value}")) +
            ". The size comparison costs a second serialization plus a reflection JSON serialization " +
            "and must be opt-in at a call site (Rapp.Dashboard's RappSizeComparison), never paid by " +
            "the library on every operation.");
    }

    private sealed class ProbeSerializer : RappBaseSerializer<string>
    {
        private static readonly byte[] HashBytes = BitConverter.GetBytes(123456789UL);
        protected override ulong SchemaHash => 123456789UL;
        protected override string TypeName => "string";
        protected override ReadOnlySpan<byte> GetSchemaHashBytes() => HashBytes;
    }
}
