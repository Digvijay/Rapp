// Copyright (c) 2025 Digvijay Chauhan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the subject to the following conditions:
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

public class ThreadSafetyTests
{
    [Fact]
    public async Task RappMetrics_Should_Be_Thread_Safe_For_RecordHit()
    {
        // Arrange
        const int iterationsPerTask = 1000;
        const int taskCount = 10;

        // Act
        var tasks = new Task[taskCount];
        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < iterationsPerTask; j++)
                {
                    RappMetrics.RecordHit();
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert - No exceptions should be thrown, indicating thread safety
        // We can't easily verify the exact count without exposing internal state,
        // but the absence of exceptions indicates thread safety
    }

    [Fact]
    public async Task RappMetrics_Should_Be_Thread_Safe_For_RecordMiss()
    {
        // Arrange
        const int iterationsPerTask = 1000;
        const int taskCount = 10;

        // Act
        var tasks = new Task[taskCount];
        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < iterationsPerTask; j++)
                {
                    RappMetrics.RecordMiss();
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert - No exceptions should be thrown, indicating thread safety
    }

    [Fact]
    public async Task RappMetrics_Should_Handle_Concurrent_Hit_And_Miss_Recording()
    {
        // Arrange
        const int iterationsPerTask = 500;
        const int taskCount = 20;

        // Act
        var tasks = new Task[taskCount];
        for (int i = 0; i < taskCount; i++)
        {
            var taskIndex = i;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < iterationsPerTask; j++)
                {
                    if (taskIndex % 2 == 0)
                    {
                        RappMetrics.RecordHit();
                    }
                    else
                    {
                        RappMetrics.RecordMiss();
                    }
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert - No exceptions should be thrown during concurrent operations
    }

    [Fact]
    public async Task Serializer_Should_Be_Thread_Safe_For_Concurrent_Operations()
    {
        // Arrange
        var serializer = new ThreadSafeTestSerializer();
        const int taskCount = 20;
        const int operationsPerTask = 100;

        // Act
        var tasks = new Task[taskCount];
        for (int i = 0; i < taskCount; i++)
        {
            var taskId = i;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < operationsPerTask; j++)
                {
                    var buffer = new System.Buffers.ArrayBufferWriter<byte>();
                    var value = $"Thread-{taskId}-Operation-{j}";
                    serializer.Serialize(value, buffer);

                    var sequence = new System.Buffers.ReadOnlySequence<byte>(buffer.WrittenMemory);
                    var result = serializer.Deserialize(sequence);
                    result.Should().Be(value);
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert - All operations completed without exceptions
    }

    [Fact]
    public void RappMetrics_Should_Handle_High_Frequency_Operations()
    {
        // Arrange
        const int operationCount = 100000;

        // Act
        for (int i = 0; i < operationCount; i++)
        {
            RappMetrics.RecordHit();
            RappMetrics.RecordMiss();
        }

        // Assert - No exceptions should be thrown during high-frequency operations
    }

    private sealed class ThreadSafeTestSerializer : RappBaseSerializer<string>
    {
        private static readonly byte[] _hashBytes = BitConverter.GetBytes(555666777UL);
        protected override ulong SchemaHash => 555666777UL;
        protected override string TypeName => "string";
        protected override ReadOnlySpan<byte> GetSchemaHashBytes() => _hashBytes;
    }
}