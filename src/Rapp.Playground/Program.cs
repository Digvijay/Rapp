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

using Microsoft.Extensions.Caching.Hybrid;
using Rapp;
using Rapp.Dashboard;
using MemoryPack;
using Rapp.Playground;
using System.Diagnostics.Metrics;
using System.Diagnostics;

// Check for command line arguments
if (args.Length > 0 && args[0] == "--metrics")
{
    // Run command-line metrics viewer
    await RunMetricsViewerAsync();
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serialization for AOT compatibility
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolver = DemoJsonContext.Default;
});

// Add metrics collection service
builder.Services.AddSingleton<MetricsDemoService>();

// builder.Services.AddHybridCache().UseRappForWeatherForecast();
builder.Services.AddHybridCache();
var app = builder.Build();

app.MapRappDashboard();

// Original weather endpoint
app.MapGet("/weather", (HybridCache cache) =>
    cache.GetOrCreateAsync<WeatherForecast>("wx", t => ValueTask.FromResult(new WeatherForecast { Summary = "AOT" })));

// Demo endpoints for schema evolution simulation
app.MapGet("/demo/memorypack-crash", () => SchemaEvolutionDemo.DemonstrateMemoryPackCrash());
app.MapGet("/demo/rapp-safety", () => SchemaEvolutionDemo.DemonstrateRappSafety());
app.MapGet("/demo/json-comparison", () => SchemaEvolutionDemo.DemonstrateJsonBehavior());

// Metrics demonstration endpoints
app.MapGet("/metrics/demo", async (MetricsDemoService metricsService) =>
    await metricsService.DemonstrateMetricsAsync());

app.MapGet("/metrics/stats", (MetricsDemoService metricsService) =>
    metricsService.GetMetricsStats());

app.MapGet("/metrics/reset", (MetricsDemoService metricsService) =>
{
    metricsService.ResetStats();
    return Results.Ok("Metrics reset");
});

// Advanced metrics demos using MetricsDemo class
// Note: These endpoints demonstrate comprehensive metrics patterns
// Uncomment when MetricsDemo class is fully implemented
/*
app.MapGet("/metrics/cache-demo", async (HybridCache cache) =>
    await MetricsDemo.DemonstrateBasicCachingAsync(cache));

app.MapGet("/metrics/custom-demo", async () =>
    await MetricsDemo.DemonstrateCustomMetricsAsync());

app.MapGet("/metrics/benchmark/{iterations}", async (HybridCache cache, int iterations) =>
    await MetricsDemo.RunPerformanceBenchmarkAsync(cache, iterations));

app.MapGet("/metrics/load-test", async (HybridCache cache) =>
    await MetricsDemo.RunLoadTestAsync(cache, concurrentUsers: 5, operationsPerUser: 20));
*/

// Simulated cache operations to generate metrics
app.MapGet("/cache/simulate/{operations}", async (int operations, HybridCache cache, MetricsDemoService metricsService) =>
{
    var results = new List<string>();
    var random = new Random();

    for (int i = 0; i < operations; i++)
    {
        var key = $"sim-{i}";
        var isHit = random.Next(100) < 70; // 70% hit rate

        if (isHit)
        {
            // This will be a cache hit (if data exists)
            var result = await cache.GetOrCreateAsync(key, async ct =>
            {
                await Task.Delay(1); // Simulate work
                return new WeatherForecast { Summary = $"Simulated-{i}" };
            });
            results.Add($"Hit: {result.Summary}");
        }
        else
        {
            // Force cache miss by using unique key
            var uniqueKey = $"{key}-{Guid.NewGuid()}";
            var result = await cache.GetOrCreateAsync(uniqueKey, async ct =>
            {
                await Task.Delay(1); // Simulate work
                return new WeatherForecast { Summary = $"New-{i}" };
            });
            results.Add($"Miss: {result.Summary}");
        }
    }

    return new
    {
        TotalOperations = operations,
        Results = results.Take(10), // Show first 10 results
        Metrics = metricsService.GetMetricsStats()
    };
});

app.Run();

