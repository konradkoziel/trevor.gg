namespace Web.Api.Features.Stats;

public record ChampionStats(
    string ChampionName,
    string? IconUrl,
    int Games,
    int Wins,
    double WinRate,
    double Kda,
    double CsPerMinute,
    double DamagePerMinute);
