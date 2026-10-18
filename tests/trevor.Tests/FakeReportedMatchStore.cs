using trevor.Features.Lol.Tracking;

namespace trevor.Tests;

public sealed class FakeReportedMatchStore : IReportedMatchStore
{
    public HashSet<string> MatchIds { get; } = [];

    public Task<bool> ContainsAsync(string matchId, CancellationToken ct = default) => Task.FromResult(MatchIds.Contains(matchId));

    public Task AddAsync(string matchId, CancellationToken ct = default)
    {
        MatchIds.Add(matchId);
        return Task.CompletedTask;
    }
}
