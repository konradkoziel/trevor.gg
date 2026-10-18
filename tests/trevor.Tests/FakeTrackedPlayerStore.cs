using trevor.Features.Lol.Tracking;

namespace trevor.Tests;

public sealed class FakeTrackedPlayerStore(params TrackedPlayer[] players) : ITrackedPlayerStore
{
    public List<TrackedPlayer> Players { get; } = [.. players];

    public Task<IReadOnlyList<TrackedPlayer>> ListAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<TrackedPlayer>>([.. Players]);

    public Task AddAsync(TrackedPlayer player, CancellationToken ct = default)
    {
        Players.RemoveAll(p => p.Puuid == player.Puuid);
        Players.Add(player);
        return Task.CompletedTask;
    }

    public Task<bool> RemoveAsync(string puuid, CancellationToken ct = default) =>
        Task.FromResult(Players.RemoveAll(p => p.Puuid == puuid) > 0);

    public Task SetLastMatchAsync(string puuid, string matchId, CancellationToken ct = default)
    {
        var i = Players.FindIndex(p => p.Puuid == puuid);
        if (i >= 0)
            Players[i] = Players[i] with { LastMatchId = matchId };
        return Task.CompletedTask;
    }

    public string? LastMatchOf(string puuid) => Players.Single(p => p.Puuid == puuid).LastMatchId;
}
