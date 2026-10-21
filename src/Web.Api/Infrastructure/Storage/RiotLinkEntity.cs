using Azure;
using Azure.Data.Tables;

namespace Web.Api.Infrastructure.Storage;

// Row of the riotlinks table: PartitionKey = Discord id, RowKey = "link" (verified) or "challenge" (pending)
public class RiotLinkEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string Puuid { get; set; } = string.Empty;
    public string RiotId { get; set; } = string.Empty;
    public int IconId { get; set; }

    // VerifiedAt for a link, ExpiresAt for a challenge
    public DateTimeOffset At { get; set; }
}
