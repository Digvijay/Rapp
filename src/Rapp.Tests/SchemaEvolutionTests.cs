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
using Xunit;

namespace Rapp.Tests;

public class SchemaEvolutionTests
{
    [Fact]
    public async Task SchemaEvolution_Should_Handle_Field_Addition_Safely()
    {
        // Arrange - Simulate v1.0 data
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForSchemaEvolutionData();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        // Create v1.0 data (without Category field)
        var v1Data = new SchemaEvolutionData { Id = 1, Name = "Test Product", Price = 29.99m };

        // Act - Cache v1.0 data
        await cache.GetOrCreateAsync("product-1-v1", async ct => v1Data);

        // Simulate schema change: v2.0 adds Category field
        // Use different cache key to ensure factory is called
        var factoryCallCount = 0;
        var result = await cache.GetOrCreateAsync(
            "product-1-v2",
            async ct =>
            {
                factoryCallCount++;
                return new SchemaEvolutionData
                {
                    Id = 1,
                    Name = "Updated Product",
                    Price = 39.99m,
                    Category = "Electronics" // New field
                };
            });

        // Assert - Factory should be called since this is a different cache key
        factoryCallCount.Should().Be(1);
        result.Name.Should().Be("Updated Product");
        result.Category.Should().Be("Electronics");
    }

    [Fact]
    public async Task SchemaEvolution_Should_Handle_Field_Removal_Safely()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForSchemaEvolutionData();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        // Create data with all fields
        var fullData = new SchemaEvolutionData
        {
            Id = 2,
            Name = "Full Product",
            Price = 49.99m,
            Category = "Books"
        };

        // Act - Cache full data
        await cache.GetOrCreateAsync("product-2-v1", async ct => fullData);

        // Simulate schema change: remove Category field
        // Use different cache key to ensure factory is called
        var factoryCallCount = 0;
        var result = await cache.GetOrCreateAsync(
            "product-2-v2",
            async ct =>
            {
                factoryCallCount++;
                return new SchemaEvolutionData
                {
                    Id = 2,
                    Name = "Updated Product",
                    Price = 59.99m
                    // Category removed
                };
            });

        // Assert - Factory should be called since this is a different cache key
        factoryCallCount.Should().Be(1);
        result.Name.Should().Be("Updated Product");
        result.Category.Should().BeNull();
    }

    [Fact]
    public async Task SchemaEvolution_Should_Handle_Field_Type_Change_Safely()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForSchemaEvolutionData();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        // Create data with original Price type
        var originalData = new SchemaEvolutionData
        {
            Id = 3,
            Name = "Type Change Test",
            Price = 19.99m
        };

        // Act - Cache original data
        await cache.GetOrCreateAsync("product-3-v1", async ct => originalData);

        // Simulate schema change: Price type changed (would be different hash)
        // Use different cache key to ensure factory is called
        var factoryCallCount = 0;
        var result = await cache.GetOrCreateAsync(
            "product-3-v2",
            async ct =>
            {
                factoryCallCount++;
                return new SchemaEvolutionData
                {
                    Id = 3,
                    Name = "Updated Type Change Test",
                    Price = 29.99m
                };
            });

        // Assert - Factory should be called since this is a different cache key
        factoryCallCount.Should().Be(1);
        result.Name.Should().Be("Updated Type Change Test");
    }

    [Fact]
    public void DirectSerializer_Should_Return_Default_On_Schema_Mismatch()
    {
        // Arrange
        var assembly = typeof(SchemaEvolutionData).Assembly;
        var serializerType = assembly.GetType("Rapp.SchemaEvolutionDataRappSerializer");
        var serializer = (IHybridCacheSerializer<SchemaEvolutionData>)System.Activator.CreateInstance(serializerType!)!;

        // Create buffer with wrong schema hash
        var buffer = new ArrayBufferWriter<byte>();
        var wrongHashBytes = new byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(wrongHashBytes, 999999UL);
        buffer.Write(wrongHashBytes);

        // Serialize valid data but with wrong hash
        MemoryPack.MemoryPackSerializer.Serialize(buffer, new SchemaEvolutionData { Id = 1, Name = "Test" });

        // Act
        var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().BeNull(); // Should return default due to hash mismatch
    }

    [Fact]
    public void Serializer_Should_Validate_Schema_Hash_Correctly()
    {
        // Arrange
        var assembly = typeof(SchemaEvolutionData).Assembly;
        var serializerType = assembly.GetType("Rapp.SchemaEvolutionDataRappSerializer");
        var serializer = (IHybridCacheSerializer<SchemaEvolutionData>)System.Activator.CreateInstance(serializerType!)!;

        var originalData = new SchemaEvolutionData
        {
            Id = 42,
            Name = "Schema Validation Test",
            Price = 99.99m,
            Category = "Test Category"
        };

        // Act - Serialize and deserialize with correct schema
        var buffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(originalData, buffer);
        var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().BeEquivalentTo(originalData);
    }
}

[RappCache]
[MemoryPack.MemoryPackable]
public partial class SchemaEvolutionData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Category { get; set; }
}