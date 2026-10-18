using trevor.Features.Lol.Common;
using trevor.Infrastructure.Riot;
using trevor.Infrastructure.Riot.Models;

namespace trevor.Features.Lol.Tracking;

// A finished match from the tracked players' side, plus the match's worst player as the report candidate
public record MatchReport(string Queue, long DurationSeconds, IReadOnlyList<MatchReportPlayer> Players, MatchReportPlayer Feeder)
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
        if (ours.Count == 0 || info.Participants.Count == 0)
            return null;

        var feeder = info.Participants.OrderBy(KdaRatio).ThenByDescending(p => p.Deaths).First();
        var feederName = names.GetValueOrDefault(feeder.Puuid) ?? RiotNames.RiotId(feeder) ?? feeder.ChampionName;

        return new MatchReport(
            RiotNames.Queue(info.QueueId, info.GameMode),
            info.GameDuration,
            ours,
            MatchReportPlayer.From(feeder, feederName, names.ContainsKey(feeder.Puuid)));
    }

    // Deaths count as at least one, so a deathless game ranks by kills + assists
    private static double KdaRatio(RiotMatchParticipant p) => (double)(p.Kills + p.Assists) / Math.Max(p.Deaths, 1);
}
