namespace Web.Api.Features.Home;

// One player of "Ranking zmarnowanego czasu". Riot publishes no hours played, so mastery points and account level stand in
public record LeaderboardEntry(
    string RiotId,
    string? ProfileIconUrl,
    long Level,
    long MasteryPoints,
    string? TopChampionName,
    string? TopChampionIconUrl,
    long TopChampionPoints);
