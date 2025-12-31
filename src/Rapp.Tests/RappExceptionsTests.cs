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
using Xunit;

namespace Rapp.Tests;

public class RappExceptionsTests
{
    [Fact]
    public void RappSchemaMismatchException_Should_Set_TypeName_Property()
    {
        // Arrange
        var typeName = "TestType";
        var expectedHash = 123456789UL;
        var actualHash = 987654321UL;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.TypeName.Should().Be(typeName);
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Set_ExpectedHash_Property()
    {
        // Arrange
        var typeName = "TestType";
        var expectedHash = 123456789UL;
        var actualHash = 987654321UL;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.ExpectedHash.Should().Be(expectedHash);
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Set_ActualHash_Property()
    {
        // Arrange
        var typeName = "TestType";
        var expectedHash = 123456789UL;
        var actualHash = 987654321UL;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.ActualHash.Should().Be(actualHash);
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Generate_Informative_Message()
    {
        // Arrange
        var typeName = "UserData";
        var expectedHash = 123456789UL;
        var actualHash = 987654321UL;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.Message.Should().Contain(typeName);
        exception.Message.Should().ContainAny("075BCD15", "X16"); // Hash in hex format
        exception.Message.Should().ContainAny("3ADE68B1", "X16"); // Hash in hex format
    }

    [Fact]
    public void RappSchemaMismatchException_Message_Should_Explain_The_Problem()
    {
        // Arrange
        var typeName = "TestClass";
        var expectedHash = 111UL;
        var actualHash = 222UL;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.Message.Should().ContainAny("Schema", "schema"); // Case insensitive check
        exception.Message.ToLower().Should().ContainAny("mismatch", "validation", "failed", "incompatible");
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Be_Throwable()
    {
        // Arrange
        var typeName = "TestType";
        var expectedHash = 123UL;
        var actualHash = 456UL;

        // Act
        Action act = () => throw new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        act.Should().Throw<RappSchemaMismatchException>()
            .Which.TypeName.Should().Be(typeName);
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Be_Catchable_As_Exception()
    {
        // Arrange
        var typeName = "TestType";
        var expectedHash = 100UL;
        var actualHash = 200UL;

        // Act
        Action act = () => throw new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        act.Should().Throw<Exception>()
            .Which.Should().BeOfType<RappSchemaMismatchException>();
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Have_All_Properties_Accessible()
    {
        // Arrange
        var typeName = "ComplexType";
        var expectedHash = 555555555UL;
        var actualHash = 666666666UL;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.TypeName.Should().Be(typeName);
        exception.ExpectedHash.Should().Be(expectedHash);
        exception.ActualHash.Should().Be(actualHash);
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Handle_Zero_Hash_Values()
    {
        // Arrange
        var typeName = "EmptyType";
        var expectedHash = 0UL;
        var actualHash = 0UL;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.ExpectedHash.Should().Be(0UL);
        exception.ActualHash.Should().Be(0UL);
        exception.Message.Should().Contain("0");
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Handle_Max_Hash_Values()
    {
        // Arrange
        var typeName = "MaxHashType";
        var expectedHash = ulong.MaxValue;
        var actualHash = ulong.MaxValue - 1;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.ExpectedHash.Should().Be(ulong.MaxValue);
        exception.ActualHash.Should().Be(ulong.MaxValue - 1);
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Handle_Empty_TypeName()
    {
        // Arrange
        var typeName = string.Empty;
        var expectedHash = 123UL;
        var actualHash = 456UL;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.TypeName.Should().BeEmpty();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RappSchemaMismatchException_Should_Handle_Long_TypeName()
    {
        // Arrange
        var typeName = "Very.Long.Namespace.With.Multiple.Segments.AndAVeryLongClassName";
        var expectedHash = 111UL;
        var actualHash = 222UL;

        // Act
        var exception = new RappSchemaMismatchException(typeName, expectedHash, actualHash);

        // Assert
        exception.TypeName.Should().Be(typeName);
        exception.Message.Should().Contain(typeName);
    }
}
