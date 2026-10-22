using System.Globalization;
using System.Text.Json.Serialization;
using Lol.Shared.Riot.Models;

namespace Lol.Shared.Riot;

public class DataDragonCatalog(DataDragonCdn cdn) : IDataDragonCatalog
{
    // Data Dragon's map id of Summoner's Rift; other maps (ARAM, Arena) have their own items and spells
    private const string SummonersRiftMapId = "11";

    // summoner.json "modes" value of a normal Summoner's Rift game
    private const string ClassicMode = "CLASSIC";

    // Keys of the four abilities in the order Data Dragon lists them
    private static readonly string[] AbilityKeys = ["Q", "W", "E", "R"];
    private const string PassiveKey = "P";

    private static readonly StringComparer NameOrder = StringComparer.Create(CultureInfo.GetCultureInfo("pl-PL"), ignoreCase: true);

    public Task<IReadOnlyList<Item>?> ItemsAsync(CancellationToken ct = default) =>
        cdn.GetAsync<ItemListDto, IReadOnlyList<Item>>($"data/{DataDragonCdn.Language}/item.json", (list, version) =>
            list.Data is not { Count: > 0 } data
                ? null
                : data
                    .Where(e => IsInRiftShop(e.Value))
                    // Arena and event copies of an item share its name; keep the regular one (lowest id)
                    .GroupBy(e => e.Value.Name)
                    .Select(g => g.OrderBy(e => e.Key.Length).ThenBy(e => e.Key, StringComparer.Ordinal).First())
                    .Select(e => new Item(e.Key, e.Value.Name, DataDragonText.Plain(e.Value.Description), e.Value.Gold!.Total,
                        e.Value.Tags ?? [], e.Value.Into is not { Count: > 0 }, DataDragonCdn.Url(version, $"img/item/{e.Key}.png")))
                    .OrderBy(i => i.TotalGold)
                    .ThenBy(i => i.Name, NameOrder)
                    .ToList(), ct);

    public Task<IReadOnlyList<RuneTree>?> RuneTreesAsync(CancellationToken ct = default) =>
        cdn.GetAsync<List<RuneTreeDto>, IReadOnlyList<RuneTree>>($"data/{DataDragonCdn.Language}/runesReforged.json", (trees, _) =>
            trees.Count == 0
                ? null
                : trees
                    .Select(t => new RuneTree(t.Id, t.Name, DataDragonCdn.ImageUrl(t.Icon), t.Slots
                        .Select(s => (IReadOnlyList<Rune>)s.Runes
                            .Select(r => new Rune(r.Id, r.Name, DataDragonText.Plain(r.ShortDesc), DataDragonCdn.ImageUrl(r.Icon)))
                            .ToList())
                        .ToList()))
                    .ToList(), ct);

    public Task<IReadOnlyList<SummonerSpell>?> SummonerSpellsAsync(CancellationToken ct = default) =>
        cdn.GetAsync<SummonerListDto, IReadOnlyList<SummonerSpell>>($"data/{DataDragonCdn.Language}/summoner.json", (list, version) =>
            list.Data is not { Count: > 0 } data
                ? null
                : data.Values
                    .Where(s => s.Modes?.Contains(ClassicMode) == true)
                    .Select(s => new SummonerSpell(s.Id, s.Name, DataDragonText.Plain(s.Description),
                        DataDragonCdn.Url(version, $"img/spell/{s.Image.Full}"), int.TryParse(s.Key, out var key) ? key : 0))
                    .OrderBy(s => s.Name, NameOrder)
                    .ToList(), ct);

    public Task<IReadOnlyList<ChampionAbility>?> ChampionAbilitiesAsync(string championId, CancellationToken ct = default) =>
        cdn.GetAsync<ChampionDetailDto, IReadOnlyList<ChampionAbility>>(
            $"data/{DataDragonCdn.Language}/champion/{Uri.EscapeDataString(championId)}.json", (detail, version) =>
            {
                if (detail.Data?.Values.FirstOrDefault() is not { Spells.Count: 4 } champion)
                    return null;

                var passive = new ChampionAbility(PassiveKey, champion.Passive.Name, DataDragonText.Plain(champion.Passive.Description),
                    DataDragonCdn.Url(version, $"img/passive/{champion.Passive.Image.Full}"));
                var spells = champion.Spells.Select((s, i) => new ChampionAbility(AbilityKeys[i], s.Name, DataDragonText.Plain(s.Description),
                    DataDragonCdn.Url(version, $"img/spell/{s.Image.Full}")));
                return [passive, .. spells];
            }, ct);

    private static bool IsInRiftShop(ItemDto item) =>
        item.Maps?.GetValueOrDefault(SummonersRiftMapId) == true
        && item.Gold is { Purchasable: true }
        && item.InStore != false
        && item.RequiredChampion is null
        && item.RequiredAlly is null
        && !string.IsNullOrWhiteSpace(item.Name);

    private sealed record ImageDto(string Full);

    // item.json: { "data": { "1001": { "name", "description", "into", "gold": { "total", "purchasable" }, "tags", "maps", ... } } }
    private sealed record ItemListDto(Dictionary<string, ItemDto>? Data);

    private sealed record ItemDto(
        string Name,
        string? Description,
        List<string>? Into,
        GoldDto? Gold,
        List<string>? Tags,
        Dictionary<string, bool>? Maps,
        bool? InStore,
        string? RequiredChampion,
        string? RequiredAlly);

    private sealed record GoldDto(int Total, bool Purchasable);

    // runesReforged.json: [ { "id", "name", "icon", "slots": [ { "runes": [ { "id", "name", "icon", "shortDesc" } ] } ] } ]
    private sealed record RuneTreeDto(int Id, string Name, string Icon, List<RuneSlotDto> Slots);

    private sealed record RuneSlotDto(List<RuneDto> Runes);

    private sealed record RuneDto(int Id, string Name, string Icon, [property: JsonPropertyName("shortDesc")] string? ShortDesc);

    // summoner.json: { "data": { "SummonerFlash": { "id", "key", "name", "description", "image": { "full" }, "modes" } } }
    private sealed record SummonerListDto(Dictionary<string, SummonerDto>? Data);

    private sealed record SummonerDto(string Id, string? Key, string Name, string? Description, ImageDto Image, List<string>? Modes);

    // champion/Yasuo.json: { "data": { "Yasuo": { "spells": [ 4 × { "name", "description", "image" } ], "passive": { ... } } } }
    private sealed record ChampionDetailDto(Dictionary<string, ChampionDetailEntryDto>? Data);

    private sealed record ChampionDetailEntryDto(List<AbilityDto> Spells, AbilityDto Passive);

    private sealed record AbilityDto(string Name, string? Description, ImageDto Image);
}
