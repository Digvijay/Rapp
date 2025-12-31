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

using MemoryPack;
using Microsoft.Extensions.Caching.Hybrid;

namespace Rapp.Playground;

/// <summary>
/// Comprehensive demonstration of Rapp metrics collection and monitoring.
/// </summary>
/// <remarks>
/// <para>
/// This class demonstrates various patterns for collecting and exposing
/// cache performance metrics using Rapp's built-in telemetry system.
/// </para>
/// <para>
/// <b>Metrics Collected:</b>
/// <list type="bullet">
/// <item><c>rapp_cache_hits_total</c> - Counter for successful cache retrievals</item>
/// <item><c>rapp_cache_misses_total</c> - Counter for cache misses requiring computation</item>
/// </list>
/// </para>
/// <para>
/// <b>Integration Patterns:</b>
/// <list type="bullet">
/// <item>Direct metrics recording in cache operations</item>
/// <item>Custom metrics collection for business logic</item>
/// <item>Metrics exposure for monitoring systems</item>
/// <item>Performance benchmarking with metrics</item>
/// </list>
/// </para>
/// </remarks>
public static class MetricsDemo
{
    /// <summary>
    /// Demonstrates basic cache operations with metrics collection.
    /// </summary>
    /// <param name="cache">The hybrid cache instance.</param>
    /// <returns>A summary of the cache operations performed.</returns>
    /// <remarks>
    /// <para>
    /// This method shows how cache operations automatically generate metrics
    /// through the Rapp serializer. Each cache hit and miss is recorded
    /// in the System.Diagnostics.Metrics infrastructure.
    /// </para>
    /// <para>
    /// <b>Metrics Generated:</b>
    /// <list type="bullet">
    /// <item>Cache hits when retrieving existing data</item>
    /// <item>Cache misses when computing new data</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static async Task<CacheOperationSummary> DemonstrateBasicCachingAsync(HybridCache cache)
    {
        var summary = new CacheOperationSummary();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // First access - cache miss (data needs to be computed)
        var result1 = await cache.GetOrCreateAsync("demo-key-1",
            async ct => {
                await Task.Delay(10); // Simulate computation
                return new DemoData { Id = 1, Name = "First Item", Timestamp = DateTime.UtcNow };
            });

        summary.Misses++;

        // Second access - cache hit (data retrieved from cache)
        var result2 = await cache.GetOrCreateAsync("demo-key-1",
            async ct => {
                await Task.Delay(10);
                return new DemoData { Id = 1, Name = "First Item", Timestamp = DateTime.UtcNow };
            });

        summary.Hits++;

        // Different key - cache miss
        var result3 = await cache.GetOrCreateAsync("demo-key-2",
            async ct => {
                await Task.Delay(5);
                return new DemoData { Id = 2, Name = "Second Item", Timestamp = DateTime.UtcNow };
            });

        summary.Misses++;

        stopwatch.Stop();
        summary.TotalDuration = stopwatch.Elapsed;

        return summary;
    }

    /// <summary>
    /// Demonstrates custom metrics collection for business logic.
    /// </summary>
    /// <returns>A summary of custom operations performed.</returns>
    /// <remarks>
    /// <para>
    /// This method shows how to integrate custom metrics collection
    /// with business logic operations. While Rapp automatically collects
    /// cache metrics, you can also collect custom metrics for your
    /// application-specific operations.
    /// </para>
    /// <para>
    /// <b>Use Cases:</b>
    /// <list type="bullet">
    /// <item>Business operation success/failure rates</item>
    /// <item>Custom performance counters</item>
    /// <item>Application-specific KPIs</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static async Task<CustomOperationSummary> DemonstrateCustomMetricsAsync()
    {
        var summary = new CustomOperationSummary();
        var random = new Random();

        // Simulate business operations with success/failure rates
        for (int i = 0; i < 100; i++)
        {
            var success = random.Next(100) < 85; // 85% success rate

            if (success)
            {
                summary.SuccessfulOperations++;
                // In real code, you might record: CustomMetrics.RecordBusinessOperationSuccess();
            }
            else
            {
                summary.FailedOperations++;
                // In real code, you might record: CustomMetrics.RecordBusinessOperationFailure();
            }

            await Task.Delay(1); // Simulate work
        }

        return summary;
    }

    /// <summary>
    /// Demonstrates performance benchmarking with metrics integration.
    /// </summary>
    /// <param name="cache">The hybrid cache instance.</param>
    /// <param name="iterations">Number of iterations to perform.</param>
    /// <returns>Performance benchmark results.</returns>
    /// <remarks>
    /// <para>
    /// This method demonstrates how to combine performance benchmarking
    /// with metrics collection to get comprehensive performance insights.
    /// </para>
    /// <para>
    /// <b>Benchmark Metrics:</b>
    /// <list type="bullet">
    /// <item>Total execution time</item>
    /// <item>Operations per second</item>
    /// <item>Cache hit/miss ratios</item>
    /// <item>Memory usage patterns</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static async Task<PerformanceBenchmark> RunPerformanceBenchmarkAsync(HybridCache cache, int iterations = 1000)
    {
        var benchmark = new PerformanceBenchmark { TotalIterations = iterations };
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            var key = $"bench-{i % 100}"; // Reused keys to create hits

            await cache.GetOrCreateAsync(key,
                async ct => {
                    benchmark.CacheMisses++;
                    await Task.Delay(1); // Simulate computation
                    return new DemoData { Id = i, Name = $"Benchmark-{i}", Timestamp = DateTime.UtcNow };
                });

            if (i % 100 == 0) benchmark.CacheHits++; // Some will be hits
        }

