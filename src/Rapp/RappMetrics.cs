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

using System.Diagnostics.Metrics;

namespace Rapp;

/// <summary>
/// Provides telemetry and metrics collection for Rapp cache operations.
/// </summary>
/// <remarks>
/// <para>
/// This static class exposes cache performance metrics using .NET's System.Diagnostics.Metrics
/// infrastructure. It tracks cache hits and misses to help monitor cache effectiveness
/// and performance characteristics.
/// </para>
/// <para>
/// <b>Metrics Exposed:</b>
/// <list type="bullet">
/// <item><c>rapp_cache_hits_total</c> - Counter for successful cache retrievals</item>
/// <item><c>rapp_cache_misses_total</c> - Counter for cache misses requiring computation</item>
/// </list>
/// </para>
/// <para>
/// <b>Usage:</b> Call <see cref="RecordHit"/> and <see cref="RecordMiss"/> from your
/// cache implementation to track performance. These metrics can be collected by
/// monitoring systems like Prometheus, Application Insights, or OpenTelemetry.
/// </para>
/// <para>
/// <b>Thread Safety:</b> All methods are thread-safe and can be called concurrently
/// from multiple threads without synchronization.
/// </para>
/// </remarks>
public static class RappMetrics
{
    /// <summary>
    /// The metrics meter used for collecting Rapp telemetry data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This meter is registered with the name "Rapp" and provides the namespace
    /// for all Rapp-related metrics. It follows OpenTelemetry naming conventions.
    /// </para>
    /// <para>
    /// The meter is initialized once and reused for all metric operations.
    /// </para>
    /// </remarks>
    private static readonly Meter Meter = new("Rapp");

    /// <summary>
    /// Counter for tracking total cache hits across all Rapp cache operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This counter is incremented each time a cache lookup successfully
    /// retrieves a value, indicating effective cache utilization.
    /// </para>
    /// <para>
    /// <b>Metric Name:</b> <c>rapp_cache_hits_total</c><br/>
    /// <b>Unit:</b> Count (dimensionless)<br/>
    /// <b>Description:</b> Total number of cache hits
    /// </para>
    /// </remarks>
    private static readonly Counter<long> CacheHits = Meter.CreateCounter<long>("rapp_cache_hits_total", "Total number of cache hits");

    /// <summary>
    /// Counter for tracking total cache misses across all Rapp cache operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This counter is incremented each time a cache lookup fails to find
    /// a value, requiring the underlying computation or data retrieval.
    /// </para>
    /// <para>
    /// <b>Metric Name:</b> <c>rapp_cache_misses_total</c><br/>
    /// <b>Unit:</b> Count (dimensionless)<br/>
    /// <b>Description:</b> Total number of cache misses
    /// </para>
    /// </remarks>
    private static readonly Counter<long> CacheMisses = Meter.CreateCounter<long>("rapp_cache_misses_total", "Total number of cache misses");

    /// <summary>
    /// Records a cache hit event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call this method when a cache lookup successfully retrieves a value.
    /// This helps track cache effectiveness and hit ratios.
    /// </para>
    /// <para>
    /// <b>Thread Safety:</b> This method is thread-safe and can be called
    /// concurrently from multiple threads.
    /// </para>
    /// <para>
    /// <b>Performance:</b> This method has minimal overhead and is designed
    /// for high-frequency calling in performance-critical cache operations.
    /// </para>
    /// </remarks>
    public static void RecordHit() => CacheHits.Add(1);

    /// <summary>
    /// Records a cache miss event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Call this method when a cache lookup fails to find a value, requiring
    /// the underlying computation or data retrieval to be performed.
    /// </para>
    /// <para>
    /// <b>Thread Safety:</b> This method is thread-safe and can be called
    /// concurrently from multiple threads.
    /// </para>
    /// <para>
    /// <b>Performance:</b> This method has minimal overhead and is designed
    /// for high-frequency calling in performance-critical cache operations.
    /// </para>
    /// </remarks>
    public static void RecordMiss() => CacheMisses.Add(1);

    /// <summary>
    /// Records a deserialization error event.
    /// </summary>
    /// <param name="errorType">The type of error that occurred.</param>
    /// <param name="typeName">The name of the type being deserialized.</param>
    /// <remarks>
    /// Used internally by Rapp to track deserialization failures for monitoring.
    /// </remarks>
    internal static void RecordDeserializationError(string errorType, string typeName)
    {
        if (!RappConfiguration.EnableTelemetry) return;
        // Future: Emit metric for deserialization errors
    }

