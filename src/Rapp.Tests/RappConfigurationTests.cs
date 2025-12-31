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

public class RappConfigurationTests
{
    [Fact]
    public void EnableTelemetry_Should_Default_To_True()
    {
        // Act
        var defaultValue = RappConfiguration.EnableTelemetry;

        // Assert
        defaultValue.Should().BeTrue();
    }

    [Fact]
    public void EnableDetailedErrors_Should_Default_To_False()
    {
        // Act
        var defaultValue = RappConfiguration.EnableDetailedErrors;

        // Assert
        defaultValue.Should().BeFalse();
    }

    [Fact]
    public void ThrowOnSchemaMismatch_Should_Default_To_False()
    {
        // Act
        var defaultValue = RappConfiguration.ThrowOnSchemaMismatch;

        // Assert
        defaultValue.Should().BeFalse();
    }

    [Fact]
    public void EnableTelemetry_Should_Be_Settable()
    {
        // Arrange
        var originalValue = RappConfiguration.EnableTelemetry;
        
        try
        {
            // Act
            RappConfiguration.EnableTelemetry = false;

            // Assert
            RappConfiguration.EnableTelemetry.Should().BeFalse();

            // Restore
            RappConfiguration.EnableTelemetry = true;
            RappConfiguration.EnableTelemetry.Should().BeTrue();
        }
        finally
        {
            // Cleanup - restore original
            RappConfiguration.EnableTelemetry = originalValue;
        }
    }

    [Fact]
    public void EnableDetailedErrors_Should_Be_Settable()
    {
        // Arrange
        var originalValue = RappConfiguration.EnableDetailedErrors;
        
        try
        {
            // Act
            RappConfiguration.EnableDetailedErrors = true;

            // Assert
            RappConfiguration.EnableDetailedErrors.Should().BeTrue();

            // Restore
            RappConfiguration.EnableDetailedErrors = false;
            RappConfiguration.EnableDetailedErrors.Should().BeFalse();
        }
        finally
        {
            // Cleanup - restore original
            RappConfiguration.EnableDetailedErrors = originalValue;
        }
    }

    [Fact]
    public void ThrowOnSchemaMismatch_Should_Be_Settable()
    {
        // Arrange
        var originalValue = RappConfiguration.ThrowOnSchemaMismatch;
        
        try
        {
            // Act
            RappConfiguration.ThrowOnSchemaMismatch = true;

            // Assert
            RappConfiguration.ThrowOnSchemaMismatch.Should().BeTrue();

            // Restore
            RappConfiguration.ThrowOnSchemaMismatch = false;
            RappConfiguration.ThrowOnSchemaMismatch.Should().BeFalse();
        }
        finally
        {
            // Cleanup - restore original
            RappConfiguration.ThrowOnSchemaMismatch = originalValue;
        }
    }

    [Fact]
    public void Configuration_Changes_Should_Not_Interfere_With_Each_Other()
    {
        // Arrange
        var originalTelemetry = RappConfiguration.EnableTelemetry;
        var originalDetailedErrors = RappConfiguration.EnableDetailedErrors;
        var originalThrowOnMismatch = RappConfiguration.ThrowOnSchemaMismatch;
        
        try
        {
            // Act - Set all to specific values
            RappConfiguration.EnableTelemetry = false;
            RappConfiguration.EnableDetailedErrors = true;
            RappConfiguration.ThrowOnSchemaMismatch = true;

            // Assert - All should maintain their independent values
            RappConfiguration.EnableTelemetry.Should().BeFalse();
            RappConfiguration.EnableDetailedErrors.Should().BeTrue();
            RappConfiguration.ThrowOnSchemaMismatch.Should().BeTrue();

            // Act - Change one value
            RappConfiguration.EnableTelemetry = true;

            // Assert - Others should remain unchanged
            RappConfiguration.EnableTelemetry.Should().BeTrue();
            RappConfiguration.EnableDetailedErrors.Should().BeTrue();
            RappConfiguration.ThrowOnSchemaMismatch.Should().BeTrue();
        }
        finally
        {
            // Cleanup - restore all originals
            RappConfiguration.EnableTelemetry = originalTelemetry;
            RappConfiguration.EnableDetailedErrors = originalDetailedErrors;
            RappConfiguration.ThrowOnSchemaMismatch = originalThrowOnMismatch;
        }
    }

    [Fact]
    public void Configuration_Should_Support_Multiple_Toggles()
    {
        // Arrange
        var originalTelemetry = RappConfiguration.EnableTelemetry;
        var originalDetailedErrors = RappConfiguration.EnableDetailedErrors;
        
        try
        {
            // Act - Toggle telemetry multiple times
            RappConfiguration.EnableTelemetry = false;
            RappConfiguration.EnableTelemetry = true;
            RappConfiguration.EnableTelemetry = false;
            
            // Act - Toggle detailed errors
            RappConfiguration.EnableDetailedErrors = true;
            RappConfiguration.EnableDetailedErrors = false;
            RappConfiguration.EnableDetailedErrors = true;

            // Assert - Values should reflect last set
            RappConfiguration.EnableTelemetry.Should().BeFalse();
            RappConfiguration.EnableDetailedErrors.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            RappConfiguration.EnableTelemetry = originalTelemetry;
            RappConfiguration.EnableDetailedErrors = originalDetailedErrors;
        }
    }

    [Fact]
    public void Configuration_Properties_Should_Be_Static()
    {
        // Act - Access from different contexts should return same value
        var value1 = RappConfiguration.EnableTelemetry;
        var value2 = RappConfiguration.EnableTelemetry;

        // Assert
        value1.Should().Be(value2);
    }
}
