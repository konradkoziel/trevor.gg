using trevor.Features.Lol.Common;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.Web;

namespace trevor.Features.Lol.Builds;

public static class BuildMessage
{
    // Gold, like the website's accent colour
    public const int Color = 0xC8AA6E;

    public const string ItemsTitle = "Itemy";
    public const string RunesTitle = "Runy";
    public const string SpellsTitle = "Czary";
    public const string SkillsTitle = "Skille";

    public const string SiteNotConnected = "Strona z buildami nie jest podpięta. Ktoś musi ją dokończyć.";
    public const string MissingChampion = "Podaj championa, np. `/build champion: Yasuo`.";

    // Shown for an empty part of the build
    private const string Nothing = "—";

    public static string UnknownChampion(string champion) =>
        $"Nie znam championa **{champion}**. Albo strona śpi, albo Ty wymyślasz postacie.";

    public static string NoBuild(string champion, string? siteUrl) => siteUrl is null
        ? $"Nie masz buildu na **{champion}**. Zrób go na stronie."
        : $"Nie masz buildu na **{champion}**. Zrób go na stronie, zanim znowu zagrasz na czuja: {siteUrl.TrimEnd('/')}";

    public static DiscordEmbed Card(WebBuildCard card, int moreBuilds) => new(
        Title: $"🛠️ {card.Name}",
        Description: $"{card.ChampionName} · build gracza {card.OwnerName}\n[Otwórz na stronie]({card.Url})",
        Color: Color,
        Fields:
        [
            new DiscordEmbedField(ItemsTitle, Join(card.Items, " → ")),
            new DiscordEmbedField(RunesTitle, Runes(card), Inline: true),
            new DiscordEmbedField(SpellsTitle, Join(card.SummonerSpells, " + "), Inline: true),
            new DiscordEmbedField(SkillsTitle, Join(card.SkillMaxOrder, " > "), Inline: true),
        ],
        Thumbnail: card.ChampionIconUrl is null ? null : new DiscordEmbedThumbnail(card.ChampionIconUrl),
        Footer: new DiscordEmbedFooter(moreBuilds > 0 ? $"{LolMessages.EmbedFooter} · i {moreBuilds} starsze na stronie" : LolMessages.EmbedFooter));

    // Keystone in bold, then the tree names and the other runes
    private static string Runes(WebBuildCard card)
    {
        var lines = new List<string>();
        if (card.Keystone is not null)
            lines.Add($"**{card.Keystone}**");
        if (card.RuneTrees.Count > 0)
            lines.Add(string.Join(" / ", card.RuneTrees));
        lines.AddRange(card.Runes);
        return lines.Count == 0 ? Nothing : string.Join("\n", lines);
    }

    private static string Join(IReadOnlyList<string> parts, string separator) => parts.Count == 0 ? Nothing : string.Join(separator, parts);
}
