using System.Collections.Concurrent;
using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.Extensions.Options;

namespace Web.Api.Features.Stats;

// The player's recent matches: ids from Riot, details from storage, only new matches fetched from Riot.
// Storage keeps just the last MatchHistoryOptions.Games of them: older ones are deleted on the next refresh
public class MatchHistory(IRiotApiClient riot, IMatchHistoryStore store, TimeProvider time, IOptions<MatchHistoryOptions> options)
{
    // Reloading the page doesn't ask Riot again within this time
    public static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(2);

    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastRefresh = new();

    // Newest first
    public async Task<IReadOnlyList<PlayedMatch>> RecentAsync(string puuid, CancellationToken ct = default)
    {
        var stored = await store.ListAsync(puuid, ct);
        if (_lastRefresh.TryGetValue(puuid, out var last) && time.GetUtcNow() - last < RefreshInterval)
            return Newest(stored);

        var ids = await riot.GetRecentMatchIdsAsync(puuid, Games, ct);
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
        await RemoveOlderAsync(puuid, stored, recentIds, ct);
        return Newest(stored.Concat(fetched).Where(m => recentIds.Contains(m.MatchId)).ToList());
    }

    private int Games => options.Value.Games;

    // Whatever Riot no longer lists among the last games is older than all of them. An empty list from Riot deletes
    // nothing, so a hiccup on their side can't wipe someone's history
    private async Task RemoveOlderAsync(string puuid, IReadOnlyList<PlayedMatch> stored, IReadOnlySet<string> recentIds, CancellationToken ct)
    {
        if (recentIds.Count == 0)
            return;
        var older = stored.Where(m => !recentIds.Contains(m.MatchId)).Select(m => m.MatchId).ToList();
        if (older.Count > 0)
            await store.DeleteAsync(puuid, older, ct);
    }

    private IReadOnlyList<PlayedMatch> Newest(IEnumerable<PlayedMatch> matches) =>
        matches.OrderByDescending(m => m.PlayedAt).Take(Games).ToList();

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
