using Lol.Shared.Riot.Models;
using Lol.Shared.Riot;

namespace trevor.Features.Lol.Tracking;

// One line of a match report: who played what and how it went. Champion is Riot's name from the match (MonkeyKing)
public record MatchReportPlayer(
    string Puuid, string Name, string Champion, string? Role, bool Win, int Kills, int Deaths, int Assists, bool IsTracked)
{
    public static MatchReportPlayer From(RiotMatchParticipant p, string name, bool isTracked) =>
        new(p.Puuid, name, p.ChampionName, RiotNames.Role(p.TeamPosition), p.Win, p.Kills, p.Deaths, p.Assists, isTracked);
}
