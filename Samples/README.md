# Rapp Samples

This directory contains example projects demonstrating how to use Rapp in various scenarios.

## 📂 Sample Projects

### 1. **AspNetCoreMinimalApi**
Demonstrates Rapp integration with ASP.NET Core Minimal APIs.

**Features:**
- HybridCache configuration
- RESTful API endpoints
- **Interactive Dashboard** (at root URL)
- **Real-time Cost Savings Analysis**
- Schema evolution handling
- Health checks

**Run:**
```bash
cd AspNetCoreMinimalApi
dotnet run
```

### 2. **ConsoleApp**
Simple console application showing basic Rapp usage.

**Features:**
- Standalone cache operations
- Serialization/deserialization
- Performance measurement
- Error handling

**Run:**
```bash
cd ConsoleApp
dotnet run
```

### 3. **GrpcService**
gRPC service using Rapp for high-performance caching.

**Features:**
- gRPC service implementation
- **gRPC JSON Transcoding**
- **Unified Rapp Dashboard**
- **Cost Savings Metrics**
- Binary serialization for gRPC messages
- Distributed caching patterns
- Load testing

**Run:**
```bash
cd GrpcService
dotnet run
```

## 🚀 Getting Started

1. **Build all samples:**
   ```bash
   dotnet build Rapp.Samples.sln
   ```

2. **Run a specific sample:**
   ```bash
   cd <SampleName>
   dotnet run
   ```

3. **Run with watch mode:**
   ```bash
   cd <SampleName>
   dotnet watch run
   ```

## 📚 Learning Path

Recommended order for exploring the samples:

1. Start with **ConsoleApp** to understand basics
2. Move to **AspNetCoreMinimalApi** for web integration
3. Explore **GrpcService** for advanced scenarios

## 🔗 Additional Resources

- [Main Documentation](../README.md)
- [Migration Guide](../docs/MIGRATION_GUIDE.md)
- [Technical Summary](../docs/TECHNICAL_SUMMARY.md)

## 💡 Need Help?

- Check the README in each sample project
- Review inline code comments
- Open an issue in the main repository
