# Getting Started with Rapp

Welcome to **Rapp**! This guide will walk you through setting up Rapp to achieve **safe, high-performance binary caching** in your .NET applications.

Rapp uses **Source Generators** to create AOT-compatible serialization code that is both faster than JSON and safer than raw binary serializers (like MemoryPack) for distributed systems.

## 🏁 Prerequisites

- .NET 10.0 or later
- An existing ASP.NET Core or Console application

## 📦 Step 1: Installation

Add the Rapp package to your project. This package includes both the runtime library and the source generators.

```bash
dotnet add package Rapp
```

You will also need `Microsoft.Extensions.Caching.Hybrid` if you haven't added it yet:

```bash
dotnet add package Microsoft.Extensions.Caching.Hybrid
```

## 🛠️ Step 2: Define Your Data Models

Rapp works by inspecting your data models at compile-time. To make a class compatible:
1. Make the class `partial`.
2. Add the `[RappCache]` attribute.
3. Add the `[MemoryPackable]` attribute (Rapp builds on MemoryPack's engine).

```csharp
using Rapp;
using MemoryPack;

namespace MyApp.Models;

// 1. partial class is required
// 2. [RappCache] adds schema safety & HybridCache integration
// 3. [MemoryPackable] adds high-performance binary serialization
[RappCache]
[MemoryPackable]
public partial class UserProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime LastLogin { get; set; }
}
```

> **Why `partial`?** Rapp generates a `UseRappForUserProfile()` extension method and a robust serializer implementation in a separate file during compilation.

## 🔌 Step 3: Register Services

In your `Program.cs` (or startup code), register `HybridCache` and chain the generated `UseRappFor{Type}` methods.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register HybridCache and strict-type Rapp serializers
builder.Services.AddHybridCache(options =>
    {
        options.MaximumPayloadBytes = 1024 * 1024; // 1MB
    })
    // ⬇️ Auto-generated extension methods for your types
    .UseRappForUserProfile(); 
```

## 🚀 Step 4: Use HybridCache

Inject `HybridCache` into your services or endpoints. You don't need to change how you use the cache—Rapp works transparently behind the scenes.

```csharp
app.MapGet("/users/{id}", async (Guid id, HybridCache cache) =>
{
    // Rapp handles the binary serialization & safety checks automatically
    var user = await cache.GetOrCreateAsync(
        key: $"user:{id}",
        factory: async ct => await dbContext.Users.FindAsync(id, ct)
    );

    return user;
});
```

## 🛡️ Step 5: Handling Schema Changes (The "Magic" Part)

One of Rapp's main features is **Schema Evolution Safety**. When you modify your class, Rapp automatically protects your application from crashing due to old/incompatible binary data in the cache.

### Scenario: Adding a Field

Let's say you update `UserProfile` to add a `PhoneNumber`:

```csharp
[RappCache]
[MemoryPackable]
public partial class UserProfile
{
    // ... existing fields ...
    public string? PhoneNumber { get; set; } // New field
}
```

**What happens?**
1. Rapp detects the schema change at compile-time.
2. It generates a new unique **Schema Hash** for version 2.
3. When your app reads an old (v1) cache entry, the hash mismatch is detected.
4. Rapp treats this as a **Cache Miss** (instead of crashing).
5. The `factory` method runs, fetches fresh data from the DB, and writes the new (v2) format to the cache.

**You don't need to do manual versioning!** Just modify your class, deploy, and Rapp handles the rest.

## 🏗️ Step 6: Native AOT Publishing

Rapp is 100% Native AOT compatible. To publish your app as a native executable:

1. Enable AOT in your `.csproj`:
   ```xml
   <PropertyGroup>
     <PublishAot>true</PublishAot>
   </PropertyGroup>
   ```

2. Publish:
   ```bash
   dotnet publish -c Release
   ```

Because Rapp uses Source Generators instead of Reflection, your app remains trim-safe and highly optimized.

## 🔍 Troubleshooting

- **"Method UseRappForX not found"**: Ensure your class is `public` (or `internal`) and `partial` and has the `[RappCache]` attribute. Try rebuilding the project to trigger source generation.
- **"Type is not serializable"**: Ensure all nested types in your model are also marked with `[MemoryPackable]`.

---

Ready for more? Check out the [README](../README.md) for advanced topics like Telemetry and Custom Validation.
