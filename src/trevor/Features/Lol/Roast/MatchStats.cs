using Lol.Shared.Riot.Models;
using Lol.Shared.Riot;

namespace trevor.Features.Lol.Roast;

// One player's numbers from a match, compared with their team
public record MatchStats(
    string Champion,
    string? Role,
    string Queue,
    bool Win,
    long DurationSeconds,
    int Kills,
    int Deaths,
    int Assists,
    int Cs,
    double CsPerMinute,
    int Damage,
    double DamageShare,
    int DamageRankInTeam,
    double KillParticipation,
    int VisionScore,
    int Gold)
{
    // Null when the player isn't in the match
    public static MatchStats? For(RiotMatch match, string puuid)
    {
        var info = match.Info;
        var me = info.Participants.FirstOrDefault(p => p.Puuid == puuid);
        if (me is null)
            return null;

        var team = info.Participants.Where(p => p.Squad() == me.Squad()).ToList();
        var teamKills = team.Sum(p => p.Kills);
        var teamDamage = team.Sum(p => p.TotalDamageDealtToChampions);
        var minutes = Math.Max(info.GameDuration / 60.0, 1);
        var cs = me.TotalMinionsKilled + me.NeutralMinionsKilled;

        return new MatchStats(
            me.ChampionName,
            RiotNames.Role(me.TeamPosition),
            RiotNames.Queue(info.QueueId, info.GameMode),
            me.Win,
            info.GameDuration,
            me.Kills, me.Deaths, me.Assists,
            cs,
            cs / minutes,
            me.TotalDamageDealtToChampions,
            Ratio(me.TotalDamageDealtToChampions, teamDamage),
            1 + team.Count(p => p.TotalDamageDealtToChampions > me.TotalDamageDealtToChampions),
            Ratio(me.Kills + me.Assists, teamKills),
            me.VisionScore,
            me.GoldEarned);
    }

    private static double Ratio(int part, int whole) => whole == 0 ? 0 : (double)part / whole;
}
