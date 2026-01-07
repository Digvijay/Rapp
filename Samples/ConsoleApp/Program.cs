using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Rapp;
using MemoryPack;
using System.Diagnostics;

Console.WriteLine("Rapp Console Sample");
Console.WriteLine("===================\n");

// Configure services
var services = new ServiceCollection();
services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024;
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5)
    };
}).UseRappForUserData();

var serviceProvider = services.BuildServiceProvider();
var cache = serviceProvider.GetRequiredService<HybridCache>();

// Demo 1: Basic Caching
Console.WriteLine("Demo 1: Basic Caching");
Console.WriteLine("---------------------");

var userId = 123;
var sw = Stopwatch.StartNew();

var user = await cache.GetOrCreateAsync(
    $"user-{userId}",
    async ct =>
    {
        Console.WriteLine("  Cache MISS - fetching from 'database'...");
        await Task.Delay(100, ct);
        return new Rapp.Samples.ConsoleApp.UserData
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            CreatedAt = DateTime.UtcNow
        };
    }
);

Console.WriteLine($"  First call: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"  User: {user.Name} ({user.Email})\n");

// Second call should be cached
sw.Restart();
user = await cache.GetOrCreateAsync(
    $"user-{userId}",
    async ct =>
    {
        Console.WriteLine("  This shouldn't print - data is cached!");
        await Task.Delay(100, ct);
        return new Rapp.Samples.ConsoleApp.UserData { Id = userId, Name = "Cached", Email = "cached@example.com", CreatedAt = DateTime.UtcNow };
    }
);

Console.WriteLine($"  Second call (cached): {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"  User: {user.Name} ({user.Email})\n");

// Demo 2: Serialization Performance
Console.WriteLine("Demo 2: Performance Measurement");
Console.WriteLine("--------------------------------");

var iterations = 1000;
var testData = new Rapp.Samples.ConsoleApp.UserData
{
    Id = 999,
    Name = "Performance Test User",
    Email = "perf@test.com",
    CreatedAt = DateTime.UtcNow
};

sw.Restart();
for (int i = 0; i < iterations; i++)
{
    await cache.GetOrCreateAsync($"perf-{i}", async ct => testData);
}
sw.Stop();

Console.WriteLine($"  {iterations} cache operations: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"  Average: {(double)sw.ElapsedMilliseconds / iterations:F3}ms per operation\n");

// Demo 3: Cache Removal
Console.WriteLine("Demo 3: Cache Management");
Console.WriteLine("------------------------");

await cache.RemoveAsync($"user-{userId}");
Console.WriteLine("  Removed cache entry");

sw.Restart();
user = await cache.GetOrCreateAsync(
    $"user-{userId}",
    async ct =>
    {
        Console.WriteLine("  Cache MISS after removal - fetching again...");
        await Task.Delay(100, ct);
        return new Rapp.Samples.ConsoleApp.UserData
        {
            Id = userId,
            Name = "John Doe Refreshed",
            Email = "john.refreshed@example.com",
            CreatedAt = DateTime.UtcNow
        };
    }
);

Console.WriteLine($"  After removal: {sw.ElapsedMilliseconds}ms");
Console.WriteLine($"  User: {user.Name} ({user.Email})\n");

Console.WriteLine("Sample completed successfully!");


namespace Rapp.Samples.ConsoleApp
{
    // Domain model
    [RappCache]
    [MemoryPackable]
    public partial class UserData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
