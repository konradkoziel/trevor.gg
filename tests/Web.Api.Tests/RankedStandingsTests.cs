using Lol.Shared.Riot.Models;
using Web.Api.Features.Ranks;

namespace Web.Api.Tests;

public class RankedStandingsTests
{
    [Fact]
    public void SoloComesFirst_ThenFlex_OtherQueuesDropped()
    {
        LeagueEntry[] entries =
        [
            new(LeagueEntry.FlexQueue, "SILVER", "I", 12, 10, 10),
            new("CHERRY", "GOLD", "I", 0, 1, 1),
            new(LeagueEntry.SoloQueue, "GOLD", "II", 47, 58, 61),
        ];

        var standings = RankedStandings.From(entries);

        Assert.Equal(["Solo/Duo", "Flex"], standings.Select(s => s.Queue));
        Assert.Equal("Gold II", standings[0].Label);
        Assert.Equal(47, standings[0].LeaguePoints);
        Assert.EndsWith("/emblem-gold.png", standings[0].EmblemUrl);
    }

    [Fact]
    public void Unranked_IsEmpty() => Assert.Empty(RankedStandings.From([]));
}
