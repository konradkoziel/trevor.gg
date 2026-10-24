using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Ranks;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Features.Home;

// Every person by total mastery points over all their accounts. A few Riot calls per account, so the list is rebuilt
// at most every few hours per app instance; a player Riot fails on is left out of that round. An old list is served
// while the new one is built, so only the very first visitor of an instance waits for Riot
public class Leaderboard(IRiotLinkStore links, IRiotApiClient riot, IDataDragon dataDragon, TimeProvider time, ILogger<Leaderboard> logger)
{
    public static readonly TimeSpan CacheFor = TimeSpan.FromHours(3);

    // Players looked up at once: quicker than one by one, but stays under Riot's per-second limit of our key
    public const int ParallelPlayers = 4;

    private readonly Lock _lock = new();
    private (IReadOnlyList<LeaderboardEntry> Entries, DateTimeOffset At)? _cached;
    private Task<IReadOnlyList<LeaderboardEntry>>? _rebuild;

    public async Task<IReadOnlyList<LeaderboardEntry>> EntriesAsync(CancellationToken ct = default)
    {
        if (_cached is not { } cached)
            return await RebuildAsync().WaitAsync(ct);

        if (IsStale(cached.At))
            _ = RebuildInBackgroundAsync();
        return cached.Entries;
    }

    // For the warm-up timer: rebuilds a missing or old list, so visitors find it ready
    public async Task WarmAsync()
    {
        if (_cached is not { } cached || IsStale(cached.At) || _rebuild is { IsCompleted: false })
            await RebuildAsync();
    }

    private bool IsStale(DateTimeOffset at) => time.GetUtcNow() - at >= CacheFor;

    // One rebuild at a time, shared by everyone asking meanwhile; not cancelled when the visitor who started it leaves
    private Task<IReadOnlyList<LeaderboardEntry>> RebuildAsync()
    {
        lock (_lock)
            return _rebuild is { IsCompleted: false } running ? running : _rebuild = BuildAsync();
    }

    private async Task RebuildInBackgroundAsync()
    {
        try
        {
            await RebuildAsync();
        }
        catch (Exception ex)
        {
            // The old list stays; the next visit tries again
            logger.LogWarning(ex, "Leaderboard: rebuilding failed");
        }
    }

    private async Task<IReadOnlyList<LeaderboardEntry>> BuildAsync()
    {
        var championsByKey = (await dataDragon.ChampionsAsync())?.ToDictionary(c => c.Key) ?? [];
        var people = (await links.ListLinksAsync()).GroupBy(l => l.DiscordId).Select(g => g.ToList()).ToList();
        var entries = new LeaderboardEntry?[people.Count];
        await Parallel.ForEachAsync(
            Enumerable.Range(0, people.Count),
            new ParallelOptions { MaxDegreeOfParallelism = ParallelPlayers },
            async (i, ct) => entries[i] = await EntryOrNullAsync(people[i], championsByKey, ct));

        var sorted = entries.OfType<LeaderboardEntry>()
            .OrderByDescending(e => e.MasteryPoints).ThenByDescending(e => e.Level).ToList();
        _cached = (sorted, time.GetUtcNow());
        return sorted;
    }

    private async Task<LeaderboardEntry?> EntryOrNullAsync(
        IReadOnlyList<RiotLink> accounts, IReadOnlyDictionary<int, Champion> championsByKey, CancellationToken ct)
    {
        try
        {
            return await EntryAsync(accounts, championsByKey, ct);
        }
        catch (Exception ex) when (ex is RiotApiException or HttpRequestException)
        {
            logger.LogWarning(ex, "Leaderboard: skipped a player Riot failed on");
            return null;
        }
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
