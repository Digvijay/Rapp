# Telemetry Configuration

Rapp provides optional telemetry for monitoring cache performance. **Telemetry is excluded from the NuGet package by default** to ensure zero overhead in production deployments.

## Performance-First Design

The NuGet package you install has **zero telemetry overhead**:
- No metric collection
- No counters
- No instrumentation code paths
- Pure serialization performance

## Enabling Telemetry (Optional)

To enable telemetry in your application, add the `RAPP_TELEMETRY` compilation symbol:

### Option 1: Project File

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);RAPP_TELEMETRY</DefineConstants>
</PropertyGroup>
```

### Option 2: Build Command

```bash
dotnet build -p:DefineConstants="RAPP_TELEMETRY"
```

### Option 3: Configuration-Based

```xml
<!-- Enable only in Development -->
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DefineConstants>$(DefineConstants);RAPP_TELEMETRY</DefineConstants>
</PropertyGroup>
```

## Available Metrics

When telemetry is enabled, Rapp exposes the following metrics via `System.Diagnostics.Metrics`:

### Cache Operations

```csharp
// Counter: rapp_cache_hits_total
RappMetrics.RecordHit();

// Counter: rapp_cache_misses_total
RappMetrics.RecordMiss();

// Counter: rapp_bytes_total
// Counter: json_bytes_equivalent
// Records serialization sizes for cost analysis
RappMetrics.RecordSerializationSize(binarySize, jsonSize);
```

### Cost Analysis Metrics

When `RAPP_TELEMETRY` is enabled, Rapp also calculates:
- **rapp_bytes_total**: Total bytes serialized/deserialized using Rapp
- **json_bytes_equivalent**: Equivalent size if JSON were used

> ⚠️ **Performance Warning**: Calculating `json_bytes_equivalent` requires double-serialization (serializing to JSON in parallel) to measure the size difference. This introduces significant overhead and should **only be used for analysis/debugging**, never in latency-critical production paths.

### Schema Validation (Internal)

These are recorded automatically by `RappBaseSerializer`:

- **Deserialization errors**: Data too short, corruption, etc.
- **Schema mismatches**: Type definition changed between serialization and deserialization

## Runtime Configuration

Even with telemetry compiled in, you can control it at runtime:

```csharp
// Disable telemetry at runtime (reduces overhead)
RappConfiguration.EnableTelemetry = false;

// Enable detailed error information (development only)
RappConfiguration.EnableDetailedErrors = true;

// Throw exceptions on schema mismatch (testing only)
RappConfiguration.ThrowOnSchemaMismatch = true;
```

## Collecting Metrics

When telemetry is enabled, use standard .NET monitoring tools:

### Prometheus

```csharp
// Using OpenTelemetry
services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddMeter("Rapp")
        .AddPrometheusExporter());
```

### Application Insights

```csharp
services.AddApplicationInsightsTelemetry();
// Rapp metrics are automatically collected
```

### Custom Listeners

```csharp
var listener = new MeterListener();
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

## Performance Impact

### Without RAPP_TELEMETRY (Default)
- **Serialization overhead**: 0% (metrics code removed at compile time)
- **Binary size**: Smallest possible
- **JIT optimization**: Maximum (no conditional branches for metrics)

### With RAPP_TELEMETRY
- **Serialization overhead**: <0.5% (simple counter increments)
- **Binary size**: +2-3 KB for metrics infrastructure
- **JIT optimization**: Minimal impact with `EnableTelemetry = false`

## Best Practices

### Production Deployments

**Recommended**: Do not define `RAPP_TELEMETRY`

```xml
<!-- Production config - NO telemetry -->
<PropertyGroup>
  <!-- No RAPP_TELEMETRY defined -->
</PropertyGroup>
```

Benefits:
- Absolute minimum overhead
- Smallest binary size
- Maximum AOT optimization

### Development/Staging

**Recommended**: Enable telemetry conditionally

```xml
<PropertyGroup Condition="'$(Configuration)' != 'Release'">
  <DefineConstants>$(DefineConstants);RAPP_TELEMETRY</DefineConstants>
</PropertyGroup>
```

### Observability Requirements

If your production environment requires observability:

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);RAPP_TELEMETRY</DefineConstants>
</PropertyGroup>
```

Then control at runtime:

```csharp
// Only enable in monitoring endpoints
app.MapGet("/metrics", () => 
{
    RappConfiguration.EnableTelemetry = true;
    // Return metrics
});
```

## Dashboard Support

The `Rapp.Dashboard` package requires telemetry. When using the dashboard:

```csharp
// Install Rapp.Dashboard (it automatically enables RAPP_TELEMETRY)
dotnet add package Rapp.Dashboard
```

The dashboard project includes telemetry by default for visualization.

## Verifying Telemetry Status

Check if telemetry is compiled in:

```csharp
#if RAPP_TELEMETRY
    Console.WriteLine("Telemetry: ENABLED");
#else
    Console.WriteLine("Telemetry: DISABLED (zero overhead)");
#endif
```

## Migration from Other Libraries

### From MemoryPack with Custom Metrics

**Before:**
```csharp
public class MySerializer : IHybridCacheSerializer<T>
{
    public void Serialize(T value, IBufferWriter<byte> target)
    {
        _metrics.RecordSerialization();  // Always runs
        MemoryPackSerializer.Serialize(target, value);
    }
}
```

**After (with Rapp):**
```csharp
// No code changes needed!
// Metrics are optional via RAPP_TELEMETRY
[RappCache]
[MemoryPackable]
public partial class MyClass { }
```

### From JSON with Built-in Metrics

JSON serializers often include metrics that can't be disabled. Rapp gives you the choice:

- **Need metrics?** Define `RAPP_TELEMETRY`
- **Want zero overhead?** Don't define it (default)

## FAQ

### Q: Will my NuGet package include metrics code?
**A:** No. The published NuGet package has zero metrics overhead by default.

### Q: Can I enable metrics without recompiling?
**A:** No. Metrics are a compile-time feature for maximum performance. This ensures production code has no overhead.

### Q: What's the performance difference?
**A:** In production (no `RAPP_TELEMETRY`): 0% overhead  
In development (with `RAPP_TELEMETRY`): <0.5% overhead

### Q: Does AOT work with telemetry?
**A:** Yes. Both configurations (with and without telemetry) are fully AOT-compatible.

### Q: Can I use different settings per environment?
**A:** Yes. Use conditional compilation in your project file based on `$(Configuration)`.

## Examples

See the `/Samples` directory for complete examples:
- All samples have `RAPP_TELEMETRY` enabled for demonstration
- Production apps should remove this for zero overhead

## Summary

| Configuration | Overhead | Use Case |
|--------------|----------|----------|
| No `RAPP_TELEMETRY` (default) | 0% | Production deployments |
| With `RAPP_TELEMETRY` + `EnableTelemetry=false` | <0.1% | Development with opt-out |
| With `RAPP_TELEMETRY` + `EnableTelemetry=true` | <0.5% | Development/staging monitoring |

**Default recommendation**: Don't define `RAPP_TELEMETRY` in production for absolute maximum performance.
