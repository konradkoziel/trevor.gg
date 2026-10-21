using Lol.Shared.Riot.Models;
using Lol.Shared.Riot;

namespace trevor.Features.Lol.Tracking;

// One line of a match report: who played what and how it went
public record MatchReportPlayer(string Name, string Champion, string? Role, bool Win, int Kills, int Deaths, int Assists, bool IsTracked)
{
    public static MatchReportPlayer From(RiotMatchParticipant p, string name, bool isTracked) =>
        new(name, p.ChampionName, RiotNames.Role(p.TeamPosition), p.Win, p.Kills, p.Deaths, p.Assists, isTracked);
}
