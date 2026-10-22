using Azure;
using Azure.Data.Tables;

namespace trevor.Infrastructure.Storage;

// Row of the trackedplayers table; RowKey is the player's PUUID
public class TrackedPlayerEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string RiotId { get; set; } = string.Empty;
    public string? LastMatchId { get; set; }
    public bool FromWebsite { get; set; }
}
