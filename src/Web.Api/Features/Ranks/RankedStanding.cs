namespace Web.Api.Features.Ranks;

// One ranked queue of a player, ready to show: Queue "Solo/Duo" or "Flex", Label "Gold II"
public record RankedStanding(string Queue, string Tier, string Label, int LeaguePoints, int Wins, int Losses, string EmblemUrl);
