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
• 4.7x faster serialization means fewer servers needed
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

## 🎯 Advanced Features

### Integration with Existing Code

Rapp works with your existing `HybridCache` setup - no code changes required:

```csharp
// Before (JSON)
var user = await cache.GetOrCreateAsync("user", factory);

// After (Binary + Safe) - Same API!
var user = await cache.GetOrCreateAsync("user", factory);
```

## 📊 Performance

Comprehensive benchmarks demonstrate Rapp's performance characteristics:

| Operation | Rapp | MemoryPack | JSON |
|-----------|------|------------|------|
| **Serialization** | 582.9 ns | 275.2 ns | 3,260.3 ns |
| **Deserialization** | 813.6 ns | 609.7 ns | 10,957.5 ns |
| **Schema Safety** | ✅ Hash validation | ❌ Crash risk | ✅ Text-based |
| **AOT Compatible** | ✅ | ✅ | ❌ |

**Real-world performance:** ~780ns per cache operation with 80% hit rate.

> 📈 **Latest benchmark results** are available in the [`benchmarks/`](benchmarks/) folder. Run `scripts/update-benchmarks.sh` (Linux/macOS) or `scripts/Update-Benchmarks.ps1` (Windows) to update with the latest results.

## 🔍 Schema Evolution Demo

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

### Planned Features
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