using Microsoft.Extensions.Caching.Hybrid;
using Rapp;
using Rapp.Dashboard;
using MemoryPack;

var builder = WebApplication.CreateBuilder(args);

// Configure HybridCache with Rapp
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024; // 1MB
    options.MaximumKeyLength = 512;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
}).UseRappForProduct()
  .UseRappForCustomer();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Map Rapp Dashboard
app.MapRappDashboard();

// Product endpoints
app.MapGet("/products/{id}", async (int id, HybridCache cache) =>
{
    var product = await cache.GetOrCreateAsync(
        $"product-{id}",
        async ct => await GetProductAsync(id, ct)
    );
    return Results.Ok(product);
});

app.MapGet("/products", async (HybridCache cache) =>
{
    var products = await cache.GetOrCreateAsync(
        "products-all",
        async ct => await GetAllProductsAsync(ct),
        new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) }
    );
    return Results.Ok(products);
});

// Customer endpoints
app.MapGet("/customers/{id}", async (int id, HybridCache cache) =>
{
    var customer = await cache.GetOrCreateAsync(
        $"customer-{id}",
        async ct => await GetCustomerAsync(id, ct)
    );
    return Results.Ok(customer);
});

// Cache management
app.MapDelete("/cache/{key}", async (string key, HybridCache cache) =>
{
    await cache.RemoveAsync(key);
    return Results.Ok($"Removed cache key: {key}");
});

app.Run();

// Sample data access methods
static async Task<Product> GetProductAsync(int id, CancellationToken ct)
{
    await Task.Delay(100, ct); // Simulate database call
    return new Product
    {
        Id = id,
        Name = $"Product {id}",
        Price = 99.99m + id,
        Category = "Electronics",
        InStock = true
    };
}

static async Task<List<Product>> GetAllProductsAsync(CancellationToken ct)
{
    await Task.Delay(200, ct); // Simulate database call
    return Enumerable.Range(1, 10)
        .Select(i => new Product
        {
            Id = i,
            Name = $"Product {i}",
            Price = 99.99m + i,
            Category = "Electronics",
            InStock = i % 2 == 0
        })
        .ToList();
}

static async Task<Customer> GetCustomerAsync(int id, CancellationToken ct)
{
    await Task.Delay(150, ct); // Simulate database call
    return new Customer
    {
        Id = id,
        Name = $"Customer {id}",
        Email = $"customer{id}@example.com",
        CreatedAt = DateTime.UtcNow.AddDays(-id)
    };
}

// Domain models with Rapp attributes
[RappCache]
[MemoryPackable]
public partial class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool InStock { get; set; }
}

[RappCache]
[MemoryPackable]
public partial class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
