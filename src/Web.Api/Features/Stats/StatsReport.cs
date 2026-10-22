using Lol.Shared.Riot;
using Web.Api.Features.Ranks;
using Web.Api.Features.RiotAccount;

namespace Web.Api.Features.Stats;

// The stats of one linked player: recent matches plus both ranked queues. Shared by "Statystyki" and the player profiles
public class StatsReport(IRiotApiClient riot, MatchHistory history, IDataDragon dataDragon)
{
    public async Task<PlayerStats> ForAsync(RiotLink link, CancellationToken ct = default)
    {
        var matches = await history.RecentAsync(link.Puuid, ct);
        var icons = new Dictionary<string, string?>();
        foreach (var champion in matches.Select(m => m.ChampionName).Distinct())
            icons[champion] = await dataDragon.ChampionIconUrlAsync(champion, ct);
        var ranks = RankedStandings.From(await riot.GetLeagueEntriesAsync(link.Puuid, ct));
        return StatsCalculator.Calculate(link.RiotId, matches, icons) with { Ranks = ranks };
    }
}
