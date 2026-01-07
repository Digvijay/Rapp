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
/// Configuration options for Rapp serialization and telemetry.
/// </summary>
/// <remarks>
/// <para>
/// This class provides global configuration settings that affect all Rapp
/// serializers and telemetry collection. Settings should be configured early
/// in application startup before any serialization operations occur.
/// </para>
/// <para>
/// <b>Thread Safety:</b> Configuration changes are not thread-safe and should
/// only be made during application initialization.
/// </para>
/// </remarks>
public static class RappConfiguration
{
    /// <summary>
    /// Gets or sets whether telemetry collection is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if telemetry is enabled; otherwise, <c>false</c>.
    /// Default is <c>true</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When disabled, no metrics will be recorded for cache hits, misses, or
    /// schema validation failures. This can provide a minor performance improvement
    /// in scenarios where telemetry is not needed.
    /// </para>
    /// <para>
    /// <b>Usage:</b> Set this to <c>false</c> in environments where telemetry
    /// overhead is unacceptable or external monitoring is not available.
    /// </para>
    /// </remarks>
    public static bool EnableTelemetry { get; set; } = true;

    /// <summary>
    /// Gets or sets whether detailed error information is included in exceptions.
    /// </summary>
    /// <value>
    /// <c>true</c> if detailed errors are enabled; otherwise, <c>false</c>.
    /// Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When enabled, schema validation failures and deserialization errors will
    /// include detailed information such as type names, schema hashes, and
    /// diagnostic data. This is useful for debugging but may expose internal
    /// implementation details.
    /// </para>
    /// <para>
    /// <b>Security Consideration:</b> Disable in production environments to
    /// avoid leaking internal type information in error messages.
    /// </para>
    /// </remarks>
    public static bool EnableDetailedErrors { get; set; }
    
    /// <summary>
    /// Gets or sets whether schema validation failures should return default
    /// values or throw exceptions.
    /// </summary>
    /// <value>
    /// <c>true</c> to throw exceptions on schema mismatch; <c>false</c> to
    /// return default values. Default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When <c>false</c> (default), schema validation failures return
    /// <c>default(T)</c> allowing graceful degradation. When <c>true</c>,
    /// a <see cref="RappSchemaMismatchException"/> is thrown with details
    /// about the mismatch.
    /// </para>
    /// <para>
    /// <b>Recommendation:</b> Use default behavior (false) in production for
    /// resilience during deployments. Enable exceptions (true) during development
    /// for immediate feedback on schema issues.
    /// </para>
    /// </remarks>
    public static bool ThrowOnSchemaMismatch { get; set; }
}
