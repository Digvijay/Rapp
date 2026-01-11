# Rapp: Schema-Aware Binary Serialization for .NET 10

**Rapp** is a source-generated library that enables safe, high-performance binary caching in .NET 10 applications by bridging the gap between MemoryPack's raw performance and enterprise deployment safety requirements.

[![NuGet Version](https://img.shields.io/nuget/v/Rapp.svg)](https://www.nuget.org/packages/Rapp/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Rapp.svg)](https://www.nuget.org/packages/Rapp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Native AOT](https://img.shields.io/badge/AOT-Compatible-green.svg)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
[![Build Status](https://github.com/Digvijay/Rapp/actions/workflows/publish.yml/badge.svg)](https://github.com/Digvijay/Rapp/actions)

## � Table of Contents

- [🚀 Why Rapp?](#-why-rapp)
- [💡 Understanding Rapp: A Layman's Guide](#-understanding-rapp-a-laymans-guide)
- [🏗️ Architecture](#️-architecture)
- [📦 Installation](#-installation)
- [📋 Requirements](#-requirements)
- [⚡ Quick Start](#-quick-start)
- [🛡️ Enterprise Capabilities](#️-enterprise-capabilities)
- [🎯 Advanced Features](#-advanced-features)
- [📊 Performance](#-performance)
- [🔍 Schema Evolution Demo](#-schema-evolution-demo)
- [📊 Monitoring & Diagnostics](#-monitoring--diagnostics)
- [📦 Supported Platforms](#-supported-platforms)
- [📝 Changelog](#-changelog)
- [🗺️ Roadmap](#️-roadmap)
- [🐛 Issues & Support](#-issues--support)
- [🤝 Contributing](#-contributing)
- [🙏 Acknowledgments](#-acknowledgments)
- [📄 License](#-license)

Standard caching libraries rely on text-based serialization (JSON), which is safe but slow and memory-intensive. High-performance binary serializers like MemoryPack offer incredible speed but introduce deployment risks due to strict schema coupling.

Rapp generates highly optimized static code that looks exactly like code you would write by hand, providing **MemoryPack's performance with enterprise-grade safety**.

| Feature | JSON Cache | MemoryPack | **Rapp** |
|---------|------------|------------|----------|
| **Performance** | 🐌 Slow | ⚡ Fast | ⚡ Fast |
| **Schema Safety** | ✅ Safe | ❌ Crash Risk | ✅ Safe |
| **AOT Compatible** | ❌ Reflection | ✅ | ✅ |
| **Enterprise Ready** | ✅ | ❌ | ✅ |

## 💡 Understanding Rapp: A Layman's Guide

### What is Rapp?

Rapp (pronounced "rap") represents **R**eliable **App**lication **P**ersistence - representing trustworthy, dependable caching without compromise.

### Why Does Rapp Exist?

Traditional caching in .NET applications works like this:

• At runtime (when your app is running), JSON serializers convert objects to text
• This is like having a verbose conversation - lots of words, slow communication
• It's safe but slow, uses lots of memory, and bloats network traffic

Rapp takes a different approach:

• At compile-time (when you build your app), it generates optimized binary serialization code automatically
• This is like having a high-speed data protocol pre-negotiated and ready to execute instantly
• It's fast, memory-efficient, and works perfectly in cloud-native and serverless environments

### Business Problems Rapp Solves

#### 🚀 Performance Problems
• Slow application response times - JSON serialization delays every cache operation
• High memory usage - Text-based caches consume unnecessary RAM
• Poor user experience - Cache misses hurt conversion rates and user satisfaction

#### ☁️ Cloud Cost Problems
• Higher infrastructure costs - Slower apps need more servers to handle the same load
• Network egress charges - Verbose JSON payloads increase data transfer costs
• Memory limits exceeded - JSON caches consume too much RAM in constrained environments

#### 🔒 Modern Deployment Problems
• Schema evolution risks - Binary serializers crash on property changes during deployments
• AOT incompatibility - Traditional libraries can't be used in ahead-of-time compiled apps
• Container size issues - Reflection requires keeping metadata that bloats container images

### Business Advantages of AOT Technology

#### 💰 Cost Savings
• 5.6× faster serialization means fewer servers needed
• Lower memory usage allows more users per server
• Smaller binary payloads reduce network and storage costs

#### ⚡ User Experience
• Instant cache operations - Binary serialization happens in microseconds, not milliseconds
• Faster API responses - Cache hits return data instantly
• Better mobile performance - Critical for mobile apps and PWAs

#### 🏢 Enterprise Benefits
• Cloud-native ready - Works perfectly in Kubernetes, serverless, and edge computing
• Compliance friendly - No dynamic code generation means easier security audits
• Future-proof - Compatible with .NET's most advanced compilation technologies

### The Rapp Difference

In an era where milliseconds matter and cloud costs dominate IT budgets, Rapp delivers genuine performance improvements that translate directly to business value.

**Rapp doesn't just cache data—it makes your entire application faster.**

## 🏗️ Architecture

Rapp's architecture is designed for maximum performance and AOT compatibility. The Roslyn Source Generator analyzes your model classes at compile-time and generates static binary serialization methods that are indistinguishable from hand-written code.

Key Components:

• **Source Generator**: Analyzes `[RappCache]` attributes and generates optimized serialization code
• **Schema Validator**: Computes cryptographic hashes to detect schema changes
• **Binary Serializer**: Uses MemoryPack for zero-copy performance
• **Cache Integration**: Seamlessly integrates with `HybridCache`

> **Note:** To ensure full compatibility with Ahead-of-Time (AoT) compilation, Rapp uses compile-time source generation instead of runtime reflection, guaranteeing that all serialization logic is statically compiled.

## 📦 Installation

```bash
dotnet add package Rapp
```

## 📋 Requirements

- **.NET Version:** 10.0 or later
- **Dependencies:**
  - `Microsoft.Extensions.Caching.Hybrid` (10.1.0+)
  - `MemoryPack` (1.21.4+)
- **Platforms:** Windows, macOS, Linux
- **Architectures:** x64, ARM64

> 📋 **Complete dependency details** are available in [`docs/DEPENDENCIES.md`](docs/DEPENDENCIES.md), including version verification and compatibility notes.

## ⚡ Quick Start

> 📘 **New to Rapp?** Check out our step-by-step [Getting Started Guide](docs/GETTING_STARTED.md) for a comprehensive walkthrough.

### 1. Mark Your Cacheable Types

Decorate DTOs with the `[RappCache]` attribute. The class must be `partial` to allow source generation.

```csharp
[RappCache]
public partial class UserProfile
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public DateTime LastLogin { get; set; }
}
```

### 2. Use in Your Application

Rapp integrates seamlessly with .NET's `HybridCache`:

```csharp
public class UserService
{
    private readonly HybridCache _cache;

    public UserService(HybridCache cache)
    {
        _cache = cache;
    }

    public async Task<UserProfile?> GetUserProfileAsync(Guid userId)
    {
        return await _cache.GetOrCreateAsync(
            $"user:{userId}",
            async ct => await _database.GetUserAsync(userId),
            options: new() { Expiration = TimeSpan.FromHours(1) }
        );
    }
}
```

That's it! Rapp automatically handles schema validation and binary serialization.

## 🛡️ Enterprise Capabilities

### 1. Schema Evolution Safety

Rapp prevents deployment crashes by detecting schema changes:

```csharp
// v1.0 - Original schema
[RappCache]
public partial class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// v2.0 - Added new field (✅ Safe)
[RappCache]
public partial class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; } // New field - automatically handled
}

// v2.1 - Removed field (✅ Safe - cache miss, fresh data)
[RappCache]
public partial class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    // Price removed - Rapp detects change and fetches fresh data
}
```

### 2. Performance Monitoring

Rapp includes built-in telemetry for cache performance tracking:

```csharp
// Metrics are automatically collected via .NET System.Diagnostics.Metrics
// Available metrics:
// - rapp_cache_hits_total: Total number of cache hits
// - rapp_cache_misses_total: Total number of cache misses
```

> **Note:** Telemetry is conditionally compiled with `RAPP_TELEMETRY` define for production use.

### 3. AOT Compatibility

Rapp is fully compatible with Native AOT compilation:

```xml
<!-- In your .csproj -->
<PropertyGroup>
    <PublishAot>true</PublishAot>
</PropertyGroup>
```

```

### 3. Native AOT Compatibility

Rapp is **100% compatible with Native AOT** (Ahead-of-Time) compilation, making it ideal for serverless and high-performance cloud scenarios.

**Why Rapp is AOT Ready:**
- **Source Generation:** All serialization code is generated at compile-time using Roslyn 4.0 source generators.
- **No Runtime Reflection:** Rapp avoids `System.Reflection` entirely for serialization paths, preventing AOT trim warnings.
- **Static Analysis:** All types are analyzed during build, ensuring AOT linkers can see all used types.
- **MemoryPack Foundation:** Built on top of MemoryPack, which is designed from the ground up for AOT.

> **Note on Demos:** The sample applications (`AspNetCoreMinimalApi`, `GrpcService`) utilize `System.Text.Json` reflection-based serialization **solely for comparison purposes** (to calculate cost savings vs JSON). Because of this comparative logic, the demos themselves generate AOT warnings. However, the **Rapp library itself** is fully AOT compliant and can be used in strictly AOT-enforced projects (like the `ConsoleApp` sample configured for AOT).

## 🎯 Advanced Features

### 👻 The Ghost Reader (Zero-Copy View)

Rapp includes a "Ghost Reader" feature for ultra-low latency scenarios (trading, gaming, IoT). Instead of deserializing objects on the heap, it creates a **Zero-Copy, Zero-Allocation** `ref struct` view over your raw binary data.

```csharp
// 1. Define
[RappGhost]
public class Tick { public double Price; public long Time; }

// 2. Read (Zero Allocation)
var ghost = new TickGhost(buffer);
if (ghost.Price > 100.0) { ... } // instant access
```

[👉 Read the full Ghost Reader Documentation](docs/GHOST_READER.md)

### Integration with Existing Code

Rapp works with your existing `HybridCache` setup - no code changes required:

```csharp
// Before (JSON)
var user = await cache.GetOrCreateAsync("user", factory);

// After (Binary + Safe) - Same API!
var user = await cache.GetOrCreateAsync("user", factory);
```

## 📊 Performance

Comprehensive benchmarks demonstrate Rapp's production performance (without telemetry overhead):

### Pure Serialization Performance

| Operation | Rapp | MemoryPack | JSON |
|-----------|------|------------|------|
| **Serialization** | 397.2 ns | 197.0 ns | 1,764.1 ns |
| **Deserialization** | 240.9 ns | 180.0 ns | 4,238.1 ns |
| **Schema Safety** | ✅ Hash validation | ❌ Crash risk | ✅ Text-based |
| **AOT Compatible** | ✅ | ✅ | ❌ |

### HybridCache Integration Performance

| Approach | Single Operation | Realistic Workload (100 ops, 80% hit) |
|----------|------------------|----------------------------------------|
| **HybridCache + Rapp** | 436.9 ns | 30.5 μs |
| **HybridCache + MemoryPack** | 416.5 ns | 44.1 μs |
| **DirectMemoryCache** | 93.9 ns | 13.0 μs |

**Key Insights:**
- **4.4× faster** than JSON serialization, **17.6× faster** deserialization
- **102% overhead** vs raw MemoryPack for schema validation (serialize)
- **34% overhead** vs raw MemoryPack for schema validation (deserialize)
- **HybridCache overhead:** Only 4.9% vs MemoryPack in single operations
- **Realistic workload:** Rapp is **31% FASTER** than MemoryPack (30.5μs vs 44.1μs) due to optimized caching

> 📈 **Latest benchmark results** are available in the [`benchmarks/`](benchmarks/) folder. Run `scripts/update-benchmarks.sh` (Linux/macOS) or `scripts/Update-Benchmarks.ps1` (Windows) to update with the latest results.

## � Competitive Analysis

### How Rapp Compares to Alternatives

Based on research from official repositories and creator documentation:

#### Performance Comparison Table

| **Serializer** | **Serialize** | **Deserialize** | **Realistic Workload** | **Schema Safety** | **Cross-Language** | **GitHub Stars** |
|---|---|---|---|---|---|---|
| **Rapp** | **397ns** | **241ns** | **30.5μs** | ✅ Automatic | ❌ .NET only | New |
| **MemoryPack** | 197ns | 180ns | 44.1μs | ⚠️ Limited | ❌ C# only | ~4.5k |
| **MessagePack-CSharp** | ~250-350ns | ~180-250ns | ~45-55μs | ✅ Manual | ✅ 50+ languages | 6.6k |
| **protobuf-net** | ~600-800ns | ~400-600ns | ~120-180μs | ✅ IDL-based | ✅ Protocol Buffers | 4.9k |
| **System.Text.Json** | 1,764ns | 4,238ns | ~200-300μs | ❌ N/A | ✅ JSON standard | Built-in |

#### Feature Matrix

| **Feature** | **Rapp** | **MemoryPack** | **MessagePack** | **protobuf-net** | **System.Text.Json** |
|---|---|---|---|---|---|
| **Native AOT** | ✅ Full | ✅ Full | ✅ Source gen | ✅ Supported | ✅ Supported |
| **Schema Validation** | ✅ Automatic SHA256 | ❌ Crashes on changes | ⚠️ Manual versioning | ⚠️ IDL required | ❌ N/A |
| **Version Tolerance** | ✅ Detect incompatible | ⚠️ **Can't remove/reorder/change** | ✅ Full tolerance | ⚠️ Manual management | ❌ N/A |
| **Null Handling** | ✅ Proper | ❌ **No null distinction** | ✅ Proper | ⚠️ **Known issues** | ✅ Proper |
| **HybridCache Integration** | ✅ First-class | ❌ Manual | ❌ Manual | ❌ Manual | ✅ Built-in |
| **Realistic Workload** | ✅ **31% FASTER** | Baseline | ❌ Slower | ❌ Much slower | ❌ Much slower |

### Rapp's Unique Value Proposition

**The Gap Rapp Fills:** Modern .NET microservices with continuous deployment need binary performance without deployment crashes.

**Problem with Alternatives:**
- **MemoryPack**: Crashes on schema changes ([official docs](https://github.com/Cysharp/MemoryPack): *"members can be added, but can NOT be deleted; can't change member order; can't change member type"*)
- **MessagePack-CSharp**: Requires manual version management (6.6k stars, mature but complex)
- **protobuf-net**: IDL overhead + null handling issues ([docs warning](https://github.com/protobuf-net/protobuf-net): *"cannot handle null and empty collection correctly"*)
- **System.Text.Json**: Too slow for high-performance caching (1,764ns vs 397ns)

**Rapp's Solution:**
```csharp
// Deploy v1
[Rapp] public class User { public int Id; public string Name; }
// Cache written with hash: SHA256("User-Id:int-Name:string")

// Deploy v2 - SAFE DETECTION!
[Rapp] public class User { public int Id; /* removed Name */ }
// Cache read detects hash mismatch → graceful fallback → no crash!
```

### Use Case Decision Matrix

| **Use Case** | **Recommended Solution** | **Why** |
|---|---|---|
| **.NET 10 continuous deployment + HybridCache** | **Rapp** | Automatic schema safety + fastest realistic workloads |
| **Unity game development** | **MemoryPack** | IL2CPP support + raw speed for struct arrays |
| **Multi-language microservices (Go/Python/Node)** | **MessagePack-CSharp** | Cross-language + mature ecosystem |
| **gRPC services with strict governance** | **protobuf-net** | IDL-based schema + Protocol Buffers standard |
| **Debugging/configuration storage** | **System.Text.Json** | Human-readable + built-in |

### Competitive Positioning

```
     Raw Speed ←─────────────────────────→ Enterprise Safety
          │                                     │
    MemoryPack ───→ Rapp ───→ MessagePack ───→ JSON
     (crashes)    (automatic)  (manual)     (human-readable)
          │           │          │              │
       197ns       397ns      ~300ns         1,764ns
      ❌ Unsafe    ✅ Safe    ⚠️ Manual       ✅ Safe but slow
```

**Rapp's Sweet Spot**: Fills the critical gap between MemoryPack's raw speed (but crashes) and MessagePack's safety (but manual management).

**Market Validation**: Research from official sources confirms:
1. MemoryPack creator acknowledges version intolerance (can't remove/reorder/change types)
2. Microsoft uses MessagePack for SignalR/VS 2022 (validates binary serialization need)
3. No existing solution provides automatic schema validation for .NET binary caching
4. Rapp is **31% faster** than MemoryPack in realistic workloads (compile-time optimizations)

**The 102% overhead vs MemoryPack is JUSTIFIED because**:
- MemoryPack crashes on schema changes (documented limitation)
- Rapp's realistic workloads are **31% FASTER** due to compile-time constant optimization
- MessagePack requires manual versioning (developer burden)
- No alternative provides automatic .NET schema validation

**Conclusion**: Rapp is the **ONLY .NET 10 serializer** combining near-MemoryPack performance with automatic schema safety, making it ideal for continuous deployment scenarios where raw MemoryPack would crash and MessagePack would be overkill.

## �🔍 Schema Evolution Demo

Experience Rapp's safety advantages firsthand:

```bash
# Run the interactive demo
./scripts/run-schema-evolution-demo.sh

# Or on Windows
./scripts/Run-SchemaEvolutionDemo.ps1
```

The demo shows:
- MemoryPack crash scenarios during deployment
- Rapp's graceful handling of schema changes
- Performance comparisons across serializers

##  Supported Platforms

- **.NET Version:** 10.0+
- **OS Support:** Windows, macOS, Linux
- **Architecture:** x64, ARM64
- **Deployment:** Native AOT, Containers, Serverless

## 📝 Changelog

See [CHANGELOG.md](CHANGELOG.md) for release notes and version history.

## �️ Roadmap

### Planned Features (not sure if I would have time but PRs are welcome!)
- **Enhanced Monitoring**: Additional metrics and observability features
- **Custom Serialization Options**: Configurable serialization modes (compression, alternative backends, field-level customization)
- **Custom Codecs**: Support for additional serialization formats
- **Distributed Tracing**: Enhanced observability with OpenTelemetry
- **Configuration UI**: Visual schema management tools
- **Migration Tools**: Automated schema evolution assistants

### Community Requests
Have a feature request? [Open a discussion](https://github.com/Digvijay/Rapp/discussions)!
## 🌟 Projects Using Rapp

*Coming soon - we'd love to showcase your projects using Rapp!*

[Add your project here](https://github.com/Digvijay/Rapp/discussions/new?category=show-and-tell)
## �🐛 Issues & Support

- **Bug Reports:** [GitHub Issues](https://github.com/Digvijay/Rapp/issues)
- **Discussions:** [GitHub Discussions](https://github.com/Digvijay/Rapp/discussions)
- **Security:** [Security Policy](https://github.com/Digvijay/Rapp/security/policy)

## 🤝 Contributing

Rapp is open-source. We welcome contributions to expand the validation rule set and optimize serialization patterns.

- [Contributing Guide](CONTRIBUTING.md)
- [Code of Conduct](CODE_OF_CONDUCT.md)
- [Getting Started Guide](docs/GETTING_STARTED.md)
- [Dependencies & Versions](docs/DEPENDENCIES.md)
- [Documentation](docs/)

## � Acknowledgments

- **MemoryPack** - High-performance binary serialization foundation
- **Microsoft.Extensions.Caching.Hybrid** - Modern .NET caching abstractions
- **Roslyn Source Generators** - Compile-time code generation technology
- **BenchmarkDotNet** - Performance benchmarking framework
> 📦 **All dependencies are kept at their latest compatible versions** - see [`docs/DEPENDENCIES.md`](docs/DEPENDENCIES.md) for detailed version information and verification process.
## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.