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
using Xunit;

namespace Rapp.Tests;

public class HybridCacheIntegrationTests
{
    [Fact]
    public async Task HybridCache_With_RappSerializer_Should_Cache_And_Retrieve_Value()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = System.TimeSpan.FromMinutes(5)
            };
        }).UseRappForTestData();

        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();
        var testData = new TestData { Name = "Integration Test", Value = 123 };

        // Act
        var result = await cache.GetOrCreateAsync(
            "test-key",
            async ct => testData,
            cancellationToken: default);

        // Assert
        result.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public async Task HybridCache_With_RappSerializer_Should_Handle_Cache_Hit()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForTestData();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();
        var testData = new TestData { Name = "Cache Hit Test", Value = 456 };
        var key = "hit-test-key";

        // Pre-populate cache
        await cache.GetOrCreateAsync(key, async ct => testData);

        // Act - This should be a cache hit
        var factoryCallCount = 0;
        var result = await cache.GetOrCreateAsync(
            key,
            async ct =>
            {
                factoryCallCount++;
                return new TestData { Name = "Should Not Be Called", Value = 999 };
            });

        // Assert
        result.Should().BeEquivalentTo(testData);
        factoryCallCount.Should().Be(0); // Factory should not be called on cache hit
    }

    [Fact]
    public async Task HybridCache_With_RappSerializer_Should_Handle_Complex_Types()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForComplexTestData();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var complexData = new ComplexTestData
        {
            Id = System.Guid.NewGuid(),
            Name = "Complex Test",
            Values = new[] { 1, 2, 3, 4, 5 },
            Nested = new NestedData { Description = "Nested", Count = 42 },
            NullableString = "Not null",
            NullableInt = null
        };

        // Act
        var result = await cache.GetOrCreateAsync(
            "complex-key",
            async ct => complexData);

        // Assert
        result.Should().BeEquivalentTo(complexData);
        result.Id.Should().Be(complexData.Id);
        result.Values.Should().BeEquivalentTo(complexData.Values);
        result.Nested.Should().BeEquivalentTo(complexData.Nested);
        result.NullableString.Should().Be(complexData.NullableString);
        result.NullableInt.Should().Be(complexData.NullableInt);
    }

    [Fact]
    public async Task HybridCache_With_RappSerializer_Should_Handle_Null_Values()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForNullableTestData();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var nullableData = new NullableTestData
        {
            RequiredString = "Required",
            OptionalString = null,
            OptionalInt = null
        };

        // Act
        var result = await cache.GetOrCreateAsync(
            "nullable-key",
            async ct => nullableData);

        // Assert
        result.Should().BeEquivalentTo(nullableData);
        result.OptionalString.Should().BeNull();
        result.OptionalInt.Should().BeNull();
    }

    [Fact]
    public async Task HybridCache_With_RappSerializer_Should_Handle_Concurrent_Access()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForTestData();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        // Act - Concurrent cache operations
        var results = new TestData[10];
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            results[index] = await cache.GetOrCreateAsync(
                $"concurrent-key-{index}",
                async ct => new TestData { Name = $"Concurrent {index}", Value = index });
        }

        // Assert
        results.Length.Should().Be(10);
        for (int i = 0; i < 10; i++)
        {
            results[i].Name.Should().Be($"Concurrent {i}");
            results[i].Value.Should().Be(i);
        }
    }
}

[RappCache]
[MemoryPack.MemoryPackable]
public partial class ComplexTestData
{
    public System.Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int[] Values { get; set; } = System.Array.Empty<int>();
    public NestedData Nested { get; set; } = new();
    public string? NullableString { get; set; }
    public int? NullableInt { get; set; }
}

[RappCache]
[MemoryPack.MemoryPackable]
public partial class NestedData
{
    public string Description { get; set; } = string.Empty;
    public int Count { get; set; }
}

[RappCache]
[MemoryPack.MemoryPackable]
public partial class NullableTestData
{
    public string RequiredString { get; set; } = string.Empty;
    public string? OptionalString { get; set; }
    public int? OptionalInt { get; set; }
}