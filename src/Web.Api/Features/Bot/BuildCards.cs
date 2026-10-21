using Lol.Shared.Riot.Models;
using Web.Api.Features.Builds;

namespace Web.Api.Features.Bot;

// Turns the ids of a stored build into the names of the current patch; ids that vanished are skipped
public static class BuildCards
{
    public static BuildCard Create(StoredBuild stored, Champion champion, BuildData data, string siteUrl)
    {
        var build = stored.Build;
        var items = data.Items.ToDictionary(i => i.Id, i => i.Name);
        var runes = data.RuneTrees.SelectMany(t => t.Slots).SelectMany(s => s).ToDictionary(r => r.Id, r => r.Name);
        var trees = data.RuneTrees.ToDictionary(t => t.Id, t => t.Name);
        var spells = data.SummonerSpells.ToDictionary(s => s.Id, s => s.Name);

        var keystone = build.Runes.Primary.FirstOrDefault() is { } keystoneId ? runes.GetValueOrDefault(keystoneId) : null;
        var minorRunes = build.Runes.Primary.Skip(1).Concat(build.Runes.Secondary.Select(r => (int?)r.RuneId))
            .Select(id => id is { } value ? runes.GetValueOrDefault(value) : null);
        var treeNames = new[] { build.Runes.PrimaryTreeId, build.Runes.SecondaryTreeId }
            .Select(id => id is { } value ? trees.GetValueOrDefault(value) : null);

        return new BuildCard(
            build.Name,
            champion.Name,
            champion.IconUrl,
            stored.OwnerName,
            $"{siteUrl.TrimEnd('/')}/b/{Uri.EscapeDataString(stored.Id)}",
            NamesOf(build.Items.Select(items.GetValueOrDefault)),
            keystone,
            NamesOf(minorRunes),
            NamesOf(treeNames),
            NamesOf(build.SummonerSpells.Select(spells.GetValueOrDefault)),
            SkillOrderRules.MaxOrder(build.SkillOrder));
    }

    private static IReadOnlyList<string> NamesOf(IEnumerable<string?> names) => names.OfType<string>().ToList();
}
