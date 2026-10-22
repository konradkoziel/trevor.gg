using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Ranks;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Features.Home;

// Every linked player by total mastery points. Three Riot calls per player, so the list is rebuilt
// at most every few hours per app instance; a player Riot fails on is left out of that round
public class Leaderboard(IRiotLinkStore links, IRiotApiClient riot, IDataDragon dataDragon, TimeProvider time, ILogger<Leaderboard> logger)
{
    public static readonly TimeSpan CacheFor = TimeSpan.FromHours(3);

    private (IReadOnlyList<LeaderboardEntry> Entries, DateTimeOffset At)? _cached;

    public async Task<IReadOnlyList<LeaderboardEntry>> EntriesAsync(CancellationToken ct = default)
    {
        if (_cached is { } cached && time.GetUtcNow() - cached.At < CacheFor)
            return cached.Entries;

        var championsByKey = (await dataDragon.ChampionsAsync(ct))?.ToDictionary(c => c.Key) ?? [];
        var entries = new List<LeaderboardEntry>();
        foreach (var link in await links.ListLinksAsync(ct))
        {
            try
            {
                entries.Add(await EntryAsync(link, championsByKey, ct));
            }
            catch (Exception ex) when (ex is RiotApiException or HttpRequestException)
            {
                logger.LogWarning(ex, "Leaderboard: skipped a player Riot failed on");
            }
        }

        var sorted = entries.OrderByDescending(e => e.MasteryPoints).ThenByDescending(e => e.Level).ToList();
        _cached = (sorted, time.GetUtcNow());
        return sorted;
    }

    private async Task<LeaderboardEntry> EntryAsync(RiotLink link, IReadOnlyDictionary<int, Champion> championsByKey, CancellationToken ct)
    {
        var summoner = await riot.GetSummonerAsync(link.Puuid, ct);
        var masteries = await riot.GetChampionMasteriesAsync(link.Puuid, ct);
        var rank = RankedStandings.From(await riot.GetLeagueEntriesAsync(link.Puuid, ct)).FirstOrDefault();
        var top = masteries.MaxBy(m => m.ChampionPoints);
        var topChampion = top is null ? null : championsByKey.GetValueOrDefault(top.ChampionId);
        var icon = summoner is null ? null : await dataDragon.ProfileIconUrlAsync(summoner.ProfileIconId, ct);

        return new LeaderboardEntry(link.RiotId, icon, summoner?.SummonerLevel ?? 0, masteries.Sum(m => m.ChampionPoints),
            topChampion?.Name, topChampion?.IconUrl, top?.ChampionPoints ?? 0, rank);
    }
}
