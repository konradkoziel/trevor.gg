using System.Net;
using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Web.Api.Features.Builds;

namespace Web.Api.Infrastructure.Storage;

// Builds in the API's own storage account. Keyed by build id, so a share link is a single point read;
// "my builds" filters by OwnerId, a scan that stays cheap for a group of friends (MaxBuildsPerUser each)
public class TableBuildStore(TableServiceClient tables) : IBuildStore
{
    private const string TableName = "builds";
    private const string Row = "build";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private TableClient? _table;

    public async Task<StoredBuild?> GetAsync(string id, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var entity = await table.GetEntityIfExistsAsync<BuildEntity>(id, Row, cancellationToken: ct);
        return entity.HasValue ? ToBuild(entity.Value!) : null;
    }

    public async Task<IReadOnlyList<StoredBuild>> ListByOwnerAsync(string ownerId, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var builds = new List<StoredBuild>();
        await foreach (var e in table.QueryAsync<BuildEntity>(e => e.OwnerId == ownerId, cancellationToken: ct))
            builds.Add(ToBuild(e));
        return builds.OrderByDescending(b => b.UpdatedAt).ToList();
    }

    // A full scan: fine for one server's builds (MaxBuildsPerUser each)
    public async Task<IReadOnlyList<StoredBuild>> ListRecentAsync(int count, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        var builds = new List<StoredBuild>();
        await foreach (var e in table.QueryAsync<BuildEntity>(cancellationToken: ct))
            builds.Add(ToBuild(e));
        return builds.OrderByDescending(b => b.UpdatedAt).Take(count).ToList();
    }

    public async Task SaveAsync(StoredBuild build, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        await table.UpsertEntityAsync(new BuildEntity
        {
            PartitionKey = build.Id,
            RowKey = Row,
            OwnerId = build.OwnerId,
            OwnerName = build.OwnerName,
            UpdatedAt = build.UpdatedAt,
            Data = JsonSerializer.Serialize(build.Build, Json),
        }, TableUpdateMode.Replace, ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var table = await TableAsync(ct);
        try
        {
            await table.DeleteEntityAsync(id, Row, ETag.All, ct);
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            // Already gone
        }
    }

    private static StoredBuild ToBuild(BuildEntity e) =>
        new(e.PartitionKey, e.OwnerId, e.OwnerName, e.UpdatedAt, JsonSerializer.Deserialize<SavedBuild>(e.Data, Json)!);

    // Created on first use, so a storage problem breaks only builds, not the champion list
    private async Task<TableClient> TableAsync(CancellationToken ct)
    {
        if (_table is not null)
            return _table;
        var table = tables.GetTableClient(TableName);
        await table.CreateIfNotExistsAsync(ct);
        return _table = table;
    }
}
