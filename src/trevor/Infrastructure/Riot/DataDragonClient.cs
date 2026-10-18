using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace trevor.Infrastructure.Riot;

// https://developer.riotgames.com/docs/lol#data-dragon
public class DataDragonClient(HttpClient httpClient, TimeProvider time, ILogger<DataDragonClient> logger) : IDataDragon
{
    public const string BaseUrl = "https://ddragon.leagueoflegends.com";

    // Patches ship every two weeks; an icon from a version a few hours old looks the same
    private static readonly TimeSpan VersionTtl = TimeSpan.FromHours(6);

    // Match-V5 championName values whose Data Dragon id is spelled differently
    private static readonly Dictionary<string, string> IconIdByChampionName = new()
    {
        ["FiddleSticks"] = "Fiddlesticks"
    };

    private CachedVersion? _cached;

    public async Task<string?> ChampionIconUrlAsync(string championName, CancellationToken ct = default)
    {
        var version = await LatestVersionAsync(ct);
        if (version is null)
            return null;

        var iconId = IconIdByChampionName.GetValueOrDefault(championName, championName);
        return $"{BaseUrl}/cdn/{version}/img/champion/{Uri.EscapeDataString(iconId)}.png";
    }

    private async Task<string?> LatestVersionAsync(CancellationToken ct)
    {
        var cached = _cached;
        if (cached is not null && time.GetUtcNow() - cached.FetchedAt < VersionTtl)
            return cached.Version;

        try
        {
            var versions = await httpClient.GetFromJsonAsync<List<string>>($"{BaseUrl}/api/versions.json", ct);
            if (versions is { Count: > 0 })
                _cached = cached = new CachedVersion(versions[0], time.GetUtcNow());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not fetch Data Dragon versions");
        }

        // A stale version still gives a working icon URL
        return cached?.Version;
    }

    private sealed record CachedVersion(string Version, DateTimeOffset FetchedAt);
}
