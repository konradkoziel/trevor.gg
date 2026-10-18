namespace trevor.Infrastructure.Riot.Models;

// League-V4 LeagueEntryDTO (only the fields we use)
public record LeagueEntry(string QueueType, string Tier, string Rank, int LeaguePoints, int Wins, int Losses)
{
    public const string SoloQueue = "RANKED_SOLO_5x5";
    public const string FlexQueue = "RANKED_FLEX_SR";
}
