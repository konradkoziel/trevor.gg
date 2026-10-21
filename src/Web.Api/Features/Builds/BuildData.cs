using Lol.Shared.Riot.Models;

namespace Web.Api.Features.Builds;

// Everything the build editor picks from, in one response
public record BuildData(IReadOnlyList<Item> Items, IReadOnlyList<RuneTree> RuneTrees, IReadOnlyList<SummonerSpell> SummonerSpells);
