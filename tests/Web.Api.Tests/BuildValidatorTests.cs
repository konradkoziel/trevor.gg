using Web.Api.Features.Builds;

namespace Web.Api.Tests;

public class BuildValidatorTests
{
    private static async Task<string?> ErrorOf(SavedBuild build) => (await TestBuilds.Validator().ValidateAsync(build)).Error;

    [Fact]
    public async Task ValidBuild_Passes() => Assert.Equal(BuildValidation.Ok, await TestBuilds.Validator().ValidateAsync(TestBuilds.Valid()));

    [Fact]
    public async Task UnfinishedBuild_Passes()
    {
        var build = TestBuilds.Valid() with
        {
            Items = [],
            SummonerSpells = [],
            Runes = new BuildRunes(null, [null, null, null, null], null, []),
            SkillOrder = new string?[18],
        };

        Assert.Null(await ErrorOf(build));
    }

    [Theory]
    [InlineData("   ", BuildMessages.NameMissing)]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901", BuildMessages.NameTooLong)]
    public async Task Name_IsRequired_AndShort(string name, string error) =>
        Assert.Equal(error, await ErrorOf(TestBuilds.Valid() with { Name = name }));

    [Fact]
    public async Task UnknownChampion_IsRejected() =>
        Assert.Equal(BuildMessages.UnknownChampion, await ErrorOf(TestBuilds.Valid() with { ChampionId = "Nobody" }));

    [Fact]
    public async Task Items_MustExist_AndFitSixSlots()
    {
        Assert.Equal(BuildMessages.BadItems, await ErrorOf(TestBuilds.Valid() with { Items = ["9999"] }));
        Assert.Equal(BuildMessages.BadItems, await ErrorOf(TestBuilds.Valid() with { Items = [.. Enumerable.Repeat("1055", 7)] }));
        Assert.Null(await ErrorOf(TestBuilds.Valid() with { Items = ["1055", "1055"] }));
    }

    [Fact]
    public async Task SummonerSpells_AreTwoDifferentOnes() =>
        Assert.Equal(BuildMessages.BadSummonerSpells, await ErrorOf(TestBuilds.Valid() with { SummonerSpells = ["SummonerFlash", "SummonerFlash"] }));

    public static TheoryData<BuildRunes> BadRunePages => new()
    {
        // Keystone of another tree
        new BuildRunes(8000, [8437, null, null, null], null, []),
        // Rune in the wrong row
        new BuildRunes(8000, [null, 9104, null, null], null, []),
        // Secondary tree = primary tree
        new BuildRunes(8000, [null, null, null, null], 8000, []),
        // Keystone from the secondary tree
        new BuildRunes(8000, [null, null, null, null], 8400, [new SecondaryRune(0, 8437)]),
        // Two secondary runes from one row
        new BuildRunes(8000, [null, null, null, null], 8400, [new SecondaryRune(1, 8446), new SecondaryRune(1, 8463)]),
        // Three secondary runes
        new BuildRunes(8000, [null, null, null, null], 8400,
            [new SecondaryRune(1, 8446), new SecondaryRune(2, 8429), new SecondaryRune(3, 8451)]),
        // Secondary runes without a secondary tree
        new BuildRunes(8000, [null, null, null, null], null, [new SecondaryRune(1, 8446)]),
        // Unknown tree
        new BuildRunes(1234, [null, null, null, null], null, []),
    };

    [Theory]
    [MemberData(nameof(BadRunePages))]
    public async Task RunesBreakingTheRules_AreRejected(BuildRunes runes) =>
        Assert.Equal(BuildMessages.BadRunes, await ErrorOf(TestBuilds.Valid() with { Runes = runes }));

    [Fact]
    public async Task ImpossibleSkillOrder_IsRejected() =>
        Assert.Equal(BuildMessages.BadSkillOrder, await ErrorOf(TestBuilds.Valid() with { SkillOrder = [.. Enumerable.Repeat((string?)"R", 18)] }));

    [Fact]
    public async Task DataDragonDown_IsUnavailable()
    {
        var result = await TestBuilds.Validator(new FakeCatalog { Items = null }).ValidateAsync(TestBuilds.Valid());

        Assert.True(result.DataUnavailable);
    }
}
