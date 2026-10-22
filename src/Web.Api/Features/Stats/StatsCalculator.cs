using Lol.Shared.Riot;

namespace Web.Api.Features.Stats;

// Summoner's Rift games only (ARAM and Arena stats would mean something else), remakes left out
public static class StatsCalculator
{
    // Draft, Solo/Duo, Blind, Flex, Quickplay: https://static.developer.riotgames.com/docs/lol/queues.json
    public static readonly IReadOnlySet<int> SummonersRiftQueues = new HashSet<int> { 400, 420, 430, 440, 490 };

    private const double SecondsPerMinute = 60.0;

    public static bool Counts(PlayedMatch m) => SummonersRiftQueues.Contains(m.QueueId) && !MatchRules.IsRemake(m.DurationSeconds);

    public static PlayerStats Calculate(string riotId, IReadOnlyList<PlayedMatch> matches, IReadOnlyDictionary<string, string?> iconUrls)
    {
        var counted = matches.Where(Counts).ToList();
        var champions = counted
            .GroupBy(m => m.ChampionName)
            .Select(g => Champion(g.Key, g.ToList(), iconUrls.GetValueOrDefault(g.Key)))
            .OrderByDescending(c => c.Games)
            .ThenByDescending(c => c.WinRate)
            .ToList();
        var recent = matches
            .Where(m => !MatchRules.IsRemake(m.DurationSeconds))
            .Select(m => new RecentMatch(m.MatchId, m.ChampionName, iconUrls.GetValueOrDefault(m.ChampionName), m.Win, m.Kills, m.Deaths,
                m.Assists, PerMinute(m.CreepScore, m.DurationSeconds), RiotNames.Queue(m.QueueId, ""), m.PlayedAt, m.DurationSeconds))
            .ToList();

        var wins = counted.Count(m => m.Win);
        return new PlayerStats(riotId, counted.Count, wins, Rate(wins, counted.Count), Kda(counted), champions, recent);
    }

    private static ChampionStats Champion(string name, List<PlayedMatch> games, string? iconUrl)
    {
        var minutes = games.Sum(m => m.DurationSeconds) / SecondsPerMinute;
        var wins = games.Count(m => m.Win);
        return new ChampionStats(name, iconUrl, games.Count, wins, Rate(wins, games.Count), Kda(games),
            Round(games.Sum(m => m.CreepScore) / minutes), Round(games.Sum(m => m.DamageToChampions) / minutes));
    }

    // (K + A) / D, with deathless games counted as one death so a perfect game isn't infinity
    private static double Kda(IReadOnlyCollection<PlayedMatch> games) =>
        games.Count == 0 ? 0 : Round((games.Sum(m => m.Kills) + games.Sum(m => m.Assists)) / (double)Math.Max(games.Sum(m => m.Deaths), 1));

    private static double Rate(int wins, int games) => games == 0 ? 0 : Round(100.0 * wins / games);

    private static double PerMinute(int value, long seconds) => seconds == 0 ? 0 : Round(value / (seconds / SecondsPerMinute));

    private static double Round(double value) => Math.Round(value, 1);
}
