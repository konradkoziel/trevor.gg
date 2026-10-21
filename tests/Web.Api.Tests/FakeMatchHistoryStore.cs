using Web.Api.Features.Stats;

namespace Web.Api.Tests;

public sealed class FakeMatchHistoryStore : IMatchHistoryStore
{
    public Dictionary<string, List<PlayedMatch>> ByPuuid { get; } = new();

    public Task<IReadOnlyList<PlayedMatch>> ListAsync(string puuid, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<PlayedMatch>>(ByPuuid.GetValueOrDefault(puuid)?.ToList() ?? []);

    public Task SaveAsync(string puuid, IReadOnlyList<PlayedMatch> matches, CancellationToken ct = default)
    {
        if (!ByPuuid.TryGetValue(puuid, out var list))
            ByPuuid[puuid] = list = [];
        list.AddRange(matches);
        return Task.CompletedTask;
    }
}
