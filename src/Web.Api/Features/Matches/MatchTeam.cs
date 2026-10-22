namespace Web.Api.Features.Matches;

// TeamId: 100 blue, 200 red, or the pair's number in Arena; Placement: the pair's final place in Arena, else null
public record MatchTeam(
    int TeamId, int? Placement, bool Win, int Kills, int Deaths, int Assists, int Gold, IReadOnlyList<MatchPlayer> Players);
