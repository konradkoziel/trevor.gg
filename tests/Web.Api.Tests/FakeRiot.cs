using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Web.Api.Tests;

public sealed class FakeRiot : IRiotApiClient
{
    public bool IsConfigured { get; init; } = true;
    public RiotAccount? Account { get; init; } = new("puuid-1", "Player", "EUNE");
    public RiotSummoner? Summoner { get; set; } = new(5, 100);
    public Exception? Error { get; init; }
    public IReadOnlyList<string> MatchIds { get; set; } = [];
    public Dictionary<string, RiotMatch> Matches { get; } = new();
    public List<string> FetchedMatches { get; } = [];
    public int MatchIdCalls { get; private set; }

    public Task<RiotAccount?> GetAccountAsync(RiotId riotId, CancellationToken ct = default) =>
        Error is null ? Task.FromResult(Account) : Task.FromException<RiotAccount?>(Error);

    public HashSet<string> FailingPuuids { get; } = [];
    public int FreeRotationCalls { get; private set; }

    public Task<RiotSummoner?> GetSummonerAsync(string puuid, CancellationToken ct = default) =>
        Error is null && !FailingPuuids.Contains(puuid)
            ? Task.FromResult(Summoner)
            : Task.FromException<RiotSummoner?>(Error ?? new RiotApiException(System.Net.HttpStatusCode.InternalServerError));

    public IReadOnlyList<int> FreeChampionKeys { get; set; } = [];
    public Dictionary<string, IReadOnlyList<ChampionMastery>> Masteries { get; } = new();

    public Task<IReadOnlyList<int>> GetFreeChampionKeysAsync(CancellationToken ct = default)
    {
        FreeRotationCalls++;
        return Task.FromResult(FreeChampionKeys);
    }

    public Task<IReadOnlyList<ChampionMastery>> GetChampionMasteriesAsync(string puuid, CancellationToken ct = default) =>
        Task.FromResult(Masteries.GetValueOrDefault(puuid) ?? []);

    public Task<IReadOnlyList<LeagueEntry>> GetLeagueEntriesAsync(string puuid, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<LeagueEntry>>([]);

    public Task<IReadOnlyList<string>> GetRecentMatchIdsAsync(string puuid, int count, CancellationToken ct = default)
    {
        MatchIdCalls++;
        return Error is null ? Task.FromResult<IReadOnlyList<string>>(MatchIds.Take(count).ToList()) : Task.FromException<IReadOnlyList<string>>(Error);
    }

    public Task<RiotMatch?> GetMatchAsync(string matchId, CancellationToken ct = default)
    {
        FetchedMatches.Add(matchId);
        return Task.FromResult(Matches.GetValueOrDefault(matchId));
    }
}
