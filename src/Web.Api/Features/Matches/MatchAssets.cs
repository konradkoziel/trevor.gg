using Lol.Shared.Riot.Models;

namespace Web.Api.Features.Matches;

// Names and icons of the patch, looked up by the ids Match-V5 uses; whatever is missing is simply not shown
public record MatchAssets(
    IReadOnlyDictionary<string, string?> ChampionIcons,
    IReadOnlyDictionary<int, MatchIcon> Items,
    IReadOnlyDictionary<int, SummonerSpell> Spells,
    IReadOnlyDictionary<int, Rune> Runes,
    IReadOnlyDictionary<int, RuneTree> Trees);
