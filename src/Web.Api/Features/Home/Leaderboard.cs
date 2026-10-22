using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Ranks;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Features.Home;

// Every person by total mastery points over all their accounts. A few Riot calls per account, so the list is rebuilt
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
        foreach (var person in (await links.ListLinksAsync(ct)).GroupBy(l => l.DiscordId))
        {
            try
            {
                entries.Add(await EntryAsync(person.ToList(), championsByKey, ct));
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

    // Mastery from every account (the same champion's points added up), name, level, icon and rank from the main one
    private async Task<LeaderboardEntry> EntryAsync(
        IReadOnlyList<RiotLink> accounts, IReadOnlyDictionary<int, Champion> championsByKey, CancellationToken ct)
    {
        var main = RiotLinks.MainOf(accounts)!;
        var summoner = await riot.GetSummonerAsync(main.Puuid, ct);
        var rank = RankedStandings.From(await riot.GetLeagueEntriesAsync(main.Puuid, ct)).FirstOrDefault();
        var icon = summoner is null ? null : await dataDragon.ProfileIconUrlAsync(summoner.ProfileIconId, ct);

        var pointsByChampion = new Dictionary<int, long>();
        foreach (var account in accounts)
            foreach (var mastery in await riot.GetChampionMasteriesAsync(account.Puuid, ct))
                pointsByChampion[mastery.ChampionId] = pointsByChampion.GetValueOrDefault(mastery.ChampionId) + mastery.ChampionPoints;
        var top = pointsByChampion.Count == 0 ? (KeyValuePair<int, long>?)null : pointsByChampion.MaxBy(p => p.Value);
        var topChampion = top is null ? null : championsByKey.GetValueOrDefault(top.Value.Key);

        return new LeaderboardEntry(main.RiotId, icon, summoner?.SummonerLevel ?? 0, pointsByChampion.Values.Sum(),
            topChampion?.Name, topChampion?.IconUrl, top?.Value ?? 0, rank, accounts.Count);
    }
}
