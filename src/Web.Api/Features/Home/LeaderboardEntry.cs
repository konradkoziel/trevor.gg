using Web.Api.Features.Ranks;

namespace Web.Api.Features.Home;

// One person of "Ranking zmarnowanego czasu": mastery points summed over all their accounts, the rest from the main one.
// Riot publishes no hours played, so mastery points and account level stand in
public record LeaderboardEntry(
    string RiotId,
    string? ProfileIconUrl,
    long Level,
    long MasteryPoints,
    string? TopChampionName,
    string? TopChampionIconUrl,
    long TopChampionPoints,
    // Solo/Duo, else Flex; null when unranked
    RankedStanding? Rank = null,
    int AccountCount = 1);
