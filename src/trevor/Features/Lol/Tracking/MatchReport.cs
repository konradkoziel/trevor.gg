using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace trevor.Features.Lol.Tracking;

// A finished match from the tracked players' side: their whole team (their pair in Arena, both teams when friends
// played each other), the champions they played against, and the best (MVP) and the worst (report candidate) of the team
// by PerformanceScore
public record MatchReport(
    string Queue,
    long DurationSeconds,
    IReadOnlyList<MatchReportPlayer> Players,
    MatchReportPlayer Mvp,
    MatchReportPlayer Feeder,
    // Every team with a tracked player in it, in Riot's order (by role on Summoner's Rift)
    IReadOnlyList<IReadOnlyList<MatchReportPlayer>> Teams,
    // Riot's names of the other team's champions; empty in Arena and when friends played each other
    IReadOnlyList<string> Opponents,
    // The pair's final place in Arena, else null
    int? Placement)
{
    public bool IsRemake => MatchRules.IsRemake(DurationSeconds);

    // Null when tracked players were on both teams (e.g. a custom game among friends)
    public bool? Win => Players.All(p => p.Win) ? true : Players.All(p => !p.Win) ? false : null;

    // Null when none of the tracked players is in the match
    public static MatchReport? For(RiotMatch match, IReadOnlyList<TrackedPlayer> tracked)
    {
        var info = match.Info;
        var names = tracked.ToDictionary(t => t.Puuid, t => t.RiotId);
        var ours = info.Participants
            .Where(p => names.ContainsKey(p.Puuid))
            .Select(p => MatchReportPlayer.From(p, names[p.Puuid], isTracked: true))
            .ToList();
        if (ours.Count == 0)
            return null;

        var squads = info.Participants.GroupBy(p => p.Squad()).ToList();
        var ourSquads = squads.Where(s => s.Any(p => names.ContainsKey(p.Puuid))).ToList();
        var candidates = ourSquads.Count == 1 ? ourSquads[0].ToList() : info.Participants;
        var scores = candidates.ToDictionary(p => p.Puuid, p => PerformanceScore.Of(p, squads.First(s => s.Key == p.Squad()).ToList()));
        var mvp = candidates.OrderByDescending(p => scores[p.Puuid]).ThenByDescending(KdaRatio).First();
        var feeder = candidates.OrderBy(p => scores[p.Puuid]).ThenBy(KdaRatio).First();
        var placement = ourSquads.Count == 1 && ourSquads[0].First().SubteamPlacement > 0 ? ourSquads[0].First().SubteamPlacement : (int?)null;

        return new MatchReport(
            RiotNames.Queue(info.QueueId, info.GameMode),
            info.GameDuration,
            ours,
            Describe(mvp, names),
            Describe(feeder, names),
            ourSquads.Select(s => (IReadOnlyList<MatchReportPlayer>)s.Select(p => Describe(p, names)).ToList()).ToList(),
            ourSquads.Count == 1 && placement is null
                ? squads.Where(s => s.Key != ourSquads[0].Key).SelectMany(s => s).Select(p => p.ChampionName).ToList()
                : [],
            placement);
    }

    // Tracked players by their stored Riot ID, everyone else by the Riot ID in the match, or the champion
    private static MatchReportPlayer Describe(RiotMatchParticipant p, Dictionary<string, string> trackedNames) =>
        MatchReportPlayer.From(
            p,
            trackedNames.GetValueOrDefault(p.Puuid) ?? RiotNames.RiotId(p) ?? p.ChampionName,
            trackedNames.ContainsKey(p.Puuid));

    // Only breaks ties of the score. Deaths count as at least one, so a deathless game ranks by kills + assists
    private static double KdaRatio(RiotMatchParticipant p) => (double)(p.Kills + p.Assists) / Math.Max(p.Deaths, 1);
}
