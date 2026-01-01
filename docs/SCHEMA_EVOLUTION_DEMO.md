# Rapp Schema Evolution Safety Demo

This demo showcases the critical difference between MemoryPack's high-performance binary serialization and Rapp's enterprise-grade schema safety in .NET 10 applications.

## Overview

The demo illustrates how schema changes during continuous deployment can cause catastrophic failures with MemoryPack, while Rapp handles these scenarios gracefully without crashes.

## Running the Demo

### Prerequisites
- .NET 10.0 SDK
- Access to a terminal

### Quick Start

1. **Navigate to the playground project:**
   ```bash
   cd src/Rapp.Playground
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Open your browser and visit the demo endpoints:**
   - **MemoryPack Crash Demo:** `http://localhost:5000/demo/memorypack-crash`
   - **Rapp Safety Demo:** `http://localhost:5000/demo/rapp-safety`
   - **JSON Comparison:** `http://localhost:5000/demo/json-comparison`

### PowerShell Script (Windows)

```powershell
# scripts/Run-SchemaEvolutionDemo.ps1
param(
    [string]$Endpoint = "memorypack-crash"
)

$baseUrl = "http://localhost:5000"

# Start the demo application in background
Start-Job -ScriptBlock {
    Set-Location "src/Rapp.Playground"
    dotnet run
} -Name "RappDemo"

# Wait for app to start
Start-Sleep -Seconds 3

# Test the endpoints
$endpoints = @("memorypack-crash", "rapp-safety", "json-comparison")

foreach ($ep in $endpoints) {
    Write-Host "Testing /$ep endpoint..." -ForegroundColor Cyan
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/demo/$ep" -Method Get
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor Green
    } catch {
        Write-Host "Error calling $ep`: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host "`n"
}

# Stop the background job
Stop-Job -Name "RappDemo"
Remove-Job -Name "RappDemo"
```

### Bash Script (Linux/macOS)

```bash
#!/bin/bash
# scripts/run-schema-evolution-demo.sh

BASE_URL="http://localhost:5000"
PROJECT_DIR="src/Rapp.Playground"

# Function to test endpoint
test_endpoint() {
    local endpoint=$1
    local description=$2

    echo "=========================================="
    echo "Testing: $description"
    echo "Endpoint: $endpoint"
    echo "=========================================="

    curl -s "$BASE_URL$endpoint" | jq '.' 2>/dev/null || curl -s "$BASE_URL$endpoint"
    echo -e "\n"
}

# Start the application in background
echo "Starting Rapp Playground application..."
cd "$PROJECT_DIR"
dotnet run &
APP_PID=$!

# Wait for app to start
sleep 3

# Test all demo endpoints
test_endpoint "/demo/memorypack-crash" "MemoryPack Crash Scenario"
test_endpoint "/demo/rapp-safety" "Rapp Safety Demonstration"
test_endpoint "/demo/json-comparison" "System.Text.Json Comparison"

# Cleanup
echo "Stopping demo application..."
kill $APP_PID 2>/dev/null
wait $APP_PID 2>/dev/null

echo "Demo completed successfully!"
```

## What the Demo Shows

### 1. MemoryPack Crash Scenario (`/demo/memorypack-crash`)

Demonstrates how MemoryPack fails catastrophically when schema changes occur:

- **v1.0** serializes data with original schema
- **v2.0** attempts to deserialize with incompatible schema (removed/reordered properties)
- **Result:** `SerializationException` crashes the application

**Real-World Impact:**
- Production outages during deployments
- Data loss in distributed cache scenarios
- Emergency rollbacks required

### 2. Rapp Safety Demonstration (`/demo/rapp-safety`)

Shows how Rapp handles the same scenario gracefully:

- **Schema Hashing:** Cryptographic validation of data compatibility
- **Automatic Invalidation:** Incompatible data treated as cache miss
- **Zero Downtime:** Application continues running, fetches fresh data

**Enterprise Benefits:**
- Safe continuous deployment
- Automatic cache management
- No crash-induced outages

### 3. JSON Comparison (`/demo/json-comparison`)

Compares with System.Text.Json behavior:

- **Schema Flexibility:** Handles missing/extra properties gracefully
- **Performance Cost:** 4.7x slower serialization, 9.3x slower deserialization
- **AOT Compatibility:** ✅ Demo uses source-generated JSON (AOT-compatible) vs. reflection mode (AOT-incompatible)
- **Payload Bloat:** ~60% larger than binary formats

**AOT Compatibility Note:** The demo uses `JsonSerializerContext` for AOT compatibility. Standard reflection-based JSON serialization would trigger IL2026/IL3050 warnings and fail AOT compilation.

## Technical Details

### Schema Evolution Scenarios Tested

| Change Type | MemoryPack Behavior | Rapp Behavior |
|-------------|-------------------|---------------|
| **Add Property** | ✅ Compatible | ✅ Compatible |
| **Remove Property** | ❌ Crash | ✅ Safe (cache miss) |
| **Reorder Properties** | ❌ Crash | ✅ Safe (cache miss) |
| **Change Property Type** | ❌ Crash | ✅ Safe (cache miss) |

### Performance Comparison

Based on BenchmarkDotNet analysis (.NET 10.0, Intel Core i7):

| Serializer | Serialization | Deserialization | Payload Size |
|------------|---------------|-----------------|--------------|
| **Rapp** | 397.2 ns | 240.9 ns | ~40% of JSON |
| **MemoryPack (raw)** | 197.0 ns | 180.0 ns | ~33% of JSON |
| **System.Text.Json** | 1,764.1 ns | 4,238.1 ns | 100% (baseline) |

**HybridCache Integration:** Rapp 436.9ns (single), 30.5μs (100 ops, 80% hit). MemoryPack 416.5ns (single), 44.1μs (100 ops, 80% hit).

## Key Takeaways

1. **MemoryPack** offers excellent raw performance but requires strict schema compatibility
2. **Rapp** adds 102% serialize / 34% deserialize overhead for enterprise-grade safety and observability
3. **System.Text.Json** provides safety but with significant performance penalties (4.4× slower serialize, 17.6× slower deserialize vs Rapp)
4. **HybridCache:** Rapp is **31% faster** than MemoryPack in realistic workloads (30.5μs vs 44.1μs)
5. **Compile-time optimizations** make Rapp production-ready for enterprise .NET 10 applications

## Production Deployment Safety

This demo validates Rapp's role in enabling safe binary caching adoption:

- **CI/CD Pipelines:** No more deployment-induced crashes
- **Distributed Caching:** Redis/memory cache stability
- **Microservices:** Schema evolution without service coordination
- **Enterprise Scale:** Zero-downtime deployments at cloud scale

---

**Note:** This demo is designed for educational purposes and to validate Rapp's safety claims. The actual crash scenarios are simulated to prevent real system failures during demonstration.