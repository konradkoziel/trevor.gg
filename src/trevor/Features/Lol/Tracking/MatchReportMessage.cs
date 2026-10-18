using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Lol.Tracking;

// The automatic post about a tracked match: a short embed, plus AI input and canned comments
public static class MatchReportMessage
{
    private const int WinColor = 0x2ECC71;
    private const int LossColor = 0xE74C3C;
    private const int MixedColor = 0x95A5A6;
    private const string FeederTitle = "🚨 Do reportu";

    public static DiscordResponseData Format(MatchReport r, string comment) =>
        DiscordResponseData.FromEmbed(new DiscordEmbed(
            Title: $"{Result(r)} · {r.Queue} · {r.DurationSeconds / 60} min",
            Description: string.Join("\n", r.Players.Select(p => $"**{p.Name}** — {Line(p)}")),
            Color: r.Win switch { true => WinColor, false => LossColor, null => MixedColor },
            Fields:
            [
                new DiscordEmbedField(FeederTitle, $"**{r.Feeder.Name}** — {Line(r.Feeder)}{Side(r)}"),
                new DiscordEmbedField(LolMessages.VerdictTitle, $"*{comment}*")
            ],
            Footer: new DiscordEmbedFooter(LolMessages.EmbedFooter)));

    public static string AiInput(MatchReport r) =>
        $"Tryb: {r.Queue}, czas gry: {r.DurationSeconds / 60} min, wynik: {Result(r)}\n" +
        $"Gracze z serwera:\n{string.Join("\n", r.Players.Select(p => $"- {p.Name}: {Line(p)}"))}\n" +
        $"Kandydat do reportu (najgorsze KDA w meczu): {r.Feeder.Name}: {Line(r.Feeder)}{Side(r)}";

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

    private static string Line(MatchReportPlayer p)
    {
        var role = p.Role is null ? string.Empty : $" ({p.Role})";
        return $"{p.Champion}{role} · {p.Kills}/{p.Deaths}/{p.Assists}";
    }

    private static string Side(MatchReport r) => r.Feeder switch
    {
        { IsTracked: true } => " — swój, wstyd podwójny",
        _ when r.Win is null => string.Empty,
        var f when f.Win == r.Win => " (z waszej drużyny)",
        _ => " (z drużyny wroga)"
    };
}
