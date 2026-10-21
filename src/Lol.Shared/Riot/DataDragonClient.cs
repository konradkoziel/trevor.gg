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

    public Task<IReadOnlyList<Champion>?> ChampionsAsync(CancellationToken ct = default) =>
        cdn.GetAsync<ChampionListDto, IReadOnlyList<Champion>>($"data/{DataDragonCdn.Language}/champion.json", (list, version) =>
            list.Data is not { Count: > 0 } data
                ? null
                : data.Values
                    .Select(c => new Champion(c.Id, c.Name, c.Title, IconUrl(version, c.Id)))
                    .OrderBy(c => c.Name, NameOrder)
                    .ToList(), ct);

    private static string IconUrl(string version, string championId) =>
        DataDragonCdn.Url(version, $"img/champion/{Uri.EscapeDataString(championId)}.png");

    // champion.json: { "data": { "Aatrox": { "id": "Aatrox", "name": "Aatrox", "title": "..." }, ... } }
    private sealed record ChampionListDto(Dictionary<string, ChampionDto>? Data);

    private sealed record ChampionDto(string Id, string Name, string Title);
}
