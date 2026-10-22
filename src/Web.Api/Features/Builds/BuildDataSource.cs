using Lol.Shared.Riot;

namespace Web.Api.Features.Builds;

// Items, rune trees and summoner spells of the latest patch in one go; null when Data Dragon is unreachable
public static class BuildDataSource
{
    public static async Task<BuildData?> LoadAsync(IDataDragonCatalog catalog, CancellationToken ct = default)
    {
        var items = catalog.ItemsAsync(ct);
        var runes = catalog.RuneTreesAsync(ct);
        var spells = catalog.SummonerSpellsAsync(ct);
        await Task.WhenAll(items, runes, spells);

        return items.Result is { } i && runes.Result is { } r && spells.Result is { } s ? new BuildData(i, r, s) : null;
    }
}