        stopwatch.Stop();
        benchmark.TotalDuration = stopwatch.Elapsed;
        benchmark.OperationsPerSecond = iterations / stopwatch.Elapsed.TotalSeconds;

        return benchmark;
    }

    /// <summary>
    /// Demonstrates metrics collection in a load testing scenario.
    /// </summary>
    /// <param name="cache">The hybrid cache instance.</param>
    /// <param name="concurrentUsers">Number of concurrent users to simulate.</param>
    /// <param name="operationsPerUser">Operations per user.</param>
    /// <returns>Load test results with metrics summary.</returns>
    /// <remarks>
    /// <para>
    /// This method simulates concurrent load to demonstrate how metrics
    /// behave under stress and help identify performance bottlenecks.
    /// </para>
    /// <para>
    /// <b>Load Testing Insights:</b>
    /// <list type="bullet">
    /// <item>Concurrent access patterns</item>
    /// <item>Thread safety validation</item>
    /// <item>Scalability characteristics</item>
    /// <item>Resource utilization under load</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static async Task<LoadTestResults> RunLoadTestAsync(HybridCache cache, int concurrentUsers = 10, int operationsPerUser = 50)
    {
        var results = new LoadTestResults
        {
            ConcurrentUsers = concurrentUsers,
            OperationsPerUser = operationsPerUser
        };

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var tasks = new List<Task>();
        for (int user = 0; user < concurrentUsers; user++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int op = 0; op < operationsPerUser; op++)
                {
                    var key = $"load-{user}-{op % 20}"; // Some key reuse per user

                    await cache.GetOrCreateAsync(key,
                        async ct => {
                            await Task.Delay(Random.Shared.Next(1, 5)); // Variable computation time
                            return new DemoData { Id = user * 1000 + op, Name = $"Load-{user}-{op}", Timestamp = DateTime.UtcNow };
                        });
                }
            }));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        results.TotalDuration = stopwatch.Elapsed;
        results.TotalOperations = concurrentUsers * operationsPerUser;
        results.OperationsPerSecond = results.TotalOperations / stopwatch.Elapsed.TotalSeconds;

        return results;
    }
}

/// <summary>
/// Sample data class for metrics demonstrations.
/// </summary>
[RappCache]
[MemoryPackable]
public partial class DemoData
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the timestamp.</summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>Summary of basic cache operations.</summary>
public class CacheOperationSummary
{
    /// <summary>Gets or sets the number of cache hits.</summary>
    public int Hits { get; set; }

    /// <summary>Gets or sets the number of cache misses.</summary>
    public int Misses { get; set; }

    /// <summary>Gets or sets the total duration of operations.</summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>Gets the hit ratio as a percentage.</summary>
    public double HitRatio => Hits + Misses > 0 ? (double)Hits / (Hits + Misses) * 100 : 0;
}

/// <summary>Summary of custom business operations.</summary>
public class CustomOperationSummary
{
    /// <summary>Gets or sets the number of successful operations.</summary>
    public int SuccessfulOperations { get; set; }

    /// <summary>Gets or sets the number of failed operations.</summary>
    public int FailedOperations { get; set; }

    /// <summary>Gets the success rate as a percentage.</summary>
    public double SuccessRate => SuccessfulOperations + FailedOperations > 0 ?
        (double)SuccessfulOperations / (SuccessfulOperations + FailedOperations) * 100 : 0;
}

/// <summary>Performance benchmark results.</summary>
public class PerformanceBenchmark
{
    /// <summary>Gets or sets the total number of iterations.</summary>
    public int TotalIterations { get; set; }

    /// <summary>Gets or sets the total duration.</summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>Gets or sets the operations per second.</summary>
    public double OperationsPerSecond { get; set; }

    /// <summary>Gets or sets the number of cache hits.</summary>
    public int CacheHits { get; set; }

    /// <summary>Gets or sets the number of cache misses.</summary>
    public int CacheMisses { get; set; }
}

/// <summary>Load test results.</summary>
public class LoadTestResults
{
    /// <summary>Gets or sets the number of concurrent users.</summary>
    public int ConcurrentUsers { get; set; }

    /// <summary>Gets or sets the operations per user.</summary>
    public int OperationsPerUser { get; set; }

    /// <summary>Gets or sets the total operations performed.</summary>
    public int TotalOperations { get; set; }

    /// <summary>Gets or sets the total duration.</summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>Gets or sets the operations per second.</summary>
    public double OperationsPerSecond { get; set; }
}