using Azure;
using Azure.Data.Tables;
using Web.Api.Features.Stats;

namespace Web.Api.Infrastructure.Storage;

// A finished match never changes, so a row is written once and read until MatchHistory drops it as too old
public class TableMatchHistoryStore(TableServiceClient tables) : IMatchHistoryStore
{
    private const string TableName = "playedmatches";

    // Table Storage transactions take up to 100 actions
    private const int BatchSize = 100;

    private TableClient? _table;

    public async Task<IReadOnlyList<PlayedMatch>> ListAsync(string puuid, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var matches = new List<PlayedMatch>();
        await foreach (var e in table.QueryAsync<PlayedMatchEntity>(e => e.PartitionKey == puuid, cancellationToken: ct))
            matches.Add(new PlayedMatch(e.RowKey, e.ChampionName, e.Win, e.Kills, e.Deaths, e.Assists, e.CreepScore,
                e.DamageToChampions, e.DurationSeconds, e.QueueId, e.PlayedAt));
        return matches;
    }

    public async Task SaveAsync(string puuid, IReadOnlyList<PlayedMatch> matches, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        foreach (var batch in matches.Chunk(BatchSize))
            await table.SubmitTransactionAsync(batch.Select(m => new TableTransactionAction(TableTransactionActionType.UpsertReplace,
                new PlayedMatchEntity
                {
                    PartitionKey = puuid, RowKey = m.MatchId, ChampionName = m.ChampionName, Win = m.Win, Kills = m.Kills,
                    Deaths = m.Deaths, Assists = m.Assists, CreepScore = m.CreepScore, DamageToChampions = m.DamageToChampions,
                    DurationSeconds = m.DurationSeconds, QueueId = m.QueueId, PlayedAt = m.PlayedAt,
                })), ct);
    }

    public async Task DeleteAsync(string puuid, IReadOnlyList<string> matchIds, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        foreach (var batch in matchIds.Chunk(BatchSize))
            await table.SubmitTransactionAsync(batch.Select(id =>
                new TableTransactionAction(TableTransactionActionType.Delete, new TableEntity(puuid, id), ETag.All)), ct);
    }

    private async Task<TableClient> TableAsync(CancellationToken ct)
    {
        if (_table is not null)
            return _table;
        var table = tables.GetTableClient(TableName);
        await table.CreateIfNotExistsAsync(ct);
        return _table = table;
    }
}
