using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Lol.Tracking;

// The automatic post about a tracked match: the whole team one player per line (no inline columns, which Discord
// squeezes and wraps), the MVP and the report candidate marked on their own lines, then the opponents' champions
public static class MatchReportMessage
{
    private const int WinColor = 0x2ECC71;
    private const int LossColor = 0xE74C3C;
    private const int MixedColor = 0x95A5A6;
    private const string OurTeamTitle = "Nasza drużyna";
    private const string OurPairTitle = "Nasza para";
    private const string TeamTitle = "Drużyna";
    private const string OpponentsTitle = "Przeciwnicy";
    private const string MvpMark = "🏆 MVP";
    private const string FeederMark = "🚨 do reportu";
    private const string TrackedFeederMark = "🚨 do reportu, swój, wstyd podwójny";
    private const string DetailsText = "🔍 Cały mecz na stronie: obie drużyny, buildy i obrażenia";

    // matchUrl: the game's page on the website; without it the card has no link.
    // championNames: Data Dragon id → Polish name (MonkeyKing → Wukong); a champion missing from it keeps Riot's name
    public static DiscordResponseData Format(
        MatchReport r, string? mvpIconUrl, string? matchUrl = null, IReadOnlyDictionary<string, string>? championNames = null)
    {
        var names = championNames ?? new Dictionary<string, string>();
        return DiscordResponseData.FromEmbed(new DiscordEmbed(
            Title: $"{Result(r)} · {r.Queue} · {r.DurationSeconds / 60} min",
            Description: Description(r, matchUrl, names),
            Color: r.Win switch { true => WinColor, false => LossColor, null => MixedColor },
            Thumbnail: mvpIconUrl is null ? null : new DiscordEmbedThumbnail(mvpIconUrl),
            Footer: new DiscordEmbedFooter(LolMessages.EmbedFooter),
            Url: matchUrl));
    }

    private static string Result(MatchReport r) => r.Win switch
    {
        true => "✅ Wygrana",
        false => "❌ Przegrana",
        null => "⚔️ Swój na swojego"
    };

    private static string Description(MatchReport r, string? matchUrl, IReadOnlyDictionary<string, string> names)
    {
        var parts = r.Teams.Select((team, i) => $"**{TeamHeading(r, i)}**\n{string.Join('\n', team.Select(p => Line(r, p, names)))}").ToList();
        if (r.Opponents.Count > 0)
            parts.Add($"**{OpponentsTitle}:** {string.Join(", ", r.Opponents.Select(c => ChampionName(c, names)))}");
        if (matchUrl is not null)
            parts.Add($"[{DetailsText}]({matchUrl})");
        return string.Join("\n\n", parts);
    }

    private static string TeamHeading(MatchReport r, int index) => r.Teams.Count > 1
        ? $"{TeamTitle} {index + 1}"
        : r.Placement is { } place ? $"{OurPairTitle} · {place}. miejsce" : OurTeamTitle;

    // Tracked players in bold; the MVP and the report candidate get their mark at the end of their line
    private static string Line(MatchReport r, MatchReportPlayer p, IReadOnlyDictionary<string, string> names)
    {
        var name = p.IsTracked ? $"**{p.Name}**" : p.Name;
        var role = p.Role is null ? string.Empty : $" ({p.Role})";
        return $"{name} · {ChampionName(p.Champion, names)}{role} · {p.Kills}/{p.Deaths}/{p.Assists}{Mark(r, p)}";
    }

    private static string Mark(MatchReport r, MatchReportPlayer p)
    {
        if (p.Puuid == r.Mvp.Puuid)
            return $" · {MvpMark}";
        if (p.Puuid == r.Feeder.Puuid)
            return $" · {(p.IsTracked ? TrackedFeederMark : FeederMark)}";
        return string.Empty;
    }

    private static string ChampionName(string riotName, IReadOnlyDictionary<string, string> names) =>
        names.GetValueOrDefault(riotName) ?? riotName;
}
