# Rapp Playground

The Rapp Playground is a comprehensive demonstration and testing environment for the Rapp library, showcasing its caching, metrics, and schema evolution capabilities.

## 🚀 Getting Started

### Web Interface (Default)
1. Run the playground:
   ```bash
   cd src/Rapp.Playground
   dotnet run
   ```

2. Open your browser to `http://localhost:5009/rapp` for the dashboard

3. Use the provided `Rapp.Playground.http` file in your REST client (VS Code REST Client, Postman, etc.)

### Command-Line Metrics Viewer
For real-time metrics monitoring in the terminal:

```bash
cd src/Rapp.Playground
dotnet run -- --metrics
```

This launches an interactive metrics viewer that displays Rapp cache metrics in real-time, updating every 2 seconds.

## 📊 Available Endpoints

### Core Functionality

- `GET /weather` - Basic weather endpoint demonstrating Rapp caching
- `GET /rapp` - Web dashboard showing cache performance and metrics

### Metrics Demonstration

- `GET /metrics/demo` - Demonstrates custom metrics collection with timing
- `GET /metrics/stats` - Shows current metrics state
- `POST /metrics/reset` - Clears demo metrics counters

### Cache Simulation

- `GET /cache/simulate/{count}` - Simulates cache operations to generate metrics
  - Example: `/cache/simulate/100` runs 100 cache operations

### Schema Evolution Demos

- `GET /demo/memorypack-crash` - Shows how MemoryPack fails with schema changes
- `GET /demo/rapp-safety` - Shows how Rapp handles schema changes safely
- `GET /demo/json-comparison` - Compares JSON behavior with schema changes

## 📈 Metrics Integration

Rapp automatically collects cache performance metrics using `System.Diagnostics.Metrics`:

### Built-in Metrics

- `rapp_cache_hits_total` - Counter for successful cache retrievals
- `rapp_cache_misses_total` - Counter for cache misses requiring computation

### Production Integration

To collect Rapp metrics in production, configure one of these monitoring solutions:

#### OpenTelemetry + Prometheus

```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(builder => builder
        .AddMeter("Rapp")
        .AddPrometheusExporter());
```

#### Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Command-Line Metrics Viewer

For development and testing, use the built-in command-line metrics viewer:

```bash
dotnet run -- --metrics
```

This provides real-time monitoring of Rapp metrics directly in your terminal, perfect for:
- Development debugging
- Performance testing
- Quick metrics validation
- CI/CD pipeline monitoring

The viewer displays:
- Cache hits and misses in real-time
- Hit ratio calculations
- Live updates every 2 seconds
- Interactive operation guidance

```csharp
using System.Diagnostics.Metrics;

// Create a meter listener
using var listener = new MeterListener();
listener.InstrumentPublished = (instrument, listener) =>
{
    if (instrument.Meter.Name == "Rapp")
    {
        listener.EnableMeasurementEvents(instrument);
    }
};

listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
{
    Console.WriteLine($"{instrument.Name}: {measurement}");
});

listener.Start();
```

## 🧪 Testing Workflows

### Basic Cache Testing

1. Reset metrics: `POST /metrics/reset`
2. Run cache operations: `GET /cache/simulate/50`
3. Check metrics: `GET /metrics/stats`
4. View dashboard: `GET /rapp`

### Schema Evolution Testing

1. Test MemoryPack crash: `GET /demo/memorypack-crash`
2. Test Rapp safety: `GET /demo/rapp-safety`
3. Compare with JSON: `GET /demo/json-comparison`

### Performance Benchmarking

1. Run multiple cache simulations with different sizes
2. Monitor metrics accumulation
3. Use dashboard to visualize performance trends

## 🔧 Configuration

The playground uses the following configuration:

- **HybridCache**: Configured with Rapp serialization for `[RappCache]` attributed classes
- **Metrics**: System.Diagnostics.Metrics with "Rapp" meter
- **Dashboard**: Built-in Rapp dashboard for metrics visualization

## 📝 HTTP File Usage

The `Rapp.Playground.http` file contains comprehensive test scenarios. Use it with:

- **VS Code REST Client** extension
- **Postman** or similar API testing tools
- **curl** commands

The file includes:
- Individual endpoint tests
- Workflow examples
- Performance testing scenarios
- Integration testing sequences

## 🎯 Key Features Demonstrated

1. **Schema-Safe Caching**: Automatic cache invalidation on schema changes
2. **Performance Metrics**: Real-time cache hit/miss tracking
3. **Schema Evolution**: Safe handling of type changes
4. **Dashboard Integration**: Web-based monitoring interface
5. **AOT Compatibility**: Native AOT-ready serialization

## 🐛 Troubleshooting

### Metrics Not Appearing

- Ensure you're using an external metrics collector (Prometheus, OpenTelemetry, etc.)
- Rapp metrics are published to `System.Diagnostics.Metrics` but require a listener

### Dashboard Not Loading

- Verify the application is running on `http://localhost:5009`
- Check browser console for JavaScript errors

### Cache Operations Failing

- Ensure the `[RappCache]` attribute is applied to cached types
- Verify the source generator has processed the types

## 📚 Learn More

- [Rapp Documentation](../../README.md)
- [Schema Evolution Guide](../SchemaEvolutionDemo.cs)
- [Metrics Integration](MetricsDemo.cs)
- [Dashboard Source](../Rapp.Dashboard/)