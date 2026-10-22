namespace Web.Api.Features.Stats;

public record RecentMatch(
    string MatchId,
    string ChampionName,
    string? IconUrl,
    bool Win,
    int Kills,
    int Deaths,
    int Assists,
    double CsPerMinute,
    string Queue,
    DateTimeOffset PlayedAt,
    long DurationSeconds);
