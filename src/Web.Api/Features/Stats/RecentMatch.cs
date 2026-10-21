namespace Web.Api.Features.Stats;

public record RecentMatch(
    string ChampionName,
    string? IconUrl,
    bool Win,
    int Kills,
    int Deaths,
    int Assists,
    double CsPerMinute,
    string Queue,
    DateTimeOffset PlayedAt);