    /// <summary>
    /// Records a schema mismatch event.
    /// </summary>
    /// <param name="typeName">The name of the type with mismatched schema.</param>
    /// <param name="expectedHash">The expected schema hash.</param>
    /// <param name="actualHash">The actual schema hash found.</param>
    /// <remarks>
    /// Used internally by Rapp to track schema validation failures for monitoring.
    /// </remarks>
    internal static void RecordSchemaMismatch(string typeName, ulong expectedHash, ulong actualHash)
    {
        if (!RappConfiguration.EnableTelemetry) return;
        // Future: Emit metric for schema mismatches
    }
}

#if RAPP_TELEMETRY
/// <summary>
/// Interface for collecting Rapp cache metrics in test and development scenarios.
/// </summary>
/// <remarks>
/// <para>
/// This interface is only available when the <c>RAPP_TELEMETRY</c> compilation
/// symbol is defined. It provides a testable abstraction for metrics collection
/// that can be mocked in unit tests.
/// </para>
/// <para>
/// <b>Usage:</b> Implement this interface in test scenarios where you need
/// to verify that metrics are being recorded correctly.
/// </para>
/// </remarks>
public interface IRappMetricsCollector
{
    /// <summary>
    /// Gets the current cache statistics.
    /// </summary>
    /// <returns>
    /// A tuple containing the total number of cache hits and misses
    /// recorded since the collector was created.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method returns a snapshot of the current metrics state.
    /// The returned values represent cumulative counts since instantiation.
    /// </para>
    /// <para>
    /// <b>Thread Safety:</b> The returned values are consistent but may
    /// not reflect the most recent operations if called concurrently.
    /// </para>
    /// </remarks>
    (long Hits, long Misses) GetStats();
}

/// <summary>
/// Testable implementation of metrics collection for development and testing.
/// </summary>
/// <remarks>
/// <para>
/// This class provides a testable alternative to the static <see cref="RappMetrics"/>
/// class. It maintains internal counters that can be inspected for testing purposes.
/// </para>
/// <para>
/// <b>Thread Safety:</b> All operations are thread-safe using atomic operations.
/// Multiple threads can record hits and misses concurrently.
/// </para>
/// <para>
/// <b>Usage:</b> Use this class in unit tests or development scenarios where
/// you need to verify metrics collection behavior.
/// </para>
/// </remarks>
public class RappMetricsCollector : IRappMetricsCollector
{
    /// <summary>
    /// Internal counter for cache hits.
    /// </summary>
    /// <remarks>
    /// This field is updated atomically to ensure thread safety.
    /// </remarks>
    private long _hits;

    /// <summary>
    /// Internal counter for cache misses.
    /// </summary>
    /// <remarks>
    /// This field is updated atomically to ensure thread safety.
    /// </remarks>
    private long _misses;

    /// <summary>
    /// Records a cache hit event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Atomically increments the internal hit counter. This operation
    /// is thread-safe and can be called concurrently.
    /// </para>
    /// <para>
    /// <b>Performance:</b> Uses <see cref="Interlocked.Increment(ref long)"/>
    /// for atomic updates with minimal overhead.
    /// </para>
    /// </remarks>
    public void RecordHit() => Interlocked.Increment(ref _hits);

    /// <summary>
    /// Records a cache miss event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Atomically increments the internal miss counter. This operation
    /// is thread-safe and can be called concurrently.
    /// </para>
    /// <para>
    /// <b>Performance:</b> Uses <see cref="Interlocked.Increment(ref long)"/>
    /// for atomic updates with minimal overhead.
    /// </para>
    /// </remarks>
    public void RecordMiss() => Interlocked.Increment(ref _misses);

    /// <summary>
    /// Gets the current cache statistics.
    /// </summary>
    /// <returns>
    /// A tuple containing the total number of cache hits and misses
    /// recorded since this instance was created.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Returns a consistent snapshot of the current hit and miss counts.
    /// The values represent cumulative totals since instantiation.
    /// </para>
    /// <para>
    /// <b>Thread Safety:</b> The returned values are read atomically but
    /// may not include the most recent concurrent operations.
    /// </para>
    /// </remarks>
    public (long Hits, long Misses) GetStats() => (_hits, _misses);
}
#endif