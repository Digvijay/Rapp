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
/// Marks a class as cacheable using Rapp's schema-aware binary serialization.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to classes that should be cached using Rapp's high-performance,
/// schema-safe binary serialization. The class must be declared as <c>partial</c> to allow
/// the source generator to create the necessary serialization code.
/// </para>
/// <para>
/// During compilation, the Rapp source generator will analyze classes marked with this
/// attribute and generate optimized binary serialization methods that provide MemoryPack's
/// performance with enterprise-grade schema safety.
/// </para>
/// <para>
/// <b>Usage:</b>
/// <code>
/// [RappCache]
/// public partial class UserProfile
/// {
///     public Guid Id { get; set; }
///     public string Email { get; set; }
///     public DateTime LastLogin { get; set; }
/// }
/// </code>
/// </para>
/// <para>
/// <b>Requirements:</b>
/// <list type="bullet">
/// <item>The class must be declared as <c>partial</c></item>
/// <item>The class must be accessible to the source generator (typically <c>public</c> or <c>internal</c>)</item>
/// <item>The class should be decorated with <c>[MemoryPack.MemoryPackable]</c> for optimal performance</item>
/// </list>
/// </para>
/// <para>
/// <b>Generated Code:</b> The source generator will create:
/// <list type="bullet">
/// <item>A serializer class implementing <c>IHybridCacheSerializer&lt;T&gt;</c></item>
/// <item>An extension method for easy HybridCache integration</item>
/// <item>Schema validation with cryptographic hashing</item>
/// </list>
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RappCacheAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RappCacheAttribute"/> class.
    /// </summary>
    /// <remarks>
    /// This attribute doesn't require any parameters as all configuration
    /// is handled through the class structure and MemoryPack attributes.
    /// </remarks>
    public RappCacheAttribute()
    {
        // This attribute serves as a marker for the source generator.
        // All configuration is derived from the class structure and
        // any applied MemoryPack attributes.
    }
}
