using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Lol.Tracking;

// The automatic post about a tracked match: an embed with a column per tracked player, plus AI input and canned comments
public static class MatchReportMessage
{
    private const int WinColor = 0x2ECC71;
    private const int LossColor = 0xE74C3C;
    private const int MixedColor = 0x95A5A6;
    private const string FeederTitle = "🚨 Do reportu";

    public static DiscordResponseData Format(MatchReport r, string comment, string? feederIconUrl) =>
        DiscordResponseData.FromEmbed(new DiscordEmbed(
            Title: $"{Result(r)} · {r.Queue} · {r.DurationSeconds / 60} min",
            Color: r.Win switch { true => WinColor, false => LossColor, null => MixedColor },
            Fields:
            [
                .. r.Players.Select(p => new DiscordEmbedField(p.Name, PlayerColumn(p), Inline: true)),
                new DiscordEmbedField(FeederTitle, $"**{r.Feeder.Name}** — {Line(r.Feeder)}{Shame(r.Feeder)}"),
                new DiscordEmbedField(LolMessages.VerdictTitle, $"*{comment}*")
            ],
            Thumbnail: feederIconUrl is null ? null : new DiscordEmbedThumbnail(feederIconUrl),
            Footer: new DiscordEmbedFooter(LolMessages.EmbedFooter)));

    public static string AiInput(MatchReport r) =>
        $"Tryb: {r.Queue}, czas gry: {r.DurationSeconds / 60} min, wynik: {Result(r)}\n" +
        $"Gracze z serwera:\n{string.Join("\n", r.Players.Select(p => $"- {p.Name}: {Line(p)}"))}\n" +
        $"Kandydat do reportu (najgorsze KDA w ich drużynie): {r.Feeder.Name}: {Line(r.Feeder)}{Shame(r.Feeder)}";

    // Used when the AI is unavailable
    public static string FallbackComment(MatchReport r) => r.Win switch
    {
        true => "Wygrana. Ktoś was wyniósł, ale niech wam będzie.",
        false => "Przegrana. Nikt nie jest zaskoczony.",
        null => "Graliście przeciwko sobie. Jedna połowa przegrała, druga i tak jest słaba."
    };

    private static string Result(MatchReport r) => r.Win switch
    {
        true => "✅ Wygrana",
        false => "❌ Przegrana",
        null => "⚔️ Swój na swojego"
    };

    // Narrow inline column: champion and role on top, KDA in bold below
    private static string PlayerColumn(MatchReportPlayer p) =>
        $"{p.Champion}{Role(p)}\n**{p.Kills}/{p.Deaths}/{p.Assists}**";

    private static string Line(MatchReportPlayer p) => $"{p.Champion}{Role(p)} · {p.Kills}/{p.Deaths}/{p.Assists}";

    private static string Role(MatchReportPlayer p) => p.Role is null ? string.Empty : $" ({p.Role})";

    // A full premade has only friends to blame
    private static string Shame(MatchReportPlayer feeder) => feeder.IsTracked ? " — swój, wstyd podwójny" : string.Empty;
}
