using Lol.Shared.Riot;

namespace Lol.Shared.Tests;

public class DataDragonCatalogTests
{
    private const string Cdn = "https://ddragon.leagueoflegends.com/cdn/16.20.1";

    private const string Items = """
        {
          "data": {
            "1001": { "name": "Buty", "description": "<mainText><stats><attention>25</attention> prędkości ruchu</stats></mainText>",
                      "into": ["3006"], "gold": { "total": 300, "purchasable": true }, "tags": ["Boots"], "maps": { "11": true } },
            "3006": { "name": "Nagolenniki Berserkera", "description": "", "gold": { "total": 1100, "purchasable": true },
                      "tags": ["Boots", "AttackSpeed"], "maps": { "11": true } },
            "223006": { "name": "Nagolenniki Berserkera", "gold": { "total": 1100, "purchasable": true }, "maps": { "11": true } },
            "3600": { "name": "Tylko na Arenie", "gold": { "total": 500, "purchasable": true }, "maps": { "11": false, "30": true } },
            "3400": { "name": "Nagroda z wydarzenia", "gold": { "total": 0, "purchasable": false }, "maps": { "11": true } },
            "3599": { "name": "Czarna Włócznia Kalisty", "gold": { "total": 0, "purchasable": true }, "maps": { "11": true },
                      "requiredChampion": "Kalista" },
            "3513": { "name": "Ukryty", "gold": { "total": 0, "purchasable": true }, "maps": { "11": true }, "inStore": false }
          }
        }
        """;

    [Fact]
    public async Task Items_AreTheRiftShop_CheapestFirst_WithoutDuplicates()
    {
        var stub = new StubDataDragon().With("data/pl_PL/item.json", Items);

        var items = (await new DataDragonCatalog(stub.Cdn()).ItemsAsync())!;

        Assert.Equal(["1001", "3006"], items.Select(i => i.Id));
        var boots = items[0];
        Assert.Equal("Buty", boots.Name);
        Assert.Equal("25 prędkości ruchu", boots.Description);
        Assert.Equal(300, boots.TotalGold);
        Assert.False(boots.IsFinished);
        Assert.True(items[1].IsFinished);
        Assert.Equal(["Boots", "AttackSpeed"], items[1].Tags);
        Assert.Equal($"{Cdn}/img/item/3006.png", items[1].IconUrl);
    }

    [Fact]
    public async Task RuneTrees_KeepTheirSlots_WithUnversionedIcons()
    {
        var stub = new StubDataDragon().With("data/pl_PL/runesReforged.json", """
            [ { "id": 8000, "key": "Precision", "name": "Precyzja", "icon": "perk-images/Styles/7201_Precision.png",
                "slots": [
                  { "runes": [ { "id": 8005, "name": "Atak Wyprzedzający", "icon": "perk-images/Styles/Precision/PressTheAttack/PressTheAttack.png",
                                 "shortDesc": "Trzy ataki <b>pod rząd</b>" } ] },
                  { "runes": [ { "id": 9101, "name": "Absorpcja Życia", "icon": "perk-images/x.png", "shortDesc": "" } ] }
                ] } ]
            """);

        var tree = Assert.Single((await new DataDragonCatalog(stub.Cdn()).RuneTreesAsync())!);

        Assert.Equal("Precyzja", tree.Name);
        Assert.Equal("https://ddragon.leagueoflegends.com/cdn/img/perk-images/Styles/7201_Precision.png", tree.IconUrl);
        Assert.Equal(2, tree.Slots.Count);
        var keystone = Assert.Single(tree.Slots[0]);
        Assert.Equal(8005, keystone.Id);
        Assert.Equal("Trzy ataki pod rząd", keystone.Description);
    }

    [Fact]
    public async Task SummonerSpells_AreOnlyTheRiftOnes_ByName()
    {
        var stub = new StubDataDragon().With("data/pl_PL/summoner.json", """
            { "data": {
                "SummonerSnowball": { "id": "SummonerSnowball", "name": "Śnieżka", "image": { "full": "SummonerSnowball.png" }, "modes": ["ARAM"] },
                "SummonerFlash": { "id": "SummonerFlash", "name": "Błysk", "description": "Teleportuje", "image": { "full": "SummonerFlash.png" }, "modes": ["CLASSIC", "ARAM"] },
                "SummonerDot": { "id": "SummonerDot", "name": "Podpalenie", "image": { "full": "SummonerDot.png" }, "modes": ["CLASSIC"] }
            } }
            """);

        var spells = (await new DataDragonCatalog(stub.Cdn()).SummonerSpellsAsync())!;

        Assert.Equal(["Błysk", "Podpalenie"], spells.Select(s => s.Name));
        Assert.Equal($"{Cdn}/img/spell/SummonerFlash.png", spells[0].IconUrl);
    }

    [Fact]
    public async Task ChampionAbilities_ArePassiveThenQwer()
    {
        var stub = new StubDataDragon().With("data/pl_PL/champion/Yasuo.json", """
            { "data": { "Yasuo": {
                "passive": { "name": "Droga Wędrowca", "description": "Tarcza", "image": { "full": "Yasuo_Passive.png" } },
                "spells": [
                  { "name": "Stalowa Nawałnica", "description": "Pchnięcie", "image": { "full": "YasuoQ1Wrapper.png" } },
                  { "name": "Ściana Wiatru", "description": "", "image": { "full": "YasuoW.png" } },
                  { "name": "Cięcie Ostrza", "description": "", "image": { "full": "YasuoE.png" } },
                  { "name": "Ostatnie Tchnienie", "description": "", "image": { "full": "YasuoR.png" } }
                ] } } }
            """);

        var abilities = (await new DataDragonCatalog(stub.Cdn()).ChampionAbilitiesAsync("Yasuo"))!;

        Assert.Equal(["P", "Q", "W", "E", "R"], abilities.Select(a => a.Key));
        Assert.Equal($"{Cdn}/img/passive/Yasuo_Passive.png", abilities[0].IconUrl);
        Assert.Equal($"{Cdn}/img/spell/YasuoQ1Wrapper.png", abilities[1].IconUrl);
        Assert.Equal("Stalowa Nawałnica", abilities[1].Name);
    }

    [Fact]
    public async Task UnknownChampion_IsNull()
    {
        Assert.Null(await new DataDragonCatalog(new StubDataDragon().Cdn()).ChampionAbilitiesAsync("Nobody"));
    }

    [Fact]
    public async Task EachFile_IsDownloadedOncePerPatch()
    {
        var stub = new StubDataDragon().With("data/pl_PL/item.json", Items);
        var catalog = new DataDragonCatalog(stub.Cdn());

        await catalog.ItemsAsync();
        await catalog.ItemsAsync();

        Assert.Single(stub.Requests, $"{Cdn}/data/pl_PL/item.json");
    }
}
