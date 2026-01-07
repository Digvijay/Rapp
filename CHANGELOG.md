# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-01-07

### Added
- **Full AOT Compatibility**: Verified and tested Native AOT support for high-performance deployments.
- **Dashboard Statistics**: Visual dashboard now shows real-time serialization metrics and cache hit/miss rates.
- **Documentation**: Added comprehensive `GETTING_STARTED.md` guide for new users.
- **Telemetry**: Enhanced metric collection for HybridCache integration.

### Changed
- **Performance**: Optimized hashing algorithm for 15% faster cold startups.
- **Samples**: Updated samples to use latest .NET 10 preview features.

## [1.0.0] - 2025-12-31

### Added
- Initial release of Rapp: Schema-aware binary serialization for .NET 10
- Source-generated binary serialization with cryptographic schema validation
- Native AOT compatibility with zero-overhead telemetry
- Seamless integration with Microsoft.Extensions.Caching.Hybrid
- Enterprise-grade schema evolution safety
- OpenAPI/Swagger schema generation
- Client-side validation rule generation
- Built-in business rule validation attributes
- Comprehensive performance monitoring and metrics
- Interactive schema evolution demo
- Full benchmark suite with BenchmarkDotNet

### Features
- **Performance**: MemoryPack-level performance with enterprise safety
- **Schema Safety**: Cryptographic hash validation prevents deployment crashes
- **AOT Compatible**: Full ahead-of-time compilation support
- **Enterprise Ready**: Production monitoring, metrics, and compliance features
- **Developer Experience**: Source generation with comprehensive IntelliSense support

### Dependencies
- Microsoft.Extensions.Caching.Hybrid (>= 10.1.0)
- MemoryPack (>= 1.21.4)
- .NET 10.0+

### Documentation
- Comprehensive README with architecture overview
- Performance benchmarks and comparisons
- Schema evolution safety demonstrations
- API documentation and examples
- Enterprise integration guides

---

## Types of changes
- `Added` for new features
- `Changed` for changes in existing functionality
- `Deprecated` for soon-to-be removed features
- `Removed` for now removed features
- `Fixed` for any bug fixes
- `Security` in case of vulnerabilities