using System.Collections.Concurrent;
using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Web.Api.Features.Stats;

// The player's recent matches: ids from Riot, details from storage, only new matches fetched from Riot
public class MatchHistory(IRiotApiClient riot, IMatchHistoryStore store, TimeProvider time)
{
    // Enough for a trend, and at most 21 Riot calls on the first visit (the key allows 100 per 2 minutes)
    public const int MatchCount = 20;

    // Reloading the page doesn't ask Riot again within this time
    public static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(2);

    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastRefresh = new();

    // Newest first
    public async Task<IReadOnlyList<PlayedMatch>> RecentAsync(string puuid, CancellationToken ct = default)
    {
        var stored = await store.ListAsync(puuid, ct);
        if (_lastRefresh.TryGetValue(puuid, out var last) && time.GetUtcNow() - last < RefreshInterval)
            return Newest(stored);

        var ids = await riot.GetRecentMatchIdsAsync(puuid, MatchCount, ct);
        var known = stored.Select(m => m.MatchId).ToHashSet();
        var fetched = new List<PlayedMatch>();
        foreach (var id in ids.Where(id => !known.Contains(id)))
        {
            var match = await riot.GetMatchAsync(id, ct);
            if (match is not null && ToPlayed(id, match, puuid) is { } played)
                fetched.Add(played);
        }
        if (fetched.Count > 0)
            await store.SaveAsync(puuid, fetched, ct);
        _lastRefresh[puuid] = time.GetUtcNow();

        var recentIds = ids.ToHashSet();
        return Newest(stored.Concat(fetched).Where(m => recentIds.Contains(m.MatchId)).ToList());
    }

    private static IReadOnlyList<PlayedMatch> Newest(IEnumerable<PlayedMatch> matches) =>
        matches.OrderByDescending(m => m.PlayedAt).Take(MatchCount).ToList();

    private static PlayedMatch? ToPlayed(string matchId, RiotMatch match, string puuid)
    {
        var p = match.Info.Participants.FirstOrDefault(x => x.Puuid == puuid);
        return p is null
            ? null
            : new PlayedMatch(matchId, p.ChampionName, p.Win, p.Kills, p.Deaths, p.Assists,
                p.TotalMinionsKilled + p.NeutralMinionsKilled, p.TotalDamageDealtToChampions, match.Info.GameDuration,
                match.Info.QueueId, DateTimeOffset.FromUnixTimeMilliseconds(match.Info.GameEndTimestamp));
    }
}
