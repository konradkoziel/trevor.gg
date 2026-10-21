using Web.Api.Features.Builds;

namespace Web.Api.Tests;

// The same rules as src/web/src/features/builds/skillOrder.test.ts
public class SkillOrderRulesTests
{
    private static string?[] Order(string keys) =>
        [.. keys.PadRight(SkillOrderRules.MaxLevel, '-').Select(c => c == '-' ? null : (string?)c.ToString())];

    [Theory]
    [InlineData("QEWQQRQEQEREEWWRWW")]
    [InlineData("")]
    [InlineData("QWE--R")]
    public void Valid(string keys) => Assert.True(SkillOrderRules.IsValid(Order(keys)));

    [Theory]
    [InlineData("QWEQR")] // R before level 6
    [InlineData("QWEQQRR")] // second R before 11
    [InlineData("QQ")] // rank 2 at level 2
    [InlineData("QWQEQRQWQQ")] // rank 6
    [InlineData("QWX")] // not an ability
    public void Invalid(string keys) => Assert.False(SkillOrderRules.IsValid(Order(keys)));

    [Fact]
    public void MustCoverEighteenLevels() => Assert.False(SkillOrderRules.IsValid(["Q"]));
}
