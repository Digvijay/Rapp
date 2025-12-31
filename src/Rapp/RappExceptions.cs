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

namespace Rapp;

/// <summary>
/// Exception thrown when schema validation fails during deserialization.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when <see cref="RappConfiguration.ThrowOnSchemaMismatch"/>
/// is set to <c>true</c> and a schema hash mismatch is detected during deserialization.
/// </para>
/// <para>
/// Schema mismatches occur when the serialized data was created with a different
/// version of the type definition than the deserializer is using.
/// </para>
/// </remarks>
public class RappSchemaMismatchException : Exception
{
    /// <summary>
    /// Gets the name of the type that failed schema validation.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets the expected schema hash.
    /// </summary>
    public ulong ExpectedHash { get; }

    /// <summary>
    /// Gets the actual schema hash found in the data.
    /// </summary>
    public ulong ActualHash { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RappSchemaMismatchException"/> class.
    /// </summary>
    /// <param name="typeName">The name of the type.</param>
    /// <param name="expectedHash">The expected schema hash.</param>
    /// <param name="actualHash">The actual schema hash.</param>
    public RappSchemaMismatchException(string typeName, ulong expectedHash, ulong actualHash)
        : base($"Schema mismatch for type '{typeName}': expected hash 0x{expectedHash:X16}, found 0x{actualHash:X16}")
    {
        TypeName = typeName;
        ExpectedHash = expectedHash;
        ActualHash = actualHash;
    }
}
