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
using MemoryPack;
using System.Text.Json.Serialization;

namespace Rapp.Playground;

// WeatherForecast v1.0 - Original schema
[MemoryPackable]
public partial class WeatherForecastV1
{
    public string? Summary { get; set; }
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
}

// WeatherForecast v2.0 - Breaking changes
[MemoryPackable]
public partial class WeatherForecastV2Breaking
{
    // Removed Date property
    // Reordered TemperatureC before Summary
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
    // Added new property
    public string? Location { get; set; }
}

// WeatherForecast v3.0 - Another breaking change
[MemoryPackable]
public partial class WeatherForecastV3Breaking
{
    // Changed TemperatureC to double
    public double TemperatureC { get; set; }
    public string? Summary { get; set; }
    public string? Location { get; set; }
    // Added another property
    public string[]? Alerts { get; set; }
}

/// <summary>
/// AOT-compatible JSON serializer context for the demo.
/// This demonstrates how System.Text.Json can be made AOT-compatible
/// using source generation instead of reflection.
/// </summary>
[JsonSerializable(typeof(WeatherForecast))]
[JsonSerializable(typeof(WeatherForecastV1))]
[JsonSerializable(typeof(WeatherForecastV2Breaking))]
[JsonSerializable(typeof(WeatherForecastV3Breaking))]
[JsonSerializable(typeof(JsonComparisonResponse))]
[JsonSerializable(typeof(MemoryPackCrashResponse))]
[JsonSerializable(typeof(RappSafetyResponse))]
public partial class DemoJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Response class for JSON comparison demo to ensure AOT compatibility.
/// </summary>
public class JsonComparisonResponse
{
    public string? Demo { get; set; }
    public string? Description { get; set; }
    public string? KeyInsight { get; set; }
    public List<string>? Results { get; set; }
    public string? Conclusion { get; set; }
}

/// <summary>
/// Response class for MemoryPack crash demo.
/// </summary>
public class MemoryPackCrashResponse
{
    public string? Demo { get; set; }
    public string? Description { get; set; }
    public string? KeyInsight { get; set; }
    public List<string>? Results { get; set; }
    public string? Conclusion { get; set; }
}

/// <summary>
/// Response class for Rapp safety demo.
/// </summary>
public class RappSafetyResponse
{
    public string? Demo { get; set; }
    public string? Description { get; set; }
    public string? KeyInsight { get; set; }
    public List<string>? Results { get; set; }
    public string? Conclusion { get; set; }
}