namespace Lol.Shared.Riot;

// When a match counts as a real game, the same for the bot and the website
public static class MatchRules
{
    // Riot counts games under ~3.5 min as remakes; 5 min keeps early surrenders out too
    public const int RemakeSeconds = 300;

    public static bool IsRemake(long durationSeconds) => durationSeconds < RemakeSeconds;
}
