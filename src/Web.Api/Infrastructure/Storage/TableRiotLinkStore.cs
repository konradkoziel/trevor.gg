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

    // Left behind by the old profile-icon check; removed with the link
    private const string LegacyChallengeRow = "challenge";

    private TableClient? _table;

    public async Task<RiotLink?> GetLinkAsync(string discordId, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var e = await table.GetEntityIfExistsAsync<RiotLinkEntity>(discordId, LinkRow, cancellationToken: ct);
        return e.HasValue ? new RiotLink(discordId, e.Value!.Puuid, e.Value.RiotId, e.Value.At) : null;
    }

    public async Task<IReadOnlyList<RiotLink>> ListLinksAsync(CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var links = new List<RiotLink>();
        await foreach (var e in table.QueryAsync<RiotLinkEntity>(e => e.RowKey == LinkRow, cancellationToken: ct))
            links.Add(new RiotLink(e.PartitionKey, e.Puuid, e.RiotId, e.At));
        return links;
    }

    public async Task SaveLinkAsync(RiotLink link, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        await table.UpsertEntityAsync(new RiotLinkEntity
        {
            PartitionKey = link.DiscordId, RowKey = LinkRow, Puuid = link.Puuid, RiotId = link.RiotId, At = link.LinkedAt,
        }, TableUpdateMode.Replace, ct);
    }

    public async Task RemoveAsync(string discordId, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        foreach (var row in new[] { LinkRow, LegacyChallengeRow })
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

    private async Task<TableClient> TableAsync(CancellationToken ct)
    {
        if (_table is not null)
            return _table;
        var table = tables.GetTableClient(TableName);
        await table.CreateIfNotExistsAsync(ct);
        return _table = table;
    }
}
