# Package Dependencies & Versions

This document outlines all NuGet package dependencies used in the Rapp project, their current versions, and verification that they are using the latest compatible versions.

## 📦 Core Dependencies

### Rapp Library (src/Rapp/Rapp.csproj)

| Package | Version | Status | Notes |
|---------|---------|--------|-------|
| `Microsoft.Extensions.Caching.Hybrid` | `10.1.0` | ✅ Latest | Modern .NET caching abstractions |
| `MemoryPack` | `1.21.4` | ✅ Latest | High-performance binary serialization |

### Source Generator (src/Rapp.Gen/Rapp.Gen.csproj)

| Package | Version | Status | Notes |
|---------|---------|--------|-------|
| `Microsoft.CodeAnalysis.CSharp` | `4.14.0` | ✅ Latest Compatible | Roslyn compiler API for C# |
| `Microsoft.CodeAnalysis.Analyzers` | `3.11.0` | ✅ Latest Compatible | Roslyn code analysis rules |

**Compatibility Note**: Roslyn analyzer packages have strict target framework requirements. We use standard 2.0 compatible versions where applicable to ensure broad compatibility.

## 🧪 Testing & Benchmarking

### Benchmark Project (src/Rapp.Benchmark/Rapp.Benchmark.csproj)

| Package | Version | Status | Notes |
|---------|---------|--------|-------|
| `BenchmarkDotNet` | `0.15.8` | ✅ Latest | Performance benchmarking framework |
| `System.Text.Json` | `10.0.1` | ✅ Latest | JSON serialization for comparisons |

## 🌐 Web & ASP.NET

### Playground Project (src/Rapp.Playground/Rapp.Playground.csproj)

| Package | Version | Status | Notes |
|---------|---------|--------|-------|
| `Microsoft.AspNetCore.OpenApi` | `10.0.1` | ✅ Latest | OpenAPI/Swagger generation |
| `MemoryPack` | `1.21.4` | ✅ Latest | Binary serialization |

## 🔄 Version Update Process

Package versions are regularly verified against NuGet.org to ensure we're using the latest compatible versions:

1. **Automated Checks**: CI/CD pipeline verifies no outdated packages
2. **Manual Verification**: Quarterly review using NuGet API queries
3. **Compatibility Testing**: All updates tested across all target frameworks
4. **Documentation Updates**: This document updated when versions change

## 🏗️ Build Requirements

- **.NET SDK**: 10.0 or later
- **Target Frameworks**:
  - `net10.0` (main library)
  - `netstandard2.1` (source generator)
- **OS Support**: Windows, macOS, Linux
- **Architecture**: x64, ARM64

## 📋 Dependency Management

### Package Reference Strategy

- **Core Dependencies**: Explicit version pinning for stability
- **Development Tools**: Latest versions for best tooling experience
- **Analyzer Packages**: Compatible versions for target framework support
- **Private Assets**: Analyzer dependencies marked as private to avoid transitive exposure

### Update Policy

- **Security Updates**: Immediate application
- **Bug Fixes**: Applied in next minor release
- **Feature Updates**: Evaluated for compatibility impact
- **Breaking Changes**: Major version updates only

## 🔍 Verification Commands

To verify package versions manually:

```bash
# Check specific package latest version
curl -s "https://api.nuget.org/v3-flatcontainer/{package-name}/index.json" | grep -o '"[0-9]\+\.[0-9]\+\.[0-9]\+"' | tail -5

# Build verification
dotnet build Rapp.sln --configuration Release

# Restore verification
dotnet restore Rapp.sln
```

## 📈 Version History

| Date | Action | Details |
|------|--------|---------|
| 2025-12-31 | Initial verification | All packages confirmed at latest compatible versions |
| 2025-12-31 | Roslyn analyzer compatibility | Confirmed 4.10.0/3.3.4 are latest compatible with .NET Standard 2.1 |
| 2026-01-07 | Update verification | All packages updated to latest. CodeAnalysis updated to 4.14.0/3.11.0. |

---

*This document is automatically updated when package versions change. Last verified: January 7, 2026*