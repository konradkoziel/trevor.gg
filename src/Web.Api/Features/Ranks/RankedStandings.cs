using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Web.Api.Features.Ranks;

// League-V4 entries → Solo/Duo first, then Flex; other queues (e.g. Arena, TFT) are left out
public static class RankedStandings
{
    public const string SoloQueueName = "Solo/Duo";
    public const string FlexQueueName = "Flex";

    public static IReadOnlyList<RankedStanding> From(IReadOnlyList<LeagueEntry> entries) =>
    [
        .. Standing(entries, LeagueEntry.SoloQueue, SoloQueueName),
        .. Standing(entries, LeagueEntry.FlexQueue, FlexQueueName),
    ];

    private static IEnumerable<RankedStanding> Standing(IReadOnlyList<LeagueEntry> entries, string queueType, string queueName) =>
        entries.Where(e => e.QueueType == queueType).Take(1).Select(e =>
            new RankedStanding(queueName, e.Tier, RankNames.Label(e), e.LeaguePoints, e.Wins, e.Losses, CommunityDragon.RankEmblemUrl(e.Tier)));
}
