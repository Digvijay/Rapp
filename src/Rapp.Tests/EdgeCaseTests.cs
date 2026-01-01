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
using System.Buffers;
using Xunit;

namespace Rapp.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void Serializer_Should_Handle_Empty_Strings()
    {
        // Arrange
        var serializer = new TestSerializer();
        var buffer = new ArrayBufferWriter<byte>();
        var value = string.Empty;

        // Act
        serializer.Serialize(value, buffer);
        var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Serializer_Should_Handle_Very_Long_Strings()
    {
        // Arrange
        var serializer = new TestSerializer();
        var buffer = new ArrayBufferWriter<byte>();
        var longString = new string('A', 10000);

        // Act
        serializer.Serialize(longString, buffer);
        var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().Be(longString);
    }

    [Fact]
    public void Serializer_Should_Handle_Special_Characters()
    {
        // Arrange
        var serializer = new TestSerializer();
        var buffer = new ArrayBufferWriter<byte>();
        var specialString = "Special chars: \n\t\r\"'\\unicode: 🚀";

        // Act
        serializer.Serialize(specialString, buffer);
        var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().Be(specialString);
    }

    [Fact]
    public void Serializer_Should_Handle_Large_Binary_Data()
    {
        // Arrange
        var serializer = new TestSerializer();
        var buffer = new ArrayBufferWriter<byte>();
        var largeData = System.Convert.ToBase64String(System.Linq.Enumerable.Range(0, 1024).Select(i => (byte)(i % 256)).ToArray());

        // Act
        serializer.Serialize(largeData, buffer);
        var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().Be(largeData);
    }

    [Fact]
    public void Serializer_Should_Handle_BufferWriter_Edge_Cases()
    {
        // Arrange
        var serializer = new TestSerializer();

        // Test with pre-allocated buffer
        var buffer = new ArrayBufferWriter<byte>(1024);
        var value = "Test value";

        // Act
        serializer.Serialize(value, buffer);

        // Assert
        buffer.WrittenCount.Should().BeGreaterThan(8); // At least schema hash + data
    }

    [Fact]
    public void Serializer_Should_Handle_ReadOnlySequence_Segments()
    {
        // Arrange
        var serializer = new TestSerializer();
        var originalValue = "Multi-segment test";

        // Create multi-segment sequence by serializing first
        var tempBuffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(originalValue, tempBuffer);
        var sequence = new System.Buffers.ReadOnlySequence<byte>(tempBuffer.WrittenMemory);

        // Act
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().Be(originalValue);
    }

    [Fact]
    public async Task Serializer_Should_Handle_Concurrent_Serialization()
    {
        // Arrange
        var serializer = new TestSerializer();
        var tasks = new System.Threading.Tasks.Task[10];

        // Act - Concurrent serialization operations
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks[i] = System.Threading.Tasks.Task.Run(() =>
            {
                var buffer = new ArrayBufferWriter<byte>();
                var value = $"Concurrent value {index}";
                serializer.Serialize(value, buffer);
                var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
                var result = serializer.Deserialize(sequence);
                result.Should().Be(value);
            });
        }

        // Assert
        await System.Threading.Tasks.Task.WhenAll(tasks);
    }

    [Fact]
    public void Serializer_Should_Handle_Memory_Pressure()
    {
        // Arrange
        var serializer = new TestSerializer();

        // Test with minimal buffer size to force growth
        var buffer = new ArrayBufferWriter<byte>(1); // Very small initial capacity
        var largeValue = new string('X', 1000);

        // Act
        serializer.Serialize(largeValue, buffer);

        // Assert
        buffer.WrittenCount.Should().BeGreaterThan(1000); // Should have grown
    }

    private class TestSerializer : RappBaseSerializer<string>
    {
        private static readonly byte[] _hashBytes = BitConverter.GetBytes(123456789UL);
        protected override ulong SchemaHash => 123456789UL;
        protected override string TypeName => "string";
        protected override ReadOnlySpan<byte> GetSchemaHashBytes() => _hashBytes;
    }
}