// Command-line metrics viewer
static async Task RunMetricsViewerAsync()
{
    Console.WriteLine("🎯 Rapp Metrics Viewer");
    Console.WriteLine("=====================");
    Console.WriteLine("Monitoring Rapp cache metrics in real-time...");
    Console.WriteLine("Press Ctrl+C to exit\n");

    // Create a metrics collector
    var collector = new CommandLineMetricsCollector();
    collector.Start();

    // Set up a simple cache to generate some metrics
#pragma warning disable ASP0000 // Suppress BuildServiceProvider warning for demo purposes
    var services = new ServiceCollection();
    services.AddHybridCache();
    var provider = services.BuildServiceProvider();
#pragma warning restore ASP0000
    var cache = provider.GetRequiredService<HybridCache>();

    Console.WriteLine("🚀 Starting metrics generation...\n");

    // Generate some initial metrics
    for (int i = 0; i < 5; i++)
    {
        if (i % 2 == 0)
            Rapp.RappMetrics.RecordHit();
        else
            Rapp.RappMetrics.RecordMiss();
        Console.WriteLine($"📊 Generated initial metric {i + 1}/5");
        await Task.Delay(100);
    }

    // Start continuous background operations to generate metrics
    var backgroundCts = new CancellationTokenSource();
    var backgroundTask = Task.Run(async () =>
    {
        var random = new Random();
        int operationCount = 0;
        
        while (!backgroundCts.Token.IsCancellationRequested)
        {
            // Mix of hits and misses - simulate Rapp metrics
            var isHit = random.Next(100) < 70; // 70% hit rate
            
            if (isHit)
            {
                Rapp.RappMetrics.RecordHit();
            }
            else
            {
                Rapp.RappMetrics.RecordMiss();
            }
            
            operationCount++;
            await Task.Delay(500); // Generate operations every 500ms
        }
    }, backgroundCts.Token);

    Console.WriteLine("\n📈 Metrics will update in real-time. Try these commands:");
    Console.WriteLine("  • Run cache operations via HTTP endpoints");
    Console.WriteLine("  • Use the web dashboard at http://localhost:5009/rapp");
    Console.WriteLine("  • Call RappMetrics.RecordHit() or RecordMiss() directly\n");

    // Keep the application running to collect metrics
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (sender, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    try
    {
        await Task.Delay(Timeout.Infinite, cts.Token);
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("\n👋 Shutting down metrics viewer...");
    }
    finally
    {
        backgroundCts.Cancel();
        await backgroundTask;
        collector.Stop();
    }
}

// Command-line metrics collector
class CommandLineMetricsCollector
{
    private MeterListener? _listener;
    private readonly Dictionary<string, long> _metricValues = new();
    private readonly Timer _displayTimer;

    public CommandLineMetricsCollector()
    {
        _displayTimer = new Timer(DisplayMetrics, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        _listener = new MeterListener();
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == "Rapp")
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        _listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            lock (_metricValues)
            {
                // For counters, accumulate the values since each measurement is an increment
                if (instrument is Counter<long>)
                {
                    _metricValues[instrument.Name] = _metricValues.GetValueOrDefault(instrument.Name) + measurement;
                }
                else
                {
                    _metricValues[instrument.Name] = measurement;
                }
            }
        });

        _listener.Start();

        // Start displaying metrics every 2 seconds
        _displayTimer.Change(0, 2000);
    }

    public void Stop()
    {
        _displayTimer.Dispose();
        _listener?.Dispose();
    }

    private void DisplayMetrics(object? state)
    {
        // Clear console and reset cursor for clean display
        Console.Clear();
        Console.SetCursorPosition(0, 0);

        lock (_metricValues)
        {
            Console.WriteLine("🎯 Rapp Metrics Viewer");
            Console.WriteLine("=====================");
            Console.WriteLine("Monitoring Rapp cache metrics in real-time...");
            Console.WriteLine();

            if (_metricValues.Count == 0)
            {
                Console.WriteLine("📊 Waiting for metrics... (Cache operations are running in background)");
                Console.WriteLine();
                Console.WriteLine("💡 Background operations will generate hits and misses automatically");
                return;
            }

            Console.WriteLine("📊 Rapp Cache Metrics (Real-time):");
            Console.WriteLine("===================================");

            foreach (var kvp in _metricValues.OrderBy(x => x.Key))
            {
                var displayName = kvp.Key switch
                {
                    "rapp_cache_hits_total" => "Cache Hits",
                    "rapp_cache_misses_total" => "Cache Misses",
                    _ => kvp.Key
                };

                Console.WriteLine($"  {displayName,-15}: {kvp.Value,8:N0}");
            }

            // Calculate and display hit ratio
            var hits = _metricValues.GetValueOrDefault("rapp_cache_hits_total");
            var misses = _metricValues.GetValueOrDefault("rapp_cache_misses_total");
            var total = hits + misses;

            if (total > 0)
            {
                var hitRatio = (double)hits / total * 100;
                Console.WriteLine($"  Hit Ratio       : {hitRatio,7:N1}%");
            }

            Console.WriteLine();
            Console.WriteLine("💡 Background operations generating metrics every 500ms");
            Console.WriteLine("   Press Ctrl+C to exit");
        }
    }
}

// Original WeatherForecast class (v1.0) - Marked for Rapp caching
// Note: Source generator is implemented but not executing due to .NET 10.0/Roslyn compatibility
[RappCache]
[MemoryPackable]
public partial class WeatherForecast
{
    public string? Summary { get; set; }
}

// Metrics demonstration service
public class MetricsDemoService
{
    private readonly Meter _demoMeter;
    private readonly Counter<long> _demoOperations;
    private readonly Histogram<double> _demoDuration;
    private long _totalOperations;

    public MetricsDemoService()
    {
        _demoMeter = new Meter("Rapp.Demo", "1.0.0");
        _demoOperations = _demoMeter.CreateCounter<long>("rapp_demo_operations_total", "Total demo operations");
        _demoDuration = _demoMeter.CreateHistogram<double>("rapp_demo_operation_duration", "ms", "Duration of demo operations");
    }

    public async Task<object> DemonstrateMetricsAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        // Simulate some work
        await Task.Delay(Random.Shared.Next(10, 100));

        stopwatch.Stop();
        _demoOperations.Add(1);
        _demoDuration.Record(stopwatch.ElapsedMilliseconds);
        Interlocked.Increment(ref _totalOperations);

        return new
        {
            Message = "Metrics operation completed",
            Duration = stopwatch.ElapsedMilliseconds,
            TotalOperations = _totalOperations
        };
    }

    public object GetMetricsStats()
    {
        // Note: In a real application, you'd use a metrics listener to collect these values
        // For demo purposes, we return what we can track internally
        return new
        {
            DemoOperations = _totalOperations,
            RappCacheHits = "Use external metrics collector (Prometheus/OpenTelemetry)",
            RappCacheMisses = "Use external metrics collector (Prometheus/OpenTelemetry)",
            Note = "RappMetrics are published to System.Diagnostics.Metrics and require external collection"
        };
    }

    public void ResetStats()
    {
        _totalOperations = 0;
    }
}
