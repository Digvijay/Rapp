# Console App Sample

Simple console application demonstrating basic Rapp usage patterns.

## Features

- Basic cache operations
- Performance measurement
- Cache removal and refresh
- Schema-aware serialization

## Running the Sample

```bash
dotnet run
```

## What This Sample Demonstrates

### 1. Basic Caching Pattern

```csharp
var user = await cache.GetOrCreateAsync(
    $"user-{userId}",
    async ct =>
    {
        // Fetch from database or external service
        return new UserData { ... };
    }
);
```

### 2. Cache Hit vs Miss Timing

The sample measures:
- First call (cache miss): ~100ms
- Second call (cache hit): ~0-1ms

### 3. High-Throughput Operations

Measures performance of 1000 consecutive cache operations to demonstrate Rapp's efficiency.

### 4. Cache Management

Shows how to manually invalidate cache entries when data changes.

## Expected Output

```
Rapp Console Sample
===================

Demo 1: Basic Caching
---------------------
  Cache MISS - fetching from 'database'...
  First call: 102ms
  User: John Doe (john@example.com)

  Second call (cached): 0ms
  User: John Doe (john@example.com)

Demo 2: Performance Measurement
--------------------------------
  1000 cache operations: 45ms
  Average: 0.045ms per operation

Demo 3: Cache Management
------------------------
  Removed cache entry
  Cache MISS after removal - fetching again...
  After removal: 101ms
  User: John Doe Refreshed (john.refreshed@example.com)

Sample completed successfully!
```

## Learning Points

- **Performance:** Observe the dramatic difference between cache hits and misses
- **Simplicity:** See how minimal code is needed for powerful caching
- **Safety:** Rapp handles serialization/deserialization automatically
- **Flexibility:** Easy to add, remove, and manage cache entries

## Next Steps

- Try modifying `UserData` to add new properties
- Experiment with different expiration times
- Add error handling for real-world scenarios
- Integrate with actual database operations
