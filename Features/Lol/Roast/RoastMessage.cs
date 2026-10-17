using System.Globalization;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Lol.Roast;

// The /roast reply: an embed with the match stats, plus AI input and canned lines
public static class RoastMessage
{
    private const int WinColor = 0x2ECC71;
    private const int LossColor = 0xE74C3C;
    private const int RemakeColor = 0x95A5A6;
    private const string Footer = "Trevor.gg · Riot API";
    private const string VerdictTitle = "Werdykt Trevora";
    private const string RemakeText = "*Remake. Nawet nie zdążyłeś zawieść, a już po meczu.*";

    public static DiscordResponseData Format(string displayName, MatchStats stats, string roast, string? championIconUrl) =>
        Card(displayName, stats, championIconUrl, stats.Win ? WinColor : LossColor, roast);

    public static DiscordResponseData Remake(string displayName, MatchStats stats, string? championIconUrl) =>
        Card(displayName, stats, championIconUrl, RemakeColor, RemakeText);

    public static DiscordResponseData NoMatches(string displayName) =>
        DiscordResponseData.FromText($"**{displayName}** nie ma żadnego meczu. Nie da się zroastować kogoś, kto nawet nie gra.");

    // Used when the AI is unavailable
    public static string FallbackRoast(MatchStats s) =>
        s.Deaths > s.Kills + s.Assists
            ? $"*{s.Deaths} śmierci. Wrogowie powinni ci płacić za darmowe złoto.*"
            : s.Win
                ? "*Wygrana. Pewnie cię ktoś wyniósł, ale niech ci będzie.*"
                : "*Przegrana. Nie wiem, co gorsze: twój mecz czy to, że chciałeś, żebym go oceniał.*";

    public static string AiInput(string displayName, MatchStats s) => string.Create(CultureInfo.InvariantCulture,
        $"""
         Gracz: {displayName}
         Postać: {s.Champion}, rola: {s.Role ?? "brak"}, tryb: {s.Queue}
         Wynik: {(s.Win ? "wygrana" : "przegrana")}, czas gry: {s.DurationSeconds / 60} min
         KDA: {s.Kills}/{s.Deaths}/{s.Assists}
         CS: {s.Cs} ({s.CsPerMinute:0.0}/min)
         Obrażenia w bohaterów: {s.Damage} ({s.DamageShare:0%} drużyny, {s.DamageRankInTeam}. miejsce w drużynie)
         Udział w zabójstwach: {s.KillParticipation:0%}
         Wynik wizji: {s.VisionScore}
         Złoto: {s.Gold}
         """);

    private static DiscordResponseData Card(string displayName, MatchStats s, string? championIconUrl, int color, string verdict)
    {
        var role = s.Role is null ? string.Empty : $" ({s.Role})";
        var result = s.Win ? "Wygrana" : "Przegrana";
        return DiscordResponseData.FromEmbed(new DiscordEmbed(
            Title: $"🔥 {displayName} — {s.Champion}{role}",
            Description: $"{s.Queue} · **{result}** · {s.DurationSeconds / 60} min",
            Color: color,
            Fields: [.. StatFields(s), new DiscordEmbedField(VerdictTitle, verdict)],
            Thumbnail: championIconUrl is null ? null : new DiscordEmbedThumbnail(championIconUrl),
            Footer: new DiscordEmbedFooter(Footer)));
    }

    private static IEnumerable<DiscordEmbedField> StatFields(MatchStats s)
    {
        string F(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);

        yield return new DiscordEmbedField("KDA", F($"{s.Kills}/{s.Deaths}/{s.Assists}"), Inline: true);
        yield return new DiscordEmbedField("CS", F($"{s.Cs} ({s.CsPerMinute:0.0}/min)"), Inline: true);
        yield return new DiscordEmbedField("Obrażenia", F($"{s.Damage / 1000.0:0.0}k ({s.DamageShare:0%})"), Inline: true);
        yield return new DiscordEmbedField("Udział w zabójstwach", F($"{s.KillParticipation:0%}"), Inline: true);
        yield return new DiscordEmbedField("Wizja", F($"{s.VisionScore}"), Inline: true);
        yield return new DiscordEmbedField("Złoto", F($"{s.Gold / 1000.0:0.0}k"), Inline: true);
    }
}
