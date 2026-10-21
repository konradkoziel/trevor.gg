using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Web.Api.Features.Builds;

// Every id in a build must exist in the current patch and the runes and skills must follow the game's rules
public class BuildValidator(IDataDragon dataDragon, IDataDragonCatalog catalog)
{
    public const int MaxNameLength = 60;
    public const int MaxItems = 6;
    public const int MaxSummonerSpells = 2;

    // Index 0 of RuneTree.Slots and of BuildRunes.Primary
    private const int KeystoneRow = 0;
    private const int PrimaryRuneCount = 4;
    private const int MaxSecondaryRunes = 2;

    public async Task<BuildValidation> ValidateAsync(SavedBuild build, CancellationToken ct = default)
    {
        var name = build.Name?.Trim() ?? string.Empty;
        if (name.Length == 0)
            return BuildValidation.Invalid(BuildMessages.NameMissing);
        if (name.Length > MaxNameLength)
            return BuildValidation.Invalid(BuildMessages.NameTooLong);
        if (build.SkillOrder is null || !SkillOrderRules.IsValid(build.SkillOrder))
            return BuildValidation.Invalid(BuildMessages.BadSkillOrder);

        var champions = await dataDragon.ChampionsAsync(ct);
        var items = await catalog.ItemsAsync(ct);
        var trees = await catalog.RuneTreesAsync(ct);
        var spells = await catalog.SummonerSpellsAsync(ct);
        if (champions is null || items is null || trees is null || spells is null)
            return BuildValidation.Unavailable;

        if (!champions.Any(c => c.Id == build.ChampionId))
            return BuildValidation.Invalid(BuildMessages.UnknownChampion);
        if (!AreItemsValid(build.Items, items))
            return BuildValidation.Invalid(BuildMessages.BadItems);
        if (!AreSummonerSpellsValid(build.SummonerSpells, spells))
            return BuildValidation.Invalid(BuildMessages.BadSummonerSpells);
        if (!AreRunesValid(build.Runes, trees))
            return BuildValidation.Invalid(BuildMessages.BadRunes);

        return BuildValidation.Ok;
    }

    private static bool AreItemsValid(IReadOnlyList<string>? chosen, IReadOnlyList<Item> items) =>
        chosen is not null && chosen.Count <= MaxItems && chosen.All(id => items.Any(i => i.Id == id));

    private static bool AreSummonerSpellsValid(IReadOnlyList<string>? chosen, IReadOnlyList<SummonerSpell> spells) =>
        chosen is not null && chosen.Count <= MaxSummonerSpells && chosen.Distinct().Count() == chosen.Count
        && chosen.All(id => spells.Any(s => s.Id == id));

    // An unfinished page is fine; a rune that isn't where the page says it is, is not
    private static bool AreRunesValid(BuildRunes? runes, IReadOnlyList<RuneTree> trees)
    {
        if (runes?.Primary is not { Count: PrimaryRuneCount } primary || runes.Secondary is null)
            return false;

        var primaryTree = trees.FirstOrDefault(t => t.Id == runes.PrimaryTreeId);
        if (runes.PrimaryTreeId is not null && primaryTree is null)
            return false;
        for (var row = 0; row < primary.Count; row++)
        {
            if (primary[row] is not { } runeId)
                continue;
            if (primaryTree is null || !IsInRow(primaryTree, row, runeId))
                return false;
        }

        if (runes.SecondaryTreeId is null)
            return runes.Secondary.Count == 0;
        var secondaryTree = trees.FirstOrDefault(t => t.Id == runes.SecondaryTreeId);
        if (secondaryTree is null || secondaryTree.Id == runes.PrimaryTreeId)
            return false;

        var secondary = runes.Secondary;
        return secondary.Count <= MaxSecondaryRunes
            && secondary.Select(r => r.Row).Distinct().Count() == secondary.Count
            && secondary.All(r => r.Row != KeystoneRow && IsInRow(secondaryTree, r.Row, r.RuneId));
    }

    private static bool IsInRow(RuneTree tree, int row, int runeId) =>
        row >= 0 && row < tree.Slots.Count && tree.Slots[row].Any(r => r.Id == runeId);
}
