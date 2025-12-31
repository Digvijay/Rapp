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
using System.Buffers;
using Xunit;

namespace Rapp.Tests;

public class SourceGeneratorTests
{
    [Fact]
    public void Generated_Serializer_Should_Exist()
    {
        // Arrange
        var assembly = typeof(TestData).Assembly;

        // Act
        var serializerType = assembly.GetType("Rapp.TestDataRappSerializer");

        // Assert
        serializerType.Should().NotBeNull();
        serializerType!.Should().Implement<IHybridCacheSerializer<TestData>>();
    }

    [Fact]
    public void Generated_Serializer_Should_Serialize_And_Deserialize()
    {
        // Arrange
        var assembly = typeof(TestData).Assembly;
        var serializerType = assembly.GetType("Rapp.TestDataRappSerializer");
        var serializer = (IHybridCacheSerializer<TestData>)Activator.CreateInstance(serializerType!)!;
        var original = new TestData { Name = "Test", Value = 42 };
        var buffer = new ArrayBufferWriter<byte>();

        // Act
        serializer.Serialize(original, buffer);
        var sequence = new ReadOnlySequence<byte>(buffer.WrittenMemory);
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().BeEquivalentTo(original);
    }
}

[RappCache]
[MemoryPack.MemoryPackable]
public partial class TestData
{
    public string? Name { get; set; }
    public int Value { get; set; }
}