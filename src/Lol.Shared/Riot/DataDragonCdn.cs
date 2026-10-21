using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Lol.Shared.Riot;

// Riot's static data CDN: https://developer.riotgames.com/docs/lol#data-dragon
// One instance per app (registered as a singleton), so the version and the downloaded data stay cached across requests.
public class DataDragonCdn(HttpClient httpClient, TimeProvider time, ILogger<DataDragonCdn> logger)
{
    public const string BaseUrl = "https://ddragon.leagueoflegends.com";

    // Names and descriptions as the Polish client shows them
    public const string Language = "pl_PL";

    // Patches ship every two weeks; data a few hours old is the same
    private static readonly TimeSpan VersionTtl = TimeSpan.FromHours(6);

    private CachedVersion? _version;
    private readonly ConcurrentDictionary<string, CachedData> _data = new();

    public static string Url(string version, string path) => $"{BaseUrl}/cdn/{version}/{path}";

    // Rune icons are not versioned
    public static string ImageUrl(string path) => $"{BaseUrl}/cdn/img/{path}";

    // Null when Data Dragon has never answered; a stale version still gives working URLs
    public async Task<string?> LatestVersionAsync(CancellationToken ct = default)
    {
        var cached = _version;
        if (cached is not null && time.GetUtcNow() - cached.FetchedAt < VersionTtl)
            return cached.Version;

        try
        {
            var versions = await httpClient.GetFromJsonAsync<List<string>>($"{BaseUrl}/api/versions.json", ct);
            if (versions is { Count: > 0 })
                _version = cached = new CachedVersion(versions[0], time.GetUtcNow());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not fetch Data Dragon versions");
        }

        return cached?.Version;
    }

    // cdn/{latest version}/{path}, mapped once per patch and cached until the next one. When the download fails
    // the last good value is returned (maybe from the previous patch, still almost right), else null.
    public async Task<T?> GetAsync<TDto, T>(string path, Func<TDto, string, T?> map, CancellationToken ct = default)
        where T : class
    {
        var version = await LatestVersionAsync(ct);
        if (version is null)
            return null;

        _data.TryGetValue(path, out var cached);
        if (cached?.Version == version)
            return (T)cached.Value;

        try
        {
            var dto = await httpClient.GetFromJsonAsync<TDto>(Url(version, path), ct);
            var value = dto is null ? null : map(dto, version);
            if (value is null)
                return cached?.Value as T;

            _data[path] = new CachedData(version, value);
            return value;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not fetch Data Dragon {Path}", path);
            return cached?.Value as T;
        }
    }

    private sealed record CachedVersion(string Version, DateTimeOffset FetchedAt);

    private sealed record CachedData(string Version, object Value);
}
