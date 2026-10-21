namespace Web.Api.Features.Stats;

// GET /api/me/stats: Games/Wins/Kda over the counted matches, champions by games played
public record PlayerStats(
    string RiotId,
    int Games,
    int Wins,
    double WinRate,
    double Kda,
    IReadOnlyList<ChampionStats> Champions,
    IReadOnlyList<RecentMatch> Recent);
