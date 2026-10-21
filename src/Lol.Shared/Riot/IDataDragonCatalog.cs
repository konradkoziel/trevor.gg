using Lol.Shared.Riot.Models;

namespace Lol.Shared.Riot;

// What a build is made of, for the latest patch, in Polish. Every method returns null when Data Dragon can't be reached.
public interface IDataDragonCatalog
{
    // Summoner's Rift shop only, cheapest first
    Task<IReadOnlyList<Item>?> ItemsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<RuneTree>?> RuneTreesAsync(CancellationToken ct = default);

    // Spells available on Summoner's Rift, by name
    Task<IReadOnlyList<SummonerSpell>?> SummonerSpellsAsync(CancellationToken ct = default);

    // Passive, Q, W, E, R; championId as in Champion.Id ("MonkeyKing")
    Task<IReadOnlyList<ChampionAbility>?> ChampionAbilitiesAsync(string championId, CancellationToken ct = default);
}
