using System.Globalization;
using trevor.Infrastructure.Riot;

namespace trevor.Features.Lol.Rank;

// Text of the /rank reply: Discord message, AI input and canned verdicts
public static class RankMessage
{
    public static string Format(string displayName, PlayerRanks ranks, string verdict)
    {
        var lines = new List<string> { $"🏅 **{displayName}**" };
        if (ranks.Solo is not null)
            lines.Add($"Solo/Duo: {Describe(ranks.Solo, bold: true)}");
        if (ranks.Flex is not null)
            lines.Add($"Flex: {Describe(ranks.Flex, bold: true)}");
        lines.Add($"*{verdict}*");

        return string.Join("\n", lines);
    }

    public static string Unranked(string displayName) =>
        $"🏅 **{displayName}** — unranked.\n*Zero rankedów. Boisz się, że wyjdzie na jaw, jaki jesteś słaby?*";

    public static string AiInput(string displayName, PlayerRanks ranks) =>
        $"Gracz: {displayName}\n" +
        $"Solo/Duo: {(ranks.Solo is null ? "brak" : Describe(ranks.Solo, bold: false))}\n" +
        $"Flex: {(ranks.Flex is null ? "brak" : Describe(ranks.Flex, bold: false))}";

    // Used when the AI is unavailable
    public static string FallbackVerdict(string tier) => tier switch
    {
        "IRON" => "Iron. Nawet boty z intro do gry by cię wyniosły.",
        "BRONZE" => "Bronze. Tu się nie gra, tu się cierpi.",
        "SILVER" => "Silver. Ani dobry, ani zły. Po prostu nikt.",
        "GOLD" => "Gold. Gratuluję, jesteś idealnie przeciętny.",
        "PLATINUM" => "Platinum. Myślisz, że umiesz grać. Prawie.",
        "EMERALD" => "Emerald. Zielony jak twoje pojęcie o makro.",
        "DIAMOND" => "Diamond. Dobra, coś tam umiesz. Nie przyzwyczajaj się do pochwał.",
        "MASTER" or "GRANDMASTER" => "Master+. Wyjdź na dwór, serio.",
        "CHALLENGER" => "Challenger. Co ty robisz na tym serwerze z tymi noobami?",
        _ => "Nie wiem, co to za ranga, ale i tak jesteś słaby."
    };

    private static string Describe(LeagueEntry entry, bool bold)
    {
        var games = entry.Wins + entry.Losses;
        var winrate = games == 0 ? 0 : 100.0 * entry.Wins / games;
        var division = IsApexTier(entry.Tier) ? string.Empty : $" {entry.Rank}";
        var b = bold ? "**" : string.Empty;
        return string.Create(CultureInfo.InvariantCulture,
            $"{b}{TierName(entry.Tier)}{division}, {entry.LeaguePoints} LP{b} — {entry.Wins}W / {entry.Losses}L ({winrate:0.0}%)");
    }

    private static bool IsApexTier(string tier) => tier is "MASTER" or "GRANDMASTER" or "CHALLENGER";

    private static string TierName(string tier) =>
        tier.Length == 0 ? tier : tier[0] + tier[1..].ToLowerInvariant();
}
