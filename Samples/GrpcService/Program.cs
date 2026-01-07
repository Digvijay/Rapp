using Microsoft.Extensions.Caching.Hybrid;
using Rapp;
using MemoryPack;
using GrpcService;
using Rapp.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// Configure HybridCache with Rapp
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 10 * 1024 * 1024; // 10MB for larger gRPC payloads
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };
}).UseRappForCachedData();



builder.Services.AddGrpc().AddJsonTranscoding();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGrpcService<DataServiceImpl>();
app.MapRappDashboard();

app.MapGet("/", () => "gRPC service running. Check /index.html for the advanced dashboard.");

app.Run();

// gRPC service implementation
public class DataServiceImpl : DataService.DataServiceBase
{
    private readonly HybridCache _cache;
    private readonly ILogger<DataServiceImpl> _logger;

    public DataServiceImpl(HybridCache cache, ILogger<DataServiceImpl> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public override async Task<DataReply> GetData(DataRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.LogInformation("GetData called with ID: {Id}", request.Id);

        // Use Rapp-backed cache for high-performance caching
        var cachedData = await _cache.GetOrCreateAsync(
            $"data-{request.Id}",
            async ct => await FetchDataFromDatabase(request.Id, ct)
        );

        return new DataReply
        {
            Id = cachedData.Id,
            Name = cachedData.Name,
            Description = cachedData.Description,
            Timestamp = cachedData.Timestamp.Ticks
        };
    }

    public override async Task<ListDataReply> ListData(ListDataRequest request, Grpc.Core.ServerCallContext context)
    {
        _logger.LogInformation("ListData called - Page: {Page}, Size: {Size}", 
            request.PageNumber, request.PageSize);

        var cacheKey = $"datalist-{request.PageNumber}-{request.PageSize}";
        var cachedList = await _cache.GetOrCreateAsync(
            cacheKey,
            async ct => await FetchDataListFromDatabase(request.PageNumber, request.PageSize, ct)
        );

        var reply = new ListDataReply { TotalCount = cachedList.TotalCount };
        reply.Items.AddRange(cachedList.Items.Select(item => new DataReply
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Timestamp = item.Timestamp.Ticks
        }));

        return reply;
    }

    private async Task<CachedData> FetchDataFromDatabase(int id, CancellationToken ct)
    {
        // Simulate database fetch
        await Task.Delay(50, ct);
        return new CachedData
        {
            Id = id,
            Name = $"Data Item {id}",
            Description = $"Description for item {id}",
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<CachedDataList> FetchDataListFromDatabase(int page, int pageSize, CancellationToken ct)
    {
        // Simulate database fetch
        await Task.Delay(100, ct);
        
        var items = Enumerable.Range((page - 1) * pageSize + 1, pageSize)
            .Select(i => new CachedData
            {
                Id = i,
                Name = $"Data Item {i}",
                Description = $"Description for item {i}",
                Timestamp = DateTime.UtcNow
            })
            .ToList();

        return new CachedDataList
        {
            Items = items,
            TotalCount = 1000 // Simulate total count
        };
    }
}

// Cache model - uses Rapp for high-performance serialization
[RappCache]
[MemoryPackable]
public partial class CachedData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

[RappCache]
[MemoryPackable]
public partial class CachedDataList
{
    public List<CachedData> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
