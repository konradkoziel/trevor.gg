using System.Net;
using Azure;
using Azure.Data.Tables;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Infrastructure.Storage;

// Linked Riot accounts in the API's own storage: one partition per Discord user, one row per account
public class TableRiotLinkStore(TableServiceClient tables) : IRiotLinkStore
{
    private const string TableName = "riotlinks";
    private const string AccountRowPrefix = "acc_";

    // The single account of the time before multi-account linking: read as the main one, moved to its own row on a change
    private const string LegacyLinkRow = "link";

    // Left behind by the old profile-icon check
    private const string LegacyChallengeRow = "challenge";

    private TableClient? _table;

    public async Task<IReadOnlyList<RiotLink>> ListAccountsAsync(string discordId, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var accounts = new List<RiotLink>();
        await foreach (var e in table.QueryAsync<RiotLinkEntity>(e => e.PartitionKey == discordId, cancellationToken: ct))
            if (ToLink(e) is { } link)
                accounts.Add(link);
        return accounts;
    }

    public async Task<IReadOnlyList<RiotLink>> ListLinksAsync(CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var links = new List<RiotLink>();
        await foreach (var e in table.QueryAsync<RiotLinkEntity>(cancellationToken: ct))
            if (ToLink(e) is { } link)
                links.Add(link);
        return links;
    }

    public async Task SaveLinkAsync(RiotLink link, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        await MoveLegacyRowAsync(table, link.DiscordId, ct);
        await table.UpsertEntityAsync(ToEntity(link), TableUpdateMode.Replace, ct);
    }

    public async Task RemoveAsync(string discordId, string puuid, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        await MoveLegacyRowAsync(table, discordId, ct);
        await DeleteIfExistsAsync(table, discordId, AccountRowPrefix + puuid, ct);
        await DeleteIfExistsAsync(table, discordId, LegacyChallengeRow, ct);
    }

    // Rows that aren't accounts (an old challenge) are skipped
    private static RiotLink? ToLink(RiotLinkEntity e) => e.RowKey switch
    {
        LegacyLinkRow => new RiotLink(e.PartitionKey, e.Puuid, e.RiotId, e.At, IsMain: true),
        _ when e.RowKey.StartsWith(AccountRowPrefix, StringComparison.Ordinal) => new RiotLink(e.PartitionKey, e.Puuid, e.RiotId, e.At, e.IsMain),
        _ => null,
    };

    private static RiotLinkEntity ToEntity(RiotLink link) => new()
    {
        PartitionKey = link.DiscordId,
        RowKey = AccountRowPrefix + link.Puuid,
        Puuid = link.Puuid,
        RiotId = link.RiotId,
        At = link.LinkedAt,
        IsMain = link.IsMain,
    };

    private static async Task MoveLegacyRowAsync(TableClient table, string discordId, CancellationToken ct)
    {
        var legacy = await table.GetEntityIfExistsAsync<RiotLinkEntity>(discordId, LegacyLinkRow, cancellationToken: ct);
        if (!legacy.HasValue || ToLink(legacy.Value!) is not { } link)
            return;
        // Same partition: both in one transaction, so the account is never missing or doubled
        await table.SubmitTransactionAsync(
        [
            new TableTransactionAction(TableTransactionActionType.UpsertReplace, ToEntity(link)),
            new TableTransactionAction(TableTransactionActionType.Delete, new TableEntity(discordId, LegacyLinkRow), ETag.All),
        ], ct);
    }

    private static async Task DeleteIfExistsAsync(TableClient table, string discordId, string row, CancellationToken ct)
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

    private async Task<TableClient> TableAsync(CancellationToken ct)
    {
        if (_table is not null)
            return _table;
        var table = tables.GetTableClient(TableName);
        await table.CreateIfNotExistsAsync(ct);
        return _table = table;
    }
}
