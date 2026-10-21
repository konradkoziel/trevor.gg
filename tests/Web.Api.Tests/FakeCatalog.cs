using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Web.Api.Tests;

// Each list = null plays Data Dragon failing for that file
public sealed class FakeCatalog : IDataDragonCatalog
{
    public IReadOnlyList<Item>? Items { get; init; } = [new("3006", "Nagolenniki Berserkera", "", 1100, ["Boots"], true, "https://icons.test/3006.png")];
    public IReadOnlyList<RuneTree>? RuneTrees { get; init; } = [new(8000, "Precyzja", "https://icons.test/p.png", [])];
    public IReadOnlyList<SummonerSpell>? SummonerSpells { get; init; } = [new("SummonerFlash", "Błysk", "", "https://icons.test/f.png")];
    public IReadOnlyList<ChampionAbility>? Abilities { get; init; } = [new("Q", "Stalowa Nawałnica", "", "https://icons.test/q.png")];

    public string? RequestedChampion { get; private set; }

    public Task<IReadOnlyList<Item>?> ItemsAsync(CancellationToken ct = default) => Task.FromResult(Items);

    public Task<IReadOnlyList<RuneTree>?> RuneTreesAsync(CancellationToken ct = default) => Task.FromResult(RuneTrees);

    public Task<IReadOnlyList<SummonerSpell>?> SummonerSpellsAsync(CancellationToken ct = default) => Task.FromResult(SummonerSpells);

    public Task<IReadOnlyList<ChampionAbility>?> ChampionAbilitiesAsync(string championId, CancellationToken ct = default)
    {
        RequestedChampion = championId;
        return Task.FromResult(Abilities);
    }
}
