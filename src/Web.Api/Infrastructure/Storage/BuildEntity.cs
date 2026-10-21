using Azure;
using Azure.Data.Tables;

namespace Web.Api.Infrastructure.Storage;

// Row of the builds table: PartitionKey = build id, RowKey = "build"; the build itself is JSON in Data
public class BuildEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string Data { get; set; } = string.Empty;
}
