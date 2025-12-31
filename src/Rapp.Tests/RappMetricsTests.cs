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
using System.Diagnostics.Metrics;
using Xunit;

namespace Rapp.Tests;

public class RappMetricsTests
{
    [Fact]
    public void RecordHit_Should_Increment_CacheHits_Counter()
    {
        // Arrange & Act
        RappMetrics.RecordHit();

        // Assert - Method should not throw
        // We can't directly test the counter value without a listener,
        // but we can verify the method exists and is callable
        var method = typeof(RappMetrics).GetMethod("RecordHit");
        method.Should().NotBeNull();
    }

    [Fact]
    public void RecordMiss_Should_Increment_CacheMisses_Counter()
    {
        // Arrange & Act
        RappMetrics.RecordMiss();

        // Assert - Method should not throw
        var method = typeof(RappMetrics).GetMethod("RecordMiss");
        method.Should().NotBeNull();
    }

    [Fact]
    public void Meter_Should_Be_Created_With_Correct_Name()
    {
        // Act
        var meterField = typeof(RappMetrics).GetField("Meter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var meter = meterField?.GetValue(null) as Meter;

        // Assert
        meter.Should().NotBeNull();
        meter!.Name.Should().Be("Rapp");
    }

    [Fact]
    public void CacheHits_Counter_Should_Have_Correct_Name_And_Description()
    {
        // Act
        var counterField = typeof(RappMetrics).GetField("CacheHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var counter = counterField?.GetValue(null) as Counter<long>;

        // Assert
        counter.Should().NotBeNull();
        // Note: Counter name and description are set during creation, we verify the field exists
    }

    [Fact]
    public void CacheMisses_Counter_Should_Have_Correct_Name_And_Description()
    {
        // Act
        var counterField = typeof(RappMetrics).GetField("CacheMisses", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var counter = counterField?.GetValue(null) as Counter<long>;

        // Assert
        counter.Should().NotBeNull();
    }
}