using Lol.Shared.Riot.Models;
using Lol.Shared.Riot;

namespace trevor.Tests;

public sealed class FakeRiot : IRiotApiClient
{
    public bool IsConfigured { get; init; } = true;
    public RiotAccount? Account { get; init; } = new("puuid-1", "Player", "EUNE");
    public IReadOnlyList<LeagueEntry> Entries { get; init; } = [];
    public IReadOnlyList<string> MatchIds { get; init; } = ["EUN1_1"];
    public RiotMatch? Match { get; init; }
    // Per-player / per-match answers; fall back to MatchIds / Match when not set
    public IReadOnlyDictionary<string, IReadOnlyList<string>>? MatchIdsByPuuid { get; init; }
    public IReadOnlyDictionary<string, RiotMatch>? MatchesById { get; init; }
    public ISet<string> FailingPuuids { get; init; } = new HashSet<string>();
    public Exception? Error { get; init; }
    public RiotId? RequestedId { get; private set; }

    public Task<RiotAccount?> GetAccountAsync(RiotId riotId, CancellationToken ct = default)
    {
        RequestedId = riotId;
        return Error is null ? Task.FromResult(Account) : Task.FromException<RiotAccount?>(Error);
    }

    public Task<RiotSummoner?> GetSummonerAsync(string puuid, CancellationToken ct = default) =>
        Task.FromResult<RiotSummoner?>(new RiotSummoner(1, 30));

    public Task<IReadOnlyList<LeagueEntry>> GetLeagueEntriesAsync(string puuid, CancellationToken ct = default) =>
        Task.FromResult(Entries);

    public Task<IReadOnlyList<string>> GetRecentMatchIdsAsync(string puuid, int count, CancellationToken ct = default) =>
        FailingPuuids.Contains(puuid)
            ? Task.FromException<IReadOnlyList<string>>(new RiotApiException(System.Net.HttpStatusCode.InternalServerError))
            : Task.FromResult(MatchIdsByPuuid?.GetValueOrDefault(puuid) ?? MatchIds);

    public Task<RiotMatch?> GetMatchAsync(string matchId, CancellationToken ct = default) =>
        Task.FromResult(MatchesById is null ? Match : MatchesById.GetValueOrDefault(matchId));
}
