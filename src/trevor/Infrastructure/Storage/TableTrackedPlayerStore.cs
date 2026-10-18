using System.Net;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using trevor.Features.Lol.Tracking;

namespace trevor.Infrastructure.Storage;

// Tracked players in Azure Table Storage, in the Function App's own storage account
public class TableTrackedPlayerStore(IOptions<StorageOptions> options) : ITrackedPlayerStore
{
    private const string TableName = "trackedplayers";

    // A handful of friends: one partition is plenty and keeps every query a single-partition scan
    private const string Partition = "players";

    private TableClient? _table;

    public async Task<IReadOnlyList<TrackedPlayer>> ListAsync(CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var players = new List<TrackedPlayer>();
        await foreach (var e in table.QueryAsync<TrackedPlayerEntity>(e => e.PartitionKey == Partition, cancellationToken: ct))
            players.Add(new TrackedPlayer(e.RowKey, e.RiotId, e.LastMatchId));
        return players;
    }

    public async Task AddAsync(TrackedPlayer player, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var entity = new TrackedPlayerEntity
        {
            PartitionKey = Partition,
            RowKey = player.Puuid,
            RiotId = player.RiotId,
            LastMatchId = player.LastMatchId
        };
        await table.UpsertEntityAsync(entity, TableUpdateMode.Replace, ct);
    }

    public async Task<bool> RemoveAsync(string puuid, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        try
        {
            using var response = await table.DeleteEntityAsync(Partition, puuid, ETag.All, ct);
            return response.Status != (int)HttpStatusCode.NotFound;
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task SetLastMatchAsync(string puuid, string matchId, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var update = new TableEntity(Partition, puuid) { [nameof(TrackedPlayerEntity.LastMatchId)] = matchId };
        try
        {
            await table.UpdateEntityAsync(update, ETag.All, TableUpdateMode.Merge, ct);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            // Untracked while their match was being reported: nothing to update
        }
    }

    // Created on first use, so a storage problem breaks only tracking, not the whole app
    private async Task<TableClient> TableAsync(CancellationToken ct)
    {
        if (_table is not null)
            return _table;

        var connectionString = options.Value.ConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Player tracking needs AzureWebJobsStorage as a connection string");

        var table = new TableClient(connectionString, TableName);
        await table.CreateIfNotExistsAsync(ct);
        return _table = table;
    }
}
