namespace Web.Api.Features.Builds;

// What the editor sends and gets back; mirrors Build in src/web/src/features/builds/build.ts.
// SkillOrder: 18 entries, "Q"/"W"/"E"/"R" or null, index 0 = level 1
public record SavedBuild(
    string Name,
    string ChampionId,
    IReadOnlyList<string> Items,
    BuildRunes Runes,
    IReadOnlyList<string> SummonerSpells,
    IReadOnlyList<string?> SkillOrder);
