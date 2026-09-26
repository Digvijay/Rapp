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
        serializer.Serialize(data, buffer);

        // Act
        var perOperation = AllocatedBytesPerOperation(1000, () =>
        {
            buffer.Clear();
            serializer.Serialize(data, buffer);
        });

        // Assert
        perOperation.Should().BeLessThanOrEqualTo(SerializeBudgetBytes);
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

        serializer.Deserialize(sequence).Should().BeEquivalentTo(data);

        // Act
        var perOperation = AllocatedBytesPerOperation(1000, () => serializer.Deserialize(sequence));

        // Assert
        perOperation.Should().BeLessThanOrEqualTo(DeserializeBudgetBytes);
    }

    [Fact]
    public async Task HybridCache_Operations_Should_Round_Trip()
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

        // Act and assert
        for (int i = 0; i < 100; i++)
        {
            var result = await cache.GetOrCreateAsync(
                $"perf-test-{i}",
                async ct => data);
            result.Should().BeEquivalentTo(data);
        }
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

        serializer.Deserialize(sequence).Should().Be(value);

        // Act
        var perOperation = AllocatedBytesPerOperation(10000, () => serializer.Deserialize(sequence));

        // Assert
        perOperation.Should().BeLessThanOrEqualTo(SchemaCheckedDeserializeBudgetBytes);
    }

    [Fact]
    public void Concurrent_Operations_Should_Be_Thread_Safe()
    {
        // Arrange
        var serializer = new TestPerformanceSerializer();

        // Act and assert
        System.Threading.Tasks.Parallel.For(0, 100, i =>
        {
            var buffer = new ArrayBufferWriter<byte>();
            var value = $"Concurrent value {i}";
            serializer.Serialize(value, buffer);

            var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
            var result = serializer.Deserialize(sequence);
            result.Should().Be(value);
        });
    }

    // Wall-clock thresholds were removed: they measured the machine rather than the code and failed
    // under parallel test execution. Allocation per operation is deterministic for a given runtime
    // and is what these tests exist to protect. Timing belongs in Rapp.Benchmark.
    //
    // Serialize writes into the caller's buffer and must allocate nothing. The deserialize budgets are
    // exactly the result graph on a 64-bit runtime: the object, its two strings and its int[5] (272 B),
    // and one 22-character string (72 B). Anything above that is overhead added by Rapp.
    private const long SerializeBudgetBytes = 0;
    private const long DeserializeBudgetBytes = 272;
    private const long SchemaCheckedDeserializeBudgetBytes = 72;

    private static long AllocatedBytesPerOperation(int iterations, Action operation)
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < iterations; i++)
        {
            operation();
        }
        return (GC.GetAllocatedBytesForCurrentThread() - before) / iterations;
    }

    private sealed class TestPerformanceSerializer : RappBaseSerializer<string>
    {
        private static readonly byte[] _hashBytes = BitConverter.GetBytes(987654321UL);
        protected override ulong SchemaHash => 987654321UL;
        protected override string TypeName => "string";
        protected override ReadOnlySpan<byte> GetSchemaHashBytes() => _hashBytes;
    }
}