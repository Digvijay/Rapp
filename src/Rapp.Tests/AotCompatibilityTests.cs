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
using System.Reflection;
using Xunit;

namespace Rapp.Tests;

[RappCache]
[MemoryPack.MemoryPackable]
public partial class AotTestData
{
    public System.Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public bool IsActive { get; set; }
}

public class AotCompatibilityTests
{
    [Fact]
    public void Generated_Serializer_Should_Not_Use_Reflection()
    {
        // Arrange
        var assembly = typeof(AotTestData).Assembly;
        var serializerType = assembly.GetType("Rapp.AotTestDataRappSerializer");

        // Act
        var methods = serializerType?.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var reflectionMethods = methods?.Where(m =>
            m.Name.Contains("Invoke") ||
            m.Name.Contains("CreateInstance") ||
            m.Name.Contains("GetMethod") ||
            m.Name.Contains("GetProperty") ||
            m.Name.Contains("GetField")).ToList();

        // Assert - Should not contain problematic reflection methods
        if (reflectionMethods != null)
        {
            reflectionMethods.Should().BeEmpty();
        }
    }

    [Fact]
    public void Generated_Serializer_Should_Be_Stateless()
    {
        // Arrange
        var assembly = typeof(AotTestData).Assembly;
        var serializerType = assembly.GetType("Rapp.AotTestDataRappSerializer");

        // Act
        var instanceFields = serializerType?.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var instanceProperties = serializerType?.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Assert - Should have minimal instance state (only expected properties like SchemaHash)
        if (instanceFields != null)
        {
            instanceFields.Should().BeEmpty(); // No instance fields
        }
        // Properties like SchemaHash are acceptable as they are computed properties
    }

    [Fact]
    public void Generated_Code_Should_Use_Only_AOT_Compatible_APIs()
    {
        // Arrange
        var assembly = typeof(AotTestData).Assembly;
        var serializerType = assembly.GetType("Rapp.AotTestDataRappSerializer");

        // Act
        var constructor = serializerType?.GetConstructor(Type.EmptyTypes);
        var serializer = (IHybridCacheSerializer<AotTestData>)constructor?.Invoke(null)!;

        // Assert - Should be able to create instance without reflection
        serializer.Should().NotBeNull();
    }

    [Fact]
    public void Generated_Extension_Method_Should_Be_AOT_Compatible()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw when configuring services
        services.AddHybridCache().UseRappForAotTestData();

        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        // Should be able to resolve cache without issues
        cache.Should().NotBeNull();
    }

    [Fact]
    public async Task AOT_Compatible_Serializer_Should_Work_In_Practice()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHybridCache().UseRappForAotTestData();
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<HybridCache>();

        var testData = new AotTestData
        {
            Id = System.Guid.NewGuid(),
            Name = "AOT Compatibility Test",
            Value = 42,
            IsActive = true
        };

        // Act
        var result = await cache.GetOrCreateAsync(
            "aot-test-key",
            async ct => testData);

        // Assert
        result.Should().BeEquivalentTo(testData);
        result.Id.Should().Be(testData.Id);
        result.Name.Should().Be(testData.Name);
        result.Value.Should().Be(testData.Value);
        result.IsActive.Should().Be(testData.IsActive);
    }

    [Fact]
    public void Generated_Serializer_Should_Have_Required_Interfaces()
    {
        // Arrange
        var assembly = typeof(AotTestData).Assembly;
        var serializerType = assembly.GetType("Rapp.AotTestDataRappSerializer");

        // Act & Assert
        serializerType.Should().NotBeNull();
        serializerType!.Should().Implement<IHybridCacheSerializer<AotTestData>>();
        serializerType!.IsPublic.Should().BeTrue();
        serializerType!.IsAbstract.Should().BeFalse();
    }

    [Fact]
    public void Generated_Code_Should_Not_Contain_Dynamic_Code()
    {
        // Arrange
        var assembly = typeof(AotTestData).Assembly;
        var serializerType = assembly.GetType("Rapp.AotTestDataRappSerializer");

        // Act
        var methods = serializerType?.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var dynamicMethods = methods?.Where(m =>
            m.Name.Contains("Dynamic") ||
            m.Name.Contains("Compile") ||
            m.Name.Contains("Emit")).ToList();

        // Assert - Should not contain dynamic code generation methods
        if (dynamicMethods != null)
        {
            dynamicMethods.Should().BeEmpty();
        }
    }

    [Fact]
    public void Schema_Hash_Should_Be_Compile_Time_Constant()
    {
        // Arrange
        var assembly = typeof(AotTestData).Assembly;
        var serializerType = assembly.GetType("Rapp.AotTestDataRappSerializer");
        var serializer = (IHybridCacheSerializer<AotTestData>)System.Activator.CreateInstance(serializerType!)!;

        // Act
        var baseSerializer = serializer as RappBaseSerializer<AotTestData>;
        var schemaHash = baseSerializer?.GetType().GetProperty("SchemaHash",
            BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(baseSerializer);

        // Assert - Schema hash should be a compile-time constant
        schemaHash.Should().NotBeNull();
        schemaHash.Should().BeOfType<ulong>();
    }
}