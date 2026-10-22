using Azure;
using Azure.Data.Tables;

namespace Web.Api.Infrastructure.Storage;

// Row of the riotlinks table: PartitionKey = Discord id, RowKey = "acc_" + PUUID (one row per linked account).
// Before multi-account linking the only account was the row "link"; it's still read, and moved on the next change
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

    public bool IsMain { get; set; }
}
