using System.Net;
using Azure;
using Azure.Data.Tables;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Infrastructure.Storage;

// Linked Riot accounts in the API's own storage, one partition per Discord user
public class TableRiotLinkStore(TableServiceClient tables) : IRiotLinkStore
{
    private const string TableName = "riotlinks";
    private const string LinkRow = "link";
    private const string ChallengeRow = "challenge";

    private TableClient? _table;

    public async Task<RiotLink?> GetLinkAsync(string discordId, CancellationToken ct = default)
    {
        var e = await GetAsync(discordId, LinkRow, ct);
        return e is null ? null : new RiotLink(discordId, e.Puuid, e.RiotId, e.At);
    }

    public async Task<IReadOnlyList<RiotLink>> ListLinksAsync(CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var links = new List<RiotLink>();
        await foreach (var e in table.QueryAsync<RiotLinkEntity>(e => e.RowKey == LinkRow, cancellationToken: ct))
            links.Add(new RiotLink(e.PartitionKey, e.Puuid, e.RiotId, e.At));
        return links;
    }

    public async Task<RiotLinkChallenge?> GetChallengeAsync(string discordId, CancellationToken ct = default)
    {
        var e = await GetAsync(discordId, ChallengeRow, ct);
        return e is null ? null : new RiotLinkChallenge(discordId, e.Puuid, e.RiotId, e.IconId, e.At);
    }

    public async Task SaveChallengeAsync(RiotLinkChallenge c, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        await table.UpsertEntityAsync(new RiotLinkEntity
        {
            PartitionKey = c.DiscordId, RowKey = ChallengeRow, Puuid = c.Puuid, RiotId = c.RiotId, IconId = c.IconId, At = c.ExpiresAt,
        }, TableUpdateMode.Replace, ct);
    }

    public async Task CompleteAsync(RiotLink link, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        // Same partition: both changes in one transaction
        await table.SubmitTransactionAsync(
        [
            new TableTransactionAction(TableTransactionActionType.UpsertReplace, new RiotLinkEntity
            {
                PartitionKey = link.DiscordId, RowKey = LinkRow, Puuid = link.Puuid, RiotId = link.RiotId, At = link.VerifiedAt,
            }),
            new TableTransactionAction(TableTransactionActionType.Delete, new TableEntity(link.DiscordId, ChallengeRow), ETag.All),
        ], ct);
    }

    public async Task RemoveAsync(string discordId, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        foreach (var row in new[] { LinkRow, ChallengeRow })
        {
            try
            {
                await table.DeleteEntityAsync(discordId, row, ETag.All, ct);
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                // Nothing to remove
            }
        }
    }

    private async Task<RiotLinkEntity?> GetAsync(string discordId, string row, CancellationToken ct)
    {
        var table = await TableAsync(ct);
        var entity = await table.GetEntityIfExistsAsync<RiotLinkEntity>(discordId, row, cancellationToken: ct);
        return entity.HasValue ? entity.Value : null;
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
