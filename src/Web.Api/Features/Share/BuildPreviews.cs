using Lol.Shared.Riot.Models;
using Web.Api.Features.Bot;
using Web.Api.Features.Builds;

namespace Web.Api.Features.Share;

// The preview of a shared build: what it is, who made it and the gist of it in one description
public static class BuildPreviews
{
    // Used when Data Dragon is down and the ids can't be turned into names
    public const string FallbackDescription = "Build z Trevor.gg. Kliknij, zanim Trevor zmieni zdanie.";

    public static LinkPreview Create(StoredBuild stored, Champion? champion, BuildData? data, string siteUrl)
    {
        if (champion is null || data is null)
            return new LinkPreview(stored.Build.Name, FallbackDescription, null, BuildCards.Url(siteUrl, stored.Id));

        var card = BuildCards.Create(stored, champion, data, siteUrl);
        return new LinkPreview($"{card.Name} · {card.ChampionName}", Describe(card), ImageOf(champion), card.Url);
    }

    // The splash art fills Discord's large preview; the icon is the fallback for a champion without one
    private static string ImageOf(Champion champion) => champion.SplashUrl is "" ? champion.IconUrl : champion.SplashUrl;

    private static string Describe(BuildCard card)
    {
        var parts = new List<string> { $"Build od {card.OwnerName}." };
        if (card.Items.Count > 0)
            parts.Add($"Itemy: {string.Join(" → ", card.Items)}.");
        if (card.Keystone is not null)
            parts.Add($"Runy: {card.Keystone} ({string.Join(" + ", card.RuneTrees)}).");
        if (card.SummonerSpells.Count > 0)
            parts.Add($"Spelle: {string.Join(", ", card.SummonerSpells)}.");
        if (card.SkillMaxOrder.Count > 0)
            parts.Add($"Max: {string.Join(" > ", card.SkillMaxOrder)}.");
        return string.Join(" ", parts);
    }
}
