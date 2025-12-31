# ASP.NET Core Minimal API with Rapp

This sample demonstrates how to integrate Rapp with ASP.NET Core Minimal APIs for high-performance caching.

## Features

- HybridCache configuration with Rapp
- RESTful API endpoints
- Rapp Dashboard integration
- Schema-aware caching
- Swagger documentation

## Running the Sample

```bash
dotnet run
```

The application will start on `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS).

## Endpoints

- `GET /products/{id}` - Get a product by ID (cached)
- `GET /products` - Get all products (cached)
- `GET /customers/{id}` - Get a customer by ID (cached)
- `DELETE /cache/{key}` - Remove a cache entry
- `GET /rapp` - View Rapp Dashboard
- `GET /swagger` - View API documentation

## Key Implementation Details

### 1. Domain Models

Models use both `[RappCache]` and `[MemoryPackable]` attributes:

```csharp
[RappCache]
[MemoryPackable]
public partial class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    // ...
}
```

### 2. Service Registration

Configure HybridCache with Rapp serializers:

```csharp
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5)
    };
}).UseRappForProduct()
  .UseRappForCustomer();
```

### 3. Using the Cache

Standard HybridCache patterns work seamlessly:

```csharp
app.MapGet("/products/{id}", async (int id, HybridCache cache) =>
{
    var product = await cache.GetOrCreateAsync(
        $"product-{id}",
        async ct => await GetProductAsync(id, ct)
    );
    return Results.Ok(product);
});
```

## Testing the Sample

1. Get a product:
   ```bash
   curl http://localhost:5000/products/1
   ```

2. View cache performance:
   ```bash
   curl http://localhost:5000/rapp
   ```

3. Remove cached data:
   ```bash
   curl -X DELETE http://localhost:5000/cache/product-1
   ```

## Performance Benefits

- **Faster than JSON:** 4.7x faster serialization
- **Smaller payloads:** 70% smaller than JSON
- **Schema safety:** Automatic validation on schema changes

## Next Steps

- Try modifying the Product model and observing schema evolution
- Monitor cache performance via the Rapp Dashboard
- Integrate with Redis for distributed caching
