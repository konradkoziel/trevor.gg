using System.Globalization;
using System.Net.Http.Json;
using Lol.Shared.Riot.Models;
using Microsoft.Extensions.Logging;

namespace Lol.Shared.Riot;

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

    // Champion names and titles as the Polish client shows them
    private const string Language = "pl_PL";

    // Sorted like the Polish client sorts them (e.g. Ł after L)
    private static readonly StringComparer NameOrder = StringComparer.Create(CultureInfo.GetCultureInfo("pl-PL"), ignoreCase: true);

    private CachedVersion? _cached;
    private CachedChampions? _champions;

    public async Task<string?> ChampionIconUrlAsync(string championName, CancellationToken ct = default)
    {
        var version = await LatestVersionAsync(ct);
        if (version is null)
            return null;

        return IconUrl(version, IconIdByChampionName.GetValueOrDefault(championName, championName));
    }

    public async Task<IReadOnlyList<Champion>?> ChampionsAsync(CancellationToken ct = default)
    {
        var version = await LatestVersionAsync(ct);
        if (version is null)
            return null;

        // The list only changes with a new patch, so it is cached for as long as the version is
        var cached = _champions;
        if (cached?.Version == version)
            return cached.Champions;

        try
        {
            var list = await httpClient.GetFromJsonAsync<ChampionListDto>($"{BaseUrl}/cdn/{version}/data/{Language}/champion.json", ct);
            if (list?.Data is not { Count: > 0 } data)
                return cached?.Champions;

            var champions = data.Values
                .Select(c => new Champion(c.Id, c.Name, c.Title, IconUrl(version, c.Id)))
                .OrderBy(c => c.Name, NameOrder)
                .ToList();
            _champions = new CachedChampions(version, champions);
            return champions;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not fetch the Data Dragon champion list");
            // The previous patch's list is still almost right
            return cached?.Champions;
        }
    }

    private static string IconUrl(string version, string championId) =>
        $"{BaseUrl}/cdn/{version}/img/champion/{Uri.EscapeDataString(championId)}.png";

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

    private sealed record CachedChampions(string Version, IReadOnlyList<Champion> Champions);

    // champion.json: { "data": { "Aatrox": { "id": "Aatrox", "name": "Aatrox", "title": "..." }, ... } }
    private sealed record ChampionListDto(Dictionary<string, ChampionDto>? Data);

    private sealed record ChampionDto(string Id, string Name, string Title);
}
