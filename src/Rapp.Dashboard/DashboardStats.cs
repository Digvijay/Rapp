using System.Text.Json.Serialization;

namespace Rapp.Dashboard;

public record RappDashboardStats(
    long Hits,
    long Misses,
    long RappBytes,
    long JsonBytes,
    long TotalRequests,
    double HitRate,
    string Version
);

[JsonSerializable(typeof(RappDashboardStats))]
internal sealed partial class DashboardJsonContext : JsonSerializerContext
{
}
