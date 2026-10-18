using trevor.Infrastructure.Riot;
using trevor.Infrastructure.Riot.Models;

namespace trevor.Features.Lol.Rank;

public record PlayerRanks(LeagueEntry? Solo, LeagueEntry? Flex)
{
    public bool IsUnranked => Solo is null && Flex is null;

    // The queue the verdict is about: Solo/Duo first, Flex otherwise
    public LeagueEntry? Main => Solo ?? Flex;

    public static PlayerRanks From(IReadOnlyList<LeagueEntry> entries) => new(
        entries.FirstOrDefault(e => e.QueueType == LeagueEntry.SoloQueue),
        entries.FirstOrDefault(e => e.QueueType == LeagueEntry.FlexQueue));
}
