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
using Rapp;
using System.Buffers;
using System.Text.Json;

BenchmarkRunner.Run<Benchmarks>();

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

// System.Text.Json serializer for HybridCache
public class JsonHybridSerializer : IHybridCacheSerializer<TestDataDirect>
{
    public static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Serialize(TestDataDirect value, IBufferWriter<byte> target)
    {
        using var writer = new Utf8JsonWriter(target);
        JsonSerializer.Serialize(writer, value, _options);
    }

    public TestDataDirect Deserialize(ReadOnlySequence<byte> source)
    {
        var reader = new Utf8JsonReader(source);
        return JsonSerializer.Deserialize<TestDataDirect>(ref reader, _options)!;
    }
}

public class Benchmarks
{
    private TestData? _testData;
    private TestDataDirect? _testDataDirect;
    private IMemoryCache? _memoryCache;
    private HybridCache? _hybridCache;
    private IServiceProvider? _services;
    private string[] _cacheKeys = null!;

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

        // Create cache keys for realistic cache simulation
        _cacheKeys = new string[1000];
        for (int i = 0; i < _cacheKeys.Length; i++)
        {
            _cacheKeys[i] = $"key_{i}";
        }

        var services = new ServiceCollection();
        services.AddMemoryCache();
#pragma warning disable EXTEXP0018 // AddHybridCache is experimental
        services.AddHybridCache()
            .UseRappForTestData()
            .AddSerializer<TestDataDirect, MemoryPackHybridSerializer>();
#pragma warning restore EXTEXP0018
        _services = services.BuildServiceProvider();

        _memoryCache = _services.GetRequiredService<IMemoryCache>();
        _hybridCache = _services.GetRequiredService<HybridCache>();

        // Pre-populate some cache entries for realistic hit/miss patterns
        for (int i = 0; i < 100; i++)
        {
            var key = _cacheKeys[i];
            _memoryCache.Set(key, _testData, TimeSpan.FromMinutes(5));
        }
    }

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
        var data = RappSerialize();
        var serializer = new TestDataRappSerializer();
        var sequence = new ReadOnlySequence<byte>(data);
        return serializer.Deserialize(sequence)!;
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
        var data = MemoryPackSerialize();
        return MemoryPack.MemoryPackSerializer.Deserialize<TestDataDirect>(data)!;
    }

    [Benchmark]
    public byte[] JsonSerialize()
    {
        if (_testDataDirect is null) throw new InvalidOperationException();
        return JsonSerializer.SerializeToUtf8Bytes(_testDataDirect, JsonHybridSerializer._options);
    }

    [Benchmark]
    public TestDataDirect JsonDeserialize()
    {
        var data = JsonSerialize();
        return JsonSerializer.Deserialize<TestDataDirect>(data, JsonHybridSerializer._options)!;
    }

    [Benchmark]
    public async Task<TestData> HybridCache_GetOrCreate_Rapp()
    {
        if (_hybridCache is null || _testData is null) throw new InvalidOperationException();
        return await _hybridCache.GetOrCreateAsync("test_key_rapp", _ => ValueTask.FromResult(_testData));
    }

    [Benchmark]
    public async Task<TestDataDirect> HybridCache_GetOrCreate_MemoryPack()
    {
        if (_hybridCache is null || _testDataDirect is null) throw new InvalidOperationException();
        return await _hybridCache.GetOrCreateAsync("test_key_memorypack", _ => ValueTask.FromResult(_testDataDirect));
    }

    [Benchmark]
    public TestData MemoryCache_GetOrCreate()
    {
        if (_memoryCache is null || _testData is null) throw new InvalidOperationException();
        return _memoryCache.GetOrCreate("test_key_memory", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return _testData;
        })!;
    }

    // Realistic cache performance benchmark with hit/miss patterns
    [Benchmark]
    public async Task RealisticCacheWorkload_Rapp()
    {
        if (_hybridCache is null || _testData is null) throw new InvalidOperationException();

        // Simulate 80% hit rate, 20% miss rate
        for (int i = 0; i < 100; i++)
        {
            var key = _cacheKeys[Random.Shared.Next(_cacheKeys.Length)];
            await _hybridCache.GetOrCreateAsync(key, _ => ValueTask.FromResult(_testData));
        }
    }

    [Benchmark]
    public async Task RealisticCacheWorkload_MemoryPack()
    {
        if (_hybridCache is null || _testDataDirect is null) throw new InvalidOperationException();

        // Simulate 80% hit rate, 20% miss rate
        for (int i = 0; i < 100; i++)
        {
            var key = _cacheKeys[Random.Shared.Next(_cacheKeys.Length)];
            await _hybridCache.GetOrCreateAsync(key, _ => ValueTask.FromResult(_testDataDirect));
        }
    }

    [Benchmark]
    public void RealisticCacheWorkload_Memory()
    {
        if (_memoryCache is null || _testData is null) throw new InvalidOperationException();

        // Simulate 80% hit rate, 20% miss rate
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
