namespace Web.Api.Features.Stats;

// How many recent games "Statystyki" shows and storage keeps per player. App setting: MATCH_HISTORY_GAMES
// (from the GitHub variable of the same name via infra/web.bicep); older games are deleted
public class MatchHistoryOptions
{
    // Enough for a trend, and at most 21 Riot calls on a first visit (the key allows 100 per 2 minutes)
    public const int DefaultGames = 20;

    // Match-V5 returns at most 100 ids per request
    public const int MaxGames = 100;

    public int Games { get; set; } = DefaultGames;

    public bool IsValid => Games is >= 1 and <= MaxGames;
}
