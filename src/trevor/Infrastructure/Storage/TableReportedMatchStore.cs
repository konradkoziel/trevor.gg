using System.Net;
using Azure;
using Azure.Data.Tables;
using trevor.Features.Lol.Tracking;

namespace trevor.Infrastructure.Storage;

// Ids of matches already reported, one empty row per match; a few rows a day, so no cleanup needed
public class TableReportedMatchStore(StorageTables tables) : IReportedMatchStore
{
    private const string TableName = "reportedmatches";

    // Match ids start with the platform ("EUN1_..."), a natural partition
    private static string Partition(string matchId) => matchId.Split('_')[0];

    public async Task<bool> ContainsAsync(string matchId, CancellationToken ct = default)
    {
        var table = await tables.GetAsync(TableName, ct);
        var row = await table.GetEntityIfExistsAsync<TableEntity>(Partition(matchId), matchId, select: [nameof(ITableEntity.RowKey)], ct);
        return row.HasValue;
    }

    public async Task AddAsync(string matchId, CancellationToken ct = default)
    {
        var table = await tables.GetAsync(TableName, ct);
        try
        {
            await table.AddEntityAsync(new TableEntity(Partition(matchId), matchId), ct);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.Conflict)
        {
            // Already there: same outcome
        }
    }
}
