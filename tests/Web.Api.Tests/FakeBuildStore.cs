using Web.Api.Features.Builds;

namespace Web.Api.Tests;

public sealed class FakeBuildStore : IBuildStore
{
    public Dictionary<string, StoredBuild> Builds { get; } = new();

    public Task<StoredBuild?> GetAsync(string id, CancellationToken ct = default) =>
        Task.FromResult(Builds.GetValueOrDefault(id));

    public Task<IReadOnlyList<StoredBuild>> ListByOwnerAsync(string ownerId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<StoredBuild>>(Builds.Values.Where(b => b.OwnerId == ownerId).OrderByDescending(b => b.UpdatedAt).ToList());

    public Task<IReadOnlyList<StoredBuild>> ListRecentAsync(int count, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<StoredBuild>>(Builds.Values.OrderByDescending(b => b.UpdatedAt).Take(count).ToList());

    public Task<IReadOnlyList<StoredBuild>> ListByChampionAsync(string championId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<StoredBuild>>(
            Builds.Values.Where(b => b.Build.ChampionId == championId).OrderByDescending(b => b.UpdatedAt).ToList());

    public Task SaveAsync(StoredBuild build, CancellationToken ct = default)
    {
        Builds[build.Id] = build;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken ct = default)
    {
        Builds.Remove(id);
        return Task.CompletedTask;
    }
}
