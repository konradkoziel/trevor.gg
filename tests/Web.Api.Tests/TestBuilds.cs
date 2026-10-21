using Lol.Shared.Riot.Models;
using Web.Api.Features.Builds;

namespace Web.Api.Tests;

// A small but real-looking patch: two rune trees, a few items and spells, and a valid build made of them
public static class TestBuilds
{
    public static readonly Champion[] Champions = [new("Yasuo", "Yasuo", "Pokutnik", "")];

    private static Rune R(int id) => new(id, $"Rune {id}", "", "");

    public static readonly RuneTree Precision = new(8000, "Precyzja", "", [[R(8005), R(8008)], [R(9101), R(9111)], [R(9104), R(9105)], [R(8014), R(8017)]]);
    public static readonly RuneTree Resolve = new(8400, "Determinacja", "", [[R(8437)], [R(8446), R(8463)], [R(8429), R(8444)], [R(8451), R(8453)]]);

    public static FakeCatalog Catalog() => new()
    {
        Items =
        [
            new("1055", "Ostrze Dorana", "", 450, [], true, ""),
            new("3006", "Nagolenniki Berserkera", "", 1100, ["Boots"], true, ""),
            new("3031", "Nieskończona Krawędź", "", 3450, [], true, ""),
        ],
        RuneTrees = [Precision, Resolve],
        SummonerSpells = [new("SummonerFlash", "Błysk", "", ""), new("SummonerDot", "Podpalenie", "", "")],
    };

    public static BuildValidator Validator(FakeCatalog? catalog = null) => new(new FakeDataDragon(Champions), catalog ?? Catalog());

    public static readonly string?[] QMax = [.. "QEWQQRQEQEREEWWRWW".Select(c => (string?)c.ToString())];

    public static SavedBuild Valid() => new(
        "Yasuo, który boli",
        "Yasuo",
        ["1055", "3006", "3031"],
        new BuildRunes(8000, [8005, 9101, 9104, 8014], 8400, [new SecondaryRune(1, 8446), new SecondaryRune(3, 8451)]),
        ["SummonerFlash", "SummonerDot"],
        QMax);
}
