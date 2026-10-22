using Lol.Shared.Riot;
using Web.Api.Features.Builds;
using Web.Api.Features.RiotAccount;
using Web.Api.Features.Stats;

namespace Web.Api.Features.Players;

// Builds a linked player's profile: Summoner-V4 icon and level, top masteries, the stats report and their builds
public class PlayerProfiles(IRiotLinkStore links, IRiotApiClient riot, IDataDragon dataDragon, StatsReport stats, IBuildStore builds)
{
    // A podium plus two: enough to see someone's mains without turning into a mastery list
    public const int TopChampionCount = 5;

    // Riot IDs are case-insensitive, and only players who linked their account on the site have a profile
    public async Task<RiotLink?> FindAsync(string gameName, string tagLine, CancellationToken ct = default)
    {
        var riotId = $"{gameName}#{tagLine}";
        return (await links.ListLinksAsync(ct)).FirstOrDefault(l => string.Equals(l.RiotId, riotId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<PlayerProfile> ForAsync(RiotLink link, CancellationToken ct = default)
    {
        var summoner = await riot.GetSummonerAsync(link.Puuid, ct);
        var icon = summoner is null ? null : await dataDragon.ProfileIconUrlAsync(summoner.ProfileIconId, ct);
        var playerBuilds = await builds.ListByOwnerAsync(link.DiscordId, ct);

        return new PlayerProfile(link.RiotId, icon, summoner?.SummonerLevel ?? 0, await TopChampionsAsync(link, ct),
            await stats.ForAsync(link, ct), playerBuilds.Select(BuildSummary.Of).ToList());
    }

    private async Task<IReadOnlyList<MasteryChampion>> TopChampionsAsync(RiotLink link, CancellationToken ct)
    {
        var championsByKey = (await dataDragon.ChampionsAsync(ct))?.ToDictionary(c => c.Key) ?? [];
        return (await riot.GetChampionMasteriesAsync(link.Puuid, ct))
            .OrderByDescending(m => m.ChampionPoints)
            .Select(m => (Mastery: m, Champion: championsByKey.GetValueOrDefault(m.ChampionId)))
            .Where(x => x.Champion is not null)
            .Take(TopChampionCount)
            .Select(x => new MasteryChampion(x.Champion!.Id, x.Champion.Name, x.Champion.IconUrl, x.Mastery.ChampionLevel, x.Mastery.ChampionPoints))
            .ToList();
    }
}
