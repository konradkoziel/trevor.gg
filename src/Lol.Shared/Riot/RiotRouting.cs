namespace Lol.Shared.Riot;

// https://developer.riotgames.com/docs/lol#routing-values
public static class RiotRouting
{
    private static readonly Dictionary<string, string> RegionByPlatform = new(StringComparer.OrdinalIgnoreCase)
    {
        ["eun1"] = "europe", ["euw1"] = "europe", ["tr1"] = "europe", ["ru"] = "europe", ["me1"] = "europe",
        ["na1"] = "americas", ["br1"] = "americas", ["la1"] = "americas", ["la2"] = "americas",
        ["kr"] = "asia", ["jp1"] = "asia",
        ["oc1"] = "sea", ["sg2"] = "sea", ["tw2"] = "sea", ["vn2"] = "sea"
    };

    public static bool IsKnownPlatform(string platform) => RegionByPlatform.ContainsKey(platform);

    // Match-V5 has its own sea cluster
    public static string MatchRegion(string platform) =>
        RegionByPlatform.TryGetValue(platform, out var region)
            ? region
            : throw new ArgumentException($"Unknown Riot platform '{platform}'", nameof(platform));

    // Account-V1 only lives in americas/asia/europe; SEA accounts are served from asia
    public static string AccountRegion(string platform) =>
        RegionByPlatform.TryGetValue(platform, out var region)
            ? region == "sea" ? "asia" : region
            : throw new ArgumentException($"Unknown Riot platform '{platform}'", nameof(platform));
}
