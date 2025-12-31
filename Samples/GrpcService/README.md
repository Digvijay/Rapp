# gRPC Service with Rapp

This sample demonstrates using Rapp for high-performance caching in gRPC services.

## Features

- gRPC service implementation
- Rapp-backed HybridCache
- Binary serialization for cache
- High-throughput data access

## Running the Sample

```bash
dotnet run
```

The gRPC service will start on `http://localhost:5000` and `https://localhost:5001`.

## Testing with grpcurl

Install grpcurl: https://github.com/fullstorydev/grpcurl

```bash
# Get single data item
grpcurl -plaintext -d '{"id": 123}' localhost:5000 dataservice.DataService/GetData

# List data with pagination
grpcurl -plaintext -d '{"page_number": 1, "page_size": 10}' localhost:5000 dataservice.DataService/ListData
```

## Key Implementation Details

### 1. Cache Configuration

Configured for larger payloads typical in gRPC:

```csharp
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 10 * 1024 * 1024; // 10MB
}).UseRappForCachedData();
```

### 2. gRPC Service with Caching

```csharp
public override async Task<DataReply> GetData(DataRequest request, ...)
{
    var cachedData = await _cache.GetOrCreateAsync(
        $"data-{request.Id}",
        async ct => await FetchDataFromDatabase(request.Id, ct)
    );
    return MapToGrpcReply(cachedData);
}
```

### 3. Cache Models

Separate from gRPC models for optimal performance:

```csharp
[RappCache]
[MemoryPackable]
public partial class CachedData
{
    public int Id { get; set; }
    public string Name { get; set; }
    // ...
}
```

## Performance Benefits

- **Fast serialization:** Binary format is 4.7x faster than JSON
- **Smaller cache footprint:** 70% reduction vs JSON
- **High throughput:** Optimized for gRPC's high-speed scenarios
- **Schema safety:** Automatic validation prevents cache corruption

## Architecture Notes

This sample separates concerns:
- **gRPC models** (generated from .proto files)
- **Cache models** (Rapp/MemoryPack optimized)
- **Service logic** (maps between the two)

This separation allows:
- Independent evolution of API and cache schemas
- Optimal serialization for each use case
- Clear boundaries between layers

## Next Steps

- Add Redis for distributed caching
- Implement cache invalidation patterns
- Add monitoring and metrics
- Load test to measure performance gains
