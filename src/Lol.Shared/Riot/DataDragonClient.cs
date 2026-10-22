using System.Globalization;
using Lol.Shared.Riot.Models;

namespace Lol.Shared.Riot;

// Champion icons and the champion list, used by the bot and the website
public class DataDragonClient(DataDragonCdn cdn) : IDataDragon
{
    // Match-V5 championName values whose Data Dragon id is spelled differently
    private static readonly Dictionary<string, string> IconIdByChampionName = new()
    {
        ["FiddleSticks"] = "Fiddlesticks"
    };

    // Sorted like the Polish client sorts them (e.g. Ł after L)
    private static readonly StringComparer NameOrder = StringComparer.Create(CultureInfo.GetCultureInfo("pl-PL"), ignoreCase: true);

    public async Task<string?> ChampionIconUrlAsync(string championName, CancellationToken ct = default)
    {
        var version = await cdn.LatestVersionAsync(ct);
        return version is null ? null : IconUrl(version, IconIdByChampionName.GetValueOrDefault(championName, championName));
    }

    public async Task<string?> ProfileIconUrlAsync(int iconId, CancellationToken ct = default)
    {
        var version = await cdn.LatestVersionAsync(ct);
        return version is null ? null : DataDragonCdn.Url(version, $"img/profileicon/{iconId}.png");
    }

    public async Task<string?> ItemIconUrlAsync(int itemId, CancellationToken ct = default)
    {
        var version = await cdn.LatestVersionAsync(ct);
        return version is null ? null : DataDragonCdn.Url(version, $"img/item/{itemId}.png");
    }

    public Task<IReadOnlyList<Champion>?> ChampionsAsync(CancellationToken ct = default) =>
        cdn.GetAsync<ChampionListDto, IReadOnlyList<Champion>>($"data/{DataDragonCdn.Language}/champion.json", (list, version) =>
            list.Data is not { Count: > 0 } data
                ? null
                : data.Values
                    .Select(c => new Champion(c.Id, c.Name, c.Title, IconUrl(version, c.Id),
                        int.TryParse(c.Key, out var key) ? key : 0, c.Blurb ?? "", SplashUrl(c.Id)))
                    .OrderBy(c => c.Name, NameOrder)
                    .ToList(), ct);

    // Splash art isn't versioned; skin 0 is the default one
    private static string SplashUrl(string championId) => DataDragonCdn.ImageUrl($"champion/splash/{Uri.EscapeDataString(championId)}_0.jpg");

    private static string IconUrl(string version, string championId) =>
        DataDragonCdn.Url(version, $"img/champion/{Uri.EscapeDataString(championId)}.png");

    // champion.json: { "data": { "Aatrox": { "id": "Aatrox", "key": "266", "name": "Aatrox", "title": "...", "blurb": "..." }, ... } }
    private sealed record ChampionListDto(Dictionary<string, ChampionDto>? Data);

    private sealed record ChampionDto(string Id, string Key, string Name, string Title, string? Blurb);
}
