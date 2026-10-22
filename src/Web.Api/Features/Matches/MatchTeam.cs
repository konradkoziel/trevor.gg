namespace Web.Api.Features.Matches;

public record MatchTeam(int TeamId, bool Win, int Kills, int Deaths, int Assists, int Gold, IReadOnlyList<MatchPlayer> Players);
