using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Web.Api.Features.Matches;

// Match-V5 → what the expanded game in "Ostatnie gry" shows
public static class MatchDetails
{
    // Riot's ids of the two sides
    private const int BlueTeam = 100;

    public static MatchDetail From(string matchId, RiotMatch match, MatchAssets assets)
    {
        var info = match.Info;
        var teams = info.Participants
            .GroupBy(p => p.Squad())
            .Select(g => Team(g.Key, g.ToList(), assets))
            .OrderBy(t => t.Placement ?? (t.TeamId == BlueTeam ? 0 : 1))
            .ToList();
        return new MatchDetail(matchId, RiotNames.Queue(info.QueueId, info.GameMode), info.GameDuration,
            DateTimeOffset.FromUnixTimeMilliseconds(info.GameEndTimestamp), teams);
    }

    private static MatchTeam Team(int teamId, IReadOnlyList<RiotMatchParticipant> players, MatchAssets assets) =>
        new(teamId, Placement(players), players.Any(p => p.Win), players.Sum(p => p.Kills), players.Sum(p => p.Deaths), players.Sum(p => p.Assists),
            players.Sum(p => p.GoldEarned), players.Select(p => Player(p, assets)).ToList());

    // Only Arena pairs have a place; 0 means another mode
    private static int? Placement(IReadOnlyList<RiotMatchParticipant> players) =>
        players[0].SubteamPlacement > 0 ? players[0].SubteamPlacement : null;

    private static MatchPlayer Player(RiotMatchParticipant p, MatchAssets assets)
    {
        var primary = p.Perks?.Styles.ElementAtOrDefault(0);
        var secondary = p.Perks?.Styles.ElementAtOrDefault(1);
        var keystone = primary?.Selections.FirstOrDefault() is { } first ? assets.Runes.GetValueOrDefault(first.Perk) : null;
        var tree = secondary is null ? null : assets.Trees.GetValueOrDefault(secondary.Style);

        return new MatchPlayer(
            RiotNames.RiotId(p),
            p.ChampionName,
            assets.ChampionIcons.GetValueOrDefault(p.ChampionName),
            p.ChampLevel,
            p.Kills,
            p.Deaths,
            p.Assists,
            p.TotalMinionsKilled + p.NeutralMinionsKilled,
            p.TotalDamageDealtToChampions,
            p.GoldEarned,
            p.VisionScore,
            p.ItemIds().Select(id => id == 0 ? null : assets.Items.GetValueOrDefault(id)).ToList(),
            new[] { p.Summoner1Id, p.Summoner2Id }
                .Select(id => assets.Spells.GetValueOrDefault(id))
                .OfType<SummonerSpell>()
                .Select(s => new MatchIcon(s.Name, s.IconUrl))
                .ToList(),
            keystone is null ? null : new MatchIcon(keystone.Name, keystone.IconUrl),
            tree is null ? null : new MatchIcon(tree.Name, tree.IconUrl));
    }
}
