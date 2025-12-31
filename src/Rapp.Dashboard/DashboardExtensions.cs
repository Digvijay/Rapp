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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.Metrics;

namespace Rapp.Dashboard;
public static class RappDashboardExtensions
{
    private static readonly Meter Meter = new("Rapp");
    private static long _hits;
    private static long _misses;

    static RappDashboardExtensions()
    {
        var hitsCounter = Meter.CreateCounter<long>("rapp_cache_hits_total");
        var missesCounter = Meter.CreateCounter<long>("rapp_cache_misses_total");

        // Create a listener to collect metrics
        var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == "Rapp")
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
        {
            if (instrument.Name == "rapp_cache_hits_total")
            {
                Interlocked.Add(ref _hits, measurement);
            }
            else if (instrument.Name == "rapp_cache_misses_total")
            {
                Interlocked.Add(ref _misses, measurement);
            }
        });
        listener.Start();
    }

    public static IEndpointConventionBuilder MapRappDashboard(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/rapp", (HttpContext context) =>
        {
            var hits = Interlocked.Read(ref _hits);
            var misses = Interlocked.Read(ref _misses);
            var totalRequests = hits + misses;
            var hitRate = totalRequests > 0 ? (double)hits / totalRequests * 100 : 0;
            
            var version = "1.0.0";
            
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Rapp Dashboard</title>
    <meta http-equiv='refresh' content='0.5'>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .stat {{ background: #f0f0f0; padding: 10px; margin: 10px 0; border-radius: 5px; }}
        .stat strong {{ color: #333; }}
        .hit-rate {{ color: { (hitRate >= 80 ? "green" : hitRate >= 50 ? "orange" : "red") }; }}
    </style>
</head>
<body>
    <h1>Rapp Dashboard</h1>
    <div class='stat'>
        <strong>Status:</strong> Active<br>
        <strong>Version:</strong> {version}<br>
        <strong>Description:</strong> High-performance caching serializer for .NET
    </div>
    
    <h2>Cache Statistics</h2>
    <div class='stat'>
        <strong>Total Requests:</strong> {totalRequests}<br>
        <strong>Cache Hits:</strong> {hits}<br>
        <strong>Cache Misses:</strong> {misses}<br>
        <strong>Hit Rate:</strong> <span class='hit-rate'>{hitRate:F2}%</span>
    </div>
    
    <h2>Features</h2>
    <ul>
        <li>MemoryPack integration</li>
        <li>Source-generated serializers</li>
        <li>HybridCache compatibility</li>
        <li>Zero-overhead telemetry</li>
    </ul>
    
    <p>Last updated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
</body>
</html>";
            
            return Results.Content(html, "text/html");
        });
    }
}
