using Azure;
using Azure.Data.Tables;

namespace Web.Api.Infrastructure.Storage;

// Row of the playedmatches table: PartitionKey = PUUID, RowKey = match id
public class PlayedMatchEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string ChampionName { get; set; } = string.Empty;
    public bool Win { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int CreepScore { get; set; }
    public int DamageToChampions { get; set; }
    public long DurationSeconds { get; set; }
    public int QueueId { get; set; }
    public DateTimeOffset PlayedAt { get; set; }
}
