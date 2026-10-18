namespace trevor.Infrastructure.Riot;

// Human-readable names for Riot's ids and enums
public static class RiotNames
{
    // Match-V5 teamPosition; null when Riot doesn't assign one (ARAM, Arena)
    public static string? Role(string? teamPosition) => teamPosition switch
    {
        "TOP" => "Top",
        "JUNGLE" => "Jungle",
        "MIDDLE" => "Mid",
        "BOTTOM" => "ADC",
        "UTILITY" => "Support",
        _ => null
    };

    // https://static.developer.riotgames.com/docs/lol/queues.json
    public static string Queue(int queueId, string gameMode) => queueId switch
    {
        420 => "Ranked Solo/Duo",
        440 => "Ranked Flex",
        400 => "Normal Draft",
        430 => "Normal Blind",
        490 => "Quickplay",
        450 => "ARAM",
        1700 or 1710 => "Arena",
        0 => "Custom",
        _ => gameMode
    };
}
