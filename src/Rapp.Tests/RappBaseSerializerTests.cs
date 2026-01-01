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
using MemoryPack;
using System.Buffers;
using Xunit;

namespace Rapp.Tests;

public class RappBaseSerializerTests
{
    [Fact]
    public void Serialize_Should_Write_SchemaHash_First()
    {
        // Arrange
        var serializer = new TestSerializer();
        var buffer = new ArrayBufferWriter<byte>();
        var value = "test";

        // Act
        serializer.Serialize(value, buffer);

        // Assert
        var span = buffer.WrittenSpan;
        var hash = BitConverter.ToUInt64(span.Slice(0, 8));
        hash.Should().Be(123456789UL);
    }

    [Fact]
    public void Deserialize_Should_Read_SchemaHash_And_Deserialize_Value()
    {
        // Arrange
        var serializer = new TestSerializer();
        var originalValue = "test";
        var buffer = new ArrayBufferWriter<byte>();
        serializer.Serialize(originalValue, buffer);

        var sequence = new ReadOnlySequence<byte>(buffer.WrittenMemory);

        // Act
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().Be(originalValue);
    }

    [Fact]
    public void Deserialize_Should_Return_Default_If_SchemaHash_Mismatch()
    {
        // Arrange
        var serializer = new TestSerializer();
        var buffer = new ArrayBufferWriter<byte>();
        // Write wrong hash
        var wrongHash = BitConverter.GetBytes(999999UL);
        buffer.Write(wrongHash);
        MemoryPackSerializer.Serialize(buffer, "test");

        var sequence = new ReadOnlySequence<byte>(buffer.WrittenMemory);

        // Act
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Deserialize_Should_Return_Default_If_Buffer_Too_Small()
    {
        // Arrange
        var serializer = new TestSerializer();
        var sequence = new ReadOnlySequence<byte>(new byte[4]);

        // Act
        var result = serializer.Deserialize(sequence);

        // Assert
        result.Should().BeNull();
    }

    private class TestSerializer : RappBaseSerializer<string>
    {
        private static readonly byte[] _hashBytes = BitConverter.GetBytes(123456789UL);
        protected override ulong SchemaHash => 123456789UL;
        protected override string TypeName => "string";
        protected override ReadOnlySpan<byte> GetSchemaHashBytes() => _hashBytes;
    }
}