// Copyright (c) 2025 Digvijay Chauhan
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rapp;
using System.Buffers;
using System.Text.Json;

BenchmarkRunner.Run<Rapp.Benchmark.Benchmarks>(args: args);

namespace Rapp.Benchmark
{

[RappCache]
[MemoryPack.MemoryPackable]
public partial class TestData
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime Timestamp { get; set; }
    public List<int>? Numbers { get; set; }
}

[MemoryPack.MemoryPackable]
public partial class TestDataDirect
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime Timestamp { get; set; }
    public List<int>? Numbers { get; set; }
}

// Rapp serializer for HybridCache
public class RappHybridSerializer : IHybridCacheSerializer<TestData>
{
    private readonly TestDataRappSerializer _serializer = new();

    public void Serialize(TestData value, IBufferWriter<byte> target)
    {
        _serializer.Serialize(value, target);
    }

    public TestData Deserialize(ReadOnlySequence<byte> source)
    {
        return _serializer.Deserialize(source);
    }
}

// MemoryPack serializer for HybridCache
public class MemoryPackHybridSerializer : IHybridCacheSerializer<TestDataDirect>
{
    public void Serialize(TestDataDirect value, IBufferWriter<byte> target)
    {
        MemoryPack.MemoryPackSerializer.Serialize(target, value);
    }

    public TestDataDirect Deserialize(ReadOnlySequence<byte> source)
    {
        return MemoryPack.MemoryPackSerializer.Deserialize<TestDataDirect>(source)!;
    }
}

public class Benchmarks
{
    private TestData? _testData;
    private TestDataDirect? _testDataDirect;
    private byte[]? _rappSerialized;
    private byte[]? _memoryPackSerialized;
    private byte[]? _jsonSerialized;
    private IMemoryCache? _memoryCache;
    private HybridCache? _hybridCache;
    private string[] _cacheKeys = null!;
    private IHost? _host;

