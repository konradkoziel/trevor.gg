using System.Globalization;
using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Riot;
using trevor.Infrastructure.Riot.Models;

namespace trevor.Features.Lol.Rank;

// The /rank reply: an embed with Solo/Duo and Flex, plus AI input and canned verdicts
public static class RankMessage
{
    // Unranked players get the same grey as /roast remakes: nothing happened
    private const int UnrankedColor = 0x95A5A6;
    private const string SoloTitle = "Solo/Duo";
    private const string FlexTitle = "Flex";
    private const string UnrankedText = "Unranked. *Zero rankedów. Boisz się, że wyjdzie na jaw, jaki jesteś słaby?*";

    public static DiscordResponseData Format(string displayName, PlayerRanks ranks, string verdict)
    {
        var main = ranks.Main!;
        return DiscordResponseData.FromEmbed(new DiscordEmbed(
            Title: Title(displayName),
            Color: TierColor(main.Tier),
            Fields: [.. QueueFields(ranks), new DiscordEmbedField(LolMessages.VerdictTitle, $"*{verdict}*")],
            Thumbnail: new DiscordEmbedThumbnail(CommunityDragon.RankEmblemUrl(main.Tier)),
            Footer: new DiscordEmbedFooter(LolMessages.EmbedFooter)));
    }

    public static DiscordResponseData Unranked(string displayName) =>
        DiscordResponseData.FromEmbed(new DiscordEmbed(
            Title: Title(displayName),
            Description: UnrankedText,
            Color: UnrankedColor,
            Footer: new DiscordEmbedFooter(LolMessages.EmbedFooter)));

    public static string AiInput(string displayName, PlayerRanks ranks) =>
        $"Gracz: {displayName}\n" +
        $"Solo/Duo: {(ranks.Solo is null ? "brak" : Describe(ranks.Solo))}\n" +
        $"Flex: {(ranks.Flex is null ? "brak" : Describe(ranks.Flex))}";

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

    private static string Title(string displayName) => $"🏅 {displayName}";

    private static IEnumerable<DiscordEmbedField> QueueFields(PlayerRanks ranks)
    {
        if (ranks.Solo is not null)
            yield return new DiscordEmbedField(SoloTitle, FieldValue(ranks.Solo), Inline: true);
        if (ranks.Flex is not null)
            yield return new DiscordEmbedField(FlexTitle, FieldValue(ranks.Flex), Inline: true);
    }

    // Two lines in a narrow inline column: rank on top, record below
    private static string FieldValue(LeagueEntry entry) => $"**{RankName(entry)}**\n{Record(entry)}";

    private static string Describe(LeagueEntry entry) => $"{RankName(entry)} — {Record(entry)}";

    private static string RankName(LeagueEntry entry)
    {
        var division = IsApexTier(entry.Tier) ? string.Empty : $" {entry.Rank}";
        return $"{TierName(entry.Tier)}{division}, {entry.LeaguePoints} LP";
    }

    private static string Record(LeagueEntry entry)
    {
        var games = entry.Wins + entry.Losses;
        var winrate = games == 0 ? 0 : 100.0 * entry.Wins / games;
        return string.Create(CultureInfo.InvariantCulture, $"{entry.Wins}W / {entry.Losses}L ({winrate:0.0}%)");
    }

    // Embed bar in the colour of the tier's emblem
    private static int TierColor(string tier) => tier switch
    {
        "IRON" => 0x5E5150,
        "BRONZE" => 0x8C5A3C,
        "SILVER" => 0x8A9BA8,
        "GOLD" => 0xC89B3C,
        "PLATINUM" => 0x4E9996,
        "EMERALD" => 0x149C5B,
        "DIAMOND" => 0x576BCE,
        "MASTER" => 0x9D48E0,
        "GRANDMASTER" => 0xCD4545,
        "CHALLENGER" => 0xF4C874,
        _ => UnrankedColor
    };

    private static bool IsApexTier(string tier) => tier is "MASTER" or "GRANDMASTER" or "CHALLENGER";

    private static string TierName(string tier) =>
        tier.Length == 0 ? tier : tier[0] + tier[1..].ToLowerInvariant();
}
