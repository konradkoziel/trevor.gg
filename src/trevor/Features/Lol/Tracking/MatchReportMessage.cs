using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Lol.Tracking;

// The automatic post about a tracked match: a column per tracked player, then the team's MVP and report candidate
public static class MatchReportMessage
{
    private const int WinColor = 0x2ECC71;
    private const int LossColor = 0xE74C3C;
    private const int MixedColor = 0x95A5A6;
    private const string MvpTitle = "🏆 MVP";
    private const string FeederTitle = "🚨 Do reportu";

    public static DiscordResponseData Format(MatchReport r, string? mvpIconUrl) =>
        DiscordResponseData.FromEmbed(new DiscordEmbed(
            Title: $"{Result(r)} · {r.Queue} · {r.DurationSeconds / 60} min",
            Color: r.Win switch { true => WinColor, false => LossColor, null => MixedColor },
            Fields:
            [
                .. r.Players.Select(p => new DiscordEmbedField(p.Name, PlayerColumn(p), Inline: true)),
                new DiscordEmbedField(MvpTitle, $"**{r.Mvp.Name}** — {Line(r.Mvp)}"),
                new DiscordEmbedField(FeederTitle, $"**{r.Feeder.Name}** — {Line(r.Feeder)}{Shame(r.Feeder)}")
            ],
            Thumbnail: mvpIconUrl is null ? null : new DiscordEmbedThumbnail(mvpIconUrl),
            Footer: new DiscordEmbedFooter(LolMessages.EmbedFooter)));

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

    // One of the friends being the worst deserves a mention
    private static string Shame(MatchReportPlayer feeder) => feeder.IsTracked ? " — swój, wstyd podwójny" : string.Empty;
}
