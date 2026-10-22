using Lol.Shared.Riot.Models;

namespace Lol.Shared.Riot;

// How ranks read for people: "Gold II", "Master" (apex tiers have no division)
public static class RankNames
{
    public static bool IsApexTier(string tier) => tier is "MASTER" or "GRANDMASTER" or "CHALLENGER";

    // "GOLD" → "Gold"
    public static string TierName(string tier) =>
        tier.Length == 0 ? tier : tier[0] + tier[1..].ToLowerInvariant();

    public static string Label(LeagueEntry entry) =>
        IsApexTier(entry.Tier) ? TierName(entry.Tier) : $"{TierName(entry.Tier)} {entry.Rank}";
}
