# Migration Guide

This guide helps you migrate from existing caching solutions to Rapp.

## Table of Contents

- [From JSON (System.Text.Json)](#from-json-systemtextjson)
- [From MemoryPack](#from-memorypack)
- [From Other Binary Serializers](#from-other-binary-serializers)
- [Handling Schema Evolution](#handling-schema-evolution)

---

## From JSON (System.Text.Json)

### Before (JSON)

```csharp
public class UserData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Configure HybridCache with JSON
builder.Services.AddHybridCache(options =>
{
    // JSON is the default
});

// Usage
var cached = await cache.GetOrCreateAsync(
    "user-123",
    async ct => await GetUserDataAsync(123, ct)
);
```

### After (Rapp)

```csharp
using Rapp;
using MemoryPack;

[RappCache]  // Add this attribute
[MemoryPackable]  // Add this attribute
public partial class UserData  // Add 'partial' keyword
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Configure HybridCache with Rapp
builder.Services.AddHybridCache(options =>
{
    // Options configuration
}).UseRappForUserData();  // Generated extension method

// Usage (same as before)
var cached = await cache.GetOrCreateAsync(
    "user-123",
    async ct => await GetUserDataAsync(123, ct)
);
```

### Benefits

- **4.7x faster serialization**
- **65% less memory usage**
- **Smaller cache payloads** (70% smaller than JSON)
- **Schema validation** prevents crashes on deployments

---

## From MemoryPack

### Before (Direct MemoryPack)

```csharp
[MemoryPackable]
public partial class CacheItem
{
    public int Id { get; set; }
    public string Value { get; set; }
}

// Custom serializer
public class MemoryPackHybridSerializer<T> : IHybridCacheSerializer<T>
{
    public void Serialize(T value, IBufferWriter<byte> target)
    {
        MemoryPackSerializer.Serialize(target, value);
    }

    public T Deserialize(ReadOnlySequence<byte> source)
    {
        return MemoryPackSerializer.Deserialize<T>(source);
    }
}

// Manual registration
builder.Services.AddHybridCache()
    .AddSerializer<CacheItem, MemoryPackHybridSerializer<CacheItem>>();
```

### After (Rapp)

```csharp
[RappCache]  // Add this attribute
[MemoryPackable]
public partial class CacheItem
{
    public int Id { get; set; }
    public string Value { get; set; }
}

// Automatic registration
builder.Services.AddHybridCache()
    .UseRappForCacheItem();  // Generated extension method
```

### Benefits

- **No custom serializer code** - fully generated
- **Schema validation** prevents crashes on property changes
- **Same performance** as raw MemoryPack
- **Safer deployments** with automatic version checking

### Important Differences

#### Schema Changes

**MemoryPack** - Crashes on schema mismatch:
```csharp
// Old version
[MemoryPackable]
public partial class Item
{
    public int Id { get; set; }
}

// New version - CRASHES on old cached data!
[MemoryPackable]
public partial class Item
{
    public int Id { get; set; }
    public string Name { get; set; }  // Added property
}
```

**Rapp** - Handles schema changes gracefully:
```csharp
// Old version
[RappCache]
[MemoryPackable]
public partial class Item
{
    public int Id { get; set; }
}

// New version - Returns default, cache rebuilds automatically
[RappCache]
[MemoryPackable]
public partial class Item
{
    public int Id { get; set; }
    public string Name { get; set; }  // Schema mismatch detected, returns default
}
```

---

## From Other Binary Serializers

### From Protobuf-net

```csharp
// Before
[ProtoContract]
public class MyData
{
    [ProtoMember(1)]
    public int Id { get; set; }
}

// After
[RappCache]
[MemoryPackable]
public partial class MyData
{
    public int Id { get; set; }
}
```

### From MessagePack

```csharp
// Before
[MessagePackObject]
public class MyData
{
    [Key(0)]
    public int Id { get; set; }
}

// After
[RappCache]
[MemoryPackable]
public partial class MyData
{
    public int Id { get; set; }
}
```

---

## Handling Schema Evolution

### Deployment Strategy

1. **Add New Properties with Defaults**
   ```csharp
   [RappCache]
   [MemoryPackable]
   public partial class User
   {
       public int Id { get; set; }
       public string Name { get; set; }
       public string? Email { get; set; } = null;  // Safe to add
   }
   ```

2. **Use Nullable Types for Optional Data**
   ```csharp
   [RappCache]
   [MemoryPackable]
   public partial class Order
   {
       public int Id { get; set; }
       public DateTime? ProcessedAt { get; set; }  // Nullable
   }
   ```

3. **Clear Cache on Breaking Changes**
   ```csharp
   // Option 1: Clear specific cache entries
   await cache.RemoveAsync("user-*");
   
   // Option 2: Use versioned keys
   var key = $"v2:user-{userId}";  // Version prefix
   ```

### Schema Change Detection

Rapp automatically detects schema changes and returns `default(T)`:

```csharp
// Old cached data exists
var result = await cache.GetOrCreateAsync(
    "data-key",
    async ct => await FetchDataAsync(ct)
);

// If schema changed:
// - Old data is rejected (returns default)
// - Factory function is called
// - New data with new schema is cached
```

### Monitoring Schema Mismatches

```csharp
// Enable telemetry to track schema mismatches
RappConfiguration.EnableTelemetry = true;

// Metrics available:
// - rapp_cache_hits_total
// - rapp_cache_misses_total
// - rapp_schema_mismatches_total (when enabled)
```

---

## Configuration Options

### Global Configuration

```csharp
// Disable telemetry
RappConfiguration.EnableTelemetry = false;

// Enable detailed errors (development only)
RappConfiguration.EnableDetailedErrors = true;

// Throw exceptions on schema mismatch (testing only)
RappConfiguration.ThrowOnSchemaMismatch = true;
```

### Per-Type Configuration

Generated extension methods provide type-safe registration:

```csharp
builder.Services.AddHybridCache()
    .UseRappForUser()        // For User class
    .UseRappForProduct()     // For Product class
    .UseRappForOrder();      // For Order class
```

---

## Performance Comparison

| Scenario | JSON | MemoryPack | Rapp | Notes |
|----------|------|------------|------|-------|
| Serialize (1KB) | 850ns | 180ns | 185ns | Rapp overhead: ~3% |
| Deserialize (1KB) | 920ns | 195ns | 200ns | Rapp overhead: ~2.5% |
| Payload Size | 1.0x | 0.32x | 0.32x | Same as MemoryPack |
| Schema Safety | ✅ | ❌ | ✅ | Rapp matches JSON |
| AOT Compatible | ❌ | ✅ | ✅ | Rapp matches MemoryPack |

---

## Troubleshooting

### Issue: "Type is not MemoryPackable"

**Solution:** Add both attributes:
```csharp
[RappCache]
[MemoryPackable]
public partial class MyClass { }
```

### Issue: "Serializer not generated"

**Cause:** Missing `partial` keyword

**Solution:**
```csharp
// Wrong
[RappCache]
public class MyClass { }

// Correct
[RappCache]
public partial class MyClass { }
```

### Issue: Getting default values after deployment

**Cause:** Schema changed, old cache data rejected

**Solution:** This is expected behavior. Cache will rebuild with new schema.

---

## Best Practices

1. **Always use `partial` classes**
2. **Add properties as nullable when possible**
3. **Use versioned cache keys for breaking changes**
4. **Monitor schema mismatch metrics**
5. **Test deployments with schema changes**
6. **Keep telemetry enabled in production**

---

## Need Help?

- Check the [Documentation](docs/)
- View [Examples](examples/)
- Open an [Issue](https://github.com/Digvijay/Rapp/issues)
- Read the [FAQ](docs/FAQ.md)
