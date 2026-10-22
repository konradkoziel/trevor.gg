using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Lol.Shared.Tests;

public class RankNamesTests
{
    [Theory]
    [InlineData("GOLD", "II", "Gold II")]
    [InlineData("EMERALD", "IV", "Emerald IV")]
    [InlineData("MASTER", "I", "Master")]
    [InlineData("CHALLENGER", "I", "Challenger")]
    public void Label_ReadsLikeTheClient(string tier, string division, string expected) =>
        Assert.Equal(expected, RankNames.Label(new LeagueEntry(LeagueEntry.SoloQueue, tier, division, 0, 0, 0)));
}
