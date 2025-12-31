# Telemetry Quick Reference

## Default Behavior (Production)

**The NuGet package has ZERO telemetry overhead by default.**

```bash
dotnet add package Rapp
# No telemetry included - maximum performance ✅
```

## Enabling Telemetry (Development/Monitoring)

Add to your `.csproj`:

```xml
<PropertyGroup>
  <DefineConstants>$(DefineConstants);RAPP_TELEMETRY</DefineConstants>
</PropertyGroup>
```

## Configuration

```csharp
// Control at runtime (when RAPP_TELEMETRY is defined)
RappConfiguration.EnableTelemetry = false;  // Disable metrics
RappConfiguration.EnableDetailedErrors = false;  // Simple errors (production)
RappConfiguration.ThrowOnSchemaMismatch = false;  // Graceful degradation
```

## Available Metrics

When `RAPP_TELEMETRY` is defined:

```csharp
RappMetrics.RecordHit();    // Counter: rapp_cache_hits_total
RappMetrics.RecordMiss();   // Counter: rapp_cache_misses_total
```

## Performance Impact

| Configuration | Overhead | Recommendation |
|--------------|----------|----------------|
| No `RAPP_TELEMETRY` | **0%** | ✅ Production |
| With `RAPP_TELEMETRY` | <0.5% | Development/Staging |

## Conditional by Environment

```xml
<!-- Enable only in non-Release builds -->
<PropertyGroup Condition="'$(Configuration)' != 'Release'">
  <DefineConstants>$(DefineConstants);RAPP_TELEMETRY</DefineConstants>
</PropertyGroup>
```

## Complete Documentation

See [docs/TELEMETRY.md](TELEMETRY.md) for full details.

## Key Points

✅ **NuGet package = Zero overhead** (no telemetry by default)  
✅ **Opt-in telemetry** via `RAPP_TELEMETRY` compilation symbol  
✅ **Runtime control** via `RappConfiguration`  
✅ **No breaking changes** - works with or without telemetry  
✅ **Fully AOT compatible** in both configurations  

## Examples

All samples in `/Samples` have telemetry enabled for demonstration. Remove `RAPP_TELEMETRY` from your production projects for maximum performance.
