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

using FluentAssertions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using System.Buffers;
using System.Diagnostics;
using Xunit;

namespace Rapp.Tests;

[RappCache]
[MemoryPack.MemoryPackable]
public partial class PerformanceTestData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int[] Values { get; set; } = System.Array.Empty<int>();
    public string Description { get; set; } = string.Empty;
}

public class PerformanceRegressionTests
{
    [Fact]
    public void Serialization_Should_Be_Fast()
    {
        // Arrange
        var assembly = typeof(PerformanceTestData).Assembly;
        var serializerType = assembly.GetType("Rapp.PerformanceTestDataRappSerializer");
        var serializer = (IHybridCacheSerializer<PerformanceTestData>)System.Activator.CreateInstance(serializerType!)!;

        var data = new PerformanceTestData
        {
            Id = 123,
            Name = "Performance Test",
            Values = new[] { 1, 2, 3, 4, 5 },
            Description = "Testing serialization performance"
        };

        var buffer = new ArrayBufferWriter<byte>();

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            buffer.Clear();
            serializer.Serialize(data, buffer);
        }
        stopwatch.Stop();

        // Assert - Should be reasonably fast (less than 50ms for 1000 operations)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    [Fact]
    public void Deserialization_Should_Be_Fast()
    {
        // Arrange
        var assembly = typeof(PerformanceTestData).Assembly;
        var serializerType = assembly.GetType("Rapp.PerformanceTestDataRappSerializer");
        var serializer = (IHybridCacheSerializer<PerformanceTestData>)System.Activator.CreateInstance(serializerType!)!;

        var data = new PerformanceTestData
        {
            Id = 456,
            Name = "Performance Test Deserialize",
            Values = new[] { 6, 7, 8, 9, 10 },
            Description = "Testing deserialization performance"
        };

        // Pre-serialize
        var buffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(data, buffer);
        var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            var result = serializer.Deserialize(sequence);
            result.Should().NotBeNull();
        }
        stopwatch.Stop();

        // Assert - Should be reasonably fast (less than 50ms for 1000 operations)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    [Fact]
    public async Task HybridCache_Operations_Should_Be_Fast()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForPerformanceTestData();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var data = new PerformanceTestData
        {
            Id = 789,
            Name = "Cache Performance Test",
            Values = System.Linq.Enumerable.Range(1, 100).ToArray(),
            Description = "Testing cache operation performance"
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            var result = await cache.GetOrCreateAsync(
                $"perf-test-{i}",
                async ct => data);
            result.Should().BeEquivalentTo(data);
        }
        stopwatch.Stop();

        // Assert - Should be reasonably fast (less than 1500ms for 100 operations with tolerance for various environments)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1500);
    }

    [Fact]
    public void Memory_Usage_Should_Be_Reasonable()
    {
        // Arrange
        var assembly = typeof(PerformanceTestData).Assembly;
        var serializerType = assembly.GetType("Rapp.PerformanceTestDataRappSerializer");
        var serializer = (IHybridCacheSerializer<PerformanceTestData>)System.Activator.CreateInstance(serializerType!)!;

        var data = new PerformanceTestData
        {
            Id = 999,
            Name = new string('A', 1000), // Large string
            Values = System.Linq.Enumerable.Range(1, 1000).ToArray(), // Large array
            Description = new string('B', 1000) // Another large string
        };

        // Act
        var buffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(data, buffer);

        // Assert - Serialized size should be reasonable (less than 10KB for this data)
        buffer.WrittenCount.Should().BeLessThan(10 * 1024);
    }

    [Fact]
    public void Schema_Hash_Validation_Should_Be_Fast()
    {
        // Arrange
        var serializer = new TestPerformanceSerializer();

        // Create valid data
        var buffer = new ArrayBufferWriter<byte>();
        var value = "Performance test value";
        serializer.Serialize(value, buffer);

        var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
        {
            var result = serializer.Deserialize(sequence);
            result.Should().Be(value);
        }
        stopwatch.Stop();

        // Assert - Hash validation should be very fast (less than 500ms for 10000 operations with tolerance)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public void Concurrent_Operations_Should_Maintain_Performance()
    {
        // Arrange
        var serializer = new TestPerformanceSerializer();

        // Act
        var stopwatch = Stopwatch.StartNew();
        System.Threading.Tasks.Parallel.For(0, 100, i =>
        {
            var buffer = new ArrayBufferWriter<byte>();
            var value = $"Concurrent value {i}";
            serializer.Serialize(value, buffer);

            var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
            var result = serializer.Deserialize(sequence);
            result.Should().Be(value);
        });
        stopwatch.Stop();

        // Assert - Should complete in reasonable time (less than 500ms with tolerance for various environments)
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    private class TestPerformanceSerializer : RappBaseSerializer<string>
    {
        protected override ulong SchemaHash => 987654321UL;
        protected override string TypeName => "string";
    }
}