using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace trevor.Features.Lol.Tracking;

// A finished match from the tracked players' side: who of them played, plus the best (MVP) and the worst
// (report candidate) player of their team — tracked players included
public record MatchReport(
    string Queue, long DurationSeconds, IReadOnlyList<MatchReportPlayer> Players, MatchReportPlayer Mvp, MatchReportPlayer Feeder)
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

        var candidates = TheirTeam(info.Participants, names.Keys.ToHashSet());
        var mvp = candidates
            .OrderByDescending(KdaRatio).ThenByDescending(p => p.Kills + p.Assists).ThenByDescending(p => p.TotalDamageDealtToChampions)
            .First();
        var feeder = candidates
            .OrderBy(KdaRatio).ThenByDescending(p => p.Deaths).ThenBy(p => p.TotalDamageDealtToChampions)
            .First();

        return new MatchReport(
            RiotNames.Queue(info.QueueId, info.GameMode),
            info.GameDuration,
            ours,
            Describe(mvp, names),
            Describe(feeder, names));
    }

    // The tracked players' team (their pair in Arena); the whole match when tracked players were on both teams (a custom game among friends)
    private static IReadOnlyList<RiotMatchParticipant> TheirTeam(IReadOnlyList<RiotMatchParticipant> participants, HashSet<string> tracked)
    {
        var teams = participants.Where(p => tracked.Contains(p.Puuid)).Select(p => p.Squad()).Distinct().ToList();
        return teams.Count == 1 ? participants.Where(p => p.Squad() == teams[0]).ToList() : participants;
    }

    // Tracked players by their stored Riot ID, everyone else by the Riot ID in the match, or the champion
    private static MatchReportPlayer Describe(RiotMatchParticipant p, Dictionary<string, string> trackedNames) =>
        MatchReportPlayer.From(
            p,
            trackedNames.GetValueOrDefault(p.Puuid) ?? RiotNames.RiotId(p) ?? p.ChampionName,
            trackedNames.ContainsKey(p.Puuid));

    // Deaths count as at least one, so a deathless game ranks by kills + assists
    private static double KdaRatio(RiotMatchParticipant p) => (double)(p.Kills + p.Assists) / Math.Max(p.Deaths, 1);
}
