using Lol.Shared.Riot.Models;

namespace Lol.Shared.Riot;

public interface IRiotApiClient
{
    bool IsConfigured { get; }

    // Null when the Riot ID doesn't exist
    Task<RiotAccount?> GetAccountAsync(RiotId riotId, CancellationToken ct = default);

    // Null when the account has never played League
    Task<RiotSummoner?> GetSummonerAsync(string puuid, CancellationToken ct = default);

    Task<IReadOnlyList<LeagueEntry>> GetLeagueEntriesAsync(string puuid, CancellationToken ct = default);

    // Newest first
    Task<IReadOnlyList<string>> GetRecentMatchIdsAsync(string puuid, int count, CancellationToken ct = default);

    // Null when the match doesn't exist
    Task<RiotMatch?> GetMatchAsync(string matchId, CancellationToken ct = default);
}
