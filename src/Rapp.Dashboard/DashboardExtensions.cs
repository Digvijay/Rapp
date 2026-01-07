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
    private static long _rappBytes;
    private static long _jsonBytes;

    static RappDashboardExtensions()
    {
        // Listener initialization
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
            if (instrument.Name == "rapp_cache_hits_total") Interlocked.Add(ref _hits, measurement);
            else if (instrument.Name == "rapp_cache_misses_total") Interlocked.Add(ref _misses, measurement);
            else if (instrument.Name == "rapp_bytes_total") Interlocked.Add(ref _rappBytes, measurement);
            else if (instrument.Name == "json_bytes_equivalent") Interlocked.Add(ref _jsonBytes, measurement);
        });
        listener.Start();
    }

    public static IEndpointConventionBuilder MapRappDashboard(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/rapp", (HttpContext context) =>
        {
            var hits = Interlocked.Read(ref _hits);
            var misses = Interlocked.Read(ref _misses);
            var rappBytes = Interlocked.Read(ref _rappBytes);
            var jsonBytes = Interlocked.Read(ref _jsonBytes);
            
            var totalRequests = hits + misses;
            var hitRate = totalRequests > 0 ? (double)hits / totalRequests * 100 : 0;
            var version = "1.0.0";

            // Return JSON if requested
            if (context.Request.Query["format"] == "json" || 
                context.Request.Headers.Accept.ToString().Contains("application/json"))
            {
                var stats = new RappDashboardStats(
                    Hits: hits,
                    Misses: misses,
                    RappBytes: rappBytes,
                    JsonBytes: jsonBytes,
                    TotalRequests: totalRequests,
                    HitRate: hitRate,
                    Version: version
                );

                return Results.Json(stats, DashboardJsonContext.Default.RappDashboardStats);
            }
            
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Rapp Dashboard</title>
    <meta http-equiv='refresh' content='1'>
    <style>
        body {{ 
            font-family: 'Segoe UI', system-ui, sans-serif; 
            margin: 0; 
            padding: 20px;
            background: #1e293b; /* Matches demo dark theme */
            color: #f1f5f9;
        }}
        .stat {{ 
            background: #334155; 
            padding: 15px; 
            margin-bottom: 10px; 
            border-radius: 8px; 
            border: 1px solid #475569;
        }}
        .stat strong {{ color: #94a3b8; display: block; font-size: 0.8em; text-transform: uppercase; letter-spacing: 0.05em; }}
        .val {{ font-size: 1.5em; font-weight: bold; margin-top: 5px; }}
        .hit-rate {{ color: { (hitRate >= 80 ? "#10b981" : hitRate >= 50 ? "#f59e0b" : "#ef4444") }; }}
        h1 {{ font-size: 1.2rem; margin-top: 0; color: #6366f1; }}
        h2 {{ font-size: 1rem; margin-top: 20px; border-bottom: 1px solid #334155; padding-bottom: 5px; }}
        ul {{ padding-left: 20px; color: #cbd5e1; }}
        li {{ margin-bottom: 5px; }}
    </style>
</head>
<body>
    <h1>Rapp Internal Stats</h1>
    
    <div class='stat'>
        <strong>Cache Hits</strong>
        <div class='val'>{hits}</div>
    </div>
    <div class='stat'>
        <strong>Cache Misses</strong>
        <div class='val'>{misses}</div>
    </div>
    <div class='stat'>
        <strong>Hit Rate</strong>
        <div class='val hit-rate'>{hitRate:F1}%</div>
    </div>
    
    <p style='color: #64748b; font-size: 0.8rem; text-align: right;'>Updates every 1s</p>
</body>
</html>";
            
            return Results.Content(html, "text/html");
        });
    }
}