    [GlobalSetup]
    public void Setup()
    {
        _testData = new TestData
        {
            Id = 42,
            Name = "Benchmark Test Data",
            Timestamp = DateTime.UtcNow,
            Numbers = Enumerable.Range(0, 100).ToList()
        };

        _testDataDirect = new TestDataDirect
        {
            Id = 42,
            Name = "Benchmark Test Data",
            Timestamp = DateTime.UtcNow,
            Numbers = Enumerable.Range(0, 100).ToList()
        };

        // Pre-serialize for accurate deserialization benchmarks
        var serializer = new TestDataRappSerializer();
        var writer = new ArrayBufferWriter<byte>();
        serializer.Serialize(_testData, writer);
        _rappSerialized = writer.WrittenSpan.ToArray();
        
        _memoryPackSerialized = MemoryPack.MemoryPackSerializer.Serialize(_testDataDirect);
        _jsonSerialized = JsonSerializer.SerializeToUtf8Bytes(_testDataDirect);

        // Create cache keys for realistic cache simulation
        _cacheKeys = new string[1000];
        for (int i = 0; i < _cacheKeys.Length; i++)
        {
            _cacheKeys[i] = $"key_{i}";
        }

        // Setup services using Host.CreateApplicationBuilder (provides keyed services support for HybridCache)
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddMemoryCache();
        builder.Services.AddHybridCache()
            .AddSerializer<TestData, RappHybridSerializer>()
            .AddSerializer<TestDataDirect, MemoryPackHybridSerializer>();
        
        _host = builder.Build();
        
        // Get services from host
        _memoryCache = _host.Services.GetRequiredService<IMemoryCache>();
        _hybridCache = _host.Services.GetRequiredService<HybridCache>();

        // Pre-populate some cache entries for realistic hit/miss patterns (80% hit rate)
        for (int i = 0; i < 800; i++)
        {
            var key = _cacheKeys[i];
            _memoryCache.Set(key, _testData, TimeSpan.FromMinutes(5));
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _host?.Dispose();
    }

    // ========== Pure Serialization Benchmarks (6) ==========
    
    [Benchmark]
    public byte[] RappSerialize()
    {
        if (_testData is null) throw new InvalidOperationException();
        var serializer = new TestDataRappSerializer();
        var writer = new ArrayBufferWriter<byte>();
        serializer.Serialize(_testData, writer);
        return writer.WrittenSpan.ToArray();
    }

    [Benchmark]
    public TestData RappDeserialize()
    {
        if (_rappSerialized is null) throw new InvalidOperationException();
        var serializer = new TestDataRappSerializer();
        var sequence = new ReadOnlySequence<byte>(_rappSerialized);
        return serializer.Deserialize(sequence);
    }

    [Benchmark]
    public byte[] MemoryPackSerialize()
    {
        if (_testDataDirect is null) throw new InvalidOperationException();
        return MemoryPack.MemoryPackSerializer.Serialize(_testDataDirect);
    }

    [Benchmark]
    public TestDataDirect MemoryPackDeserialize()
    {
        if (_memoryPackSerialized is null) throw new InvalidOperationException();
        return MemoryPack.MemoryPackSerializer.Deserialize<TestDataDirect>(_memoryPackSerialized)!;
    }

    [Benchmark]
    public byte[] JsonSerialize()
    {
        if (_testDataDirect is null) throw new InvalidOperationException();
        return JsonSerializer.SerializeToUtf8Bytes(_testDataDirect);
    }

    [Benchmark]
    public TestDataDirect JsonDeserialize()
    {
        if (_jsonSerialized is null) throw new InvalidOperationException();
        return JsonSerializer.Deserialize<TestDataDirect>(_jsonSerialized)!;
    }

    // ========== HybridCache Integration Benchmarks (3) ==========

    [Benchmark]
    public async Task<TestData> HybridCache_Rapp()
    {
        if (_hybridCache is null || _testData is null) throw new InvalidOperationException();
        return await _hybridCache.GetOrCreateAsync(
            "test_key_rapp",
            _ => ValueTask.FromResult(_testData!));
    }

    [Benchmark]
    public async Task<TestDataDirect> HybridCache_MemoryPack()
    {
        if (_hybridCache is null || _testDataDirect is null) throw new InvalidOperationException();
        return await _hybridCache.GetOrCreateAsync(
            "test_key_memorypack",
            _ => ValueTask.FromResult(_testDataDirect!));
    }

    [Benchmark]
    public TestData DirectMemoryCache()
    {
        if (_memoryCache is null || _testData is null) throw new InvalidOperationException();
        return _memoryCache.GetOrCreate("test_key_direct", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return _testData;
        })!;
    }

    // ========== Realistic Cache Workload Benchmarks (3) ==========

    [Benchmark]
    public async Task RealisticWorkload_HybridCache_Rapp()
    {
        if (_hybridCache is null || _testData is null) throw new InvalidOperationException();
        
        // Simulate 100 cache operations with 80% hit rate
        for (int i = 0; i < 100; i++)
        {
            var key = _cacheKeys[Random.Shared.Next(_cacheKeys.Length)];
            await _hybridCache.GetOrCreateAsync(key, _ => ValueTask.FromResult(_testData!));
        }
    }

    [Benchmark]
    public async Task RealisticWorkload_HybridCache_MemoryPack()
    {
        if (_hybridCache is null || _testDataDirect is null) throw new InvalidOperationException();
        
        // Simulate 100 cache operations with 80% hit rate
        for (int i = 0; i < 100; i++)
        {
            var key = _cacheKeys[Random.Shared.Next(_cacheKeys.Length)];
            await _hybridCache.GetOrCreateAsync(key, _ => ValueTask.FromResult(_testDataDirect!));
        }
    }

    [Benchmark]
    public void RealisticWorkload_DirectMemory()
    {
        if (_memoryCache is null || _testData is null) throw new InvalidOperationException();

        // Simulate 100 cache operations with 80% hit rate
        for (int i = 0; i < 100; i++)
        {
            var key = _cacheKeys[Random.Shared.Next(_cacheKeys.Length)];
            _memoryCache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(5);
                return _testData;
            });
        }
    }
}
}
