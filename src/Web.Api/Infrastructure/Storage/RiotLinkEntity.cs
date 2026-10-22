using Azure;
using Azure.Data.Tables;

namespace Web.Api.Infrastructure.Storage;

// Row of the riotlinks table: PartitionKey = Discord id, RowKey = "link"
public class RiotLinkEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Puuid { get; set; } = string.Empty;
    public string RiotId { get; set; } = string.Empty;

    // When the account was linked
    public DateTimeOffset At { get; set; }
}
