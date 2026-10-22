using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Web.Api.Features.Matches;

// Finished matches never change, so each one is fetched from Riot once per app instance and kept for a while
public class MatchDetailSource(IRiotApiClient riot, IDataDragon dataDragon, IDataDragonCatalog catalog, IMemoryCache cache)
{
    // Long enough for a group looking at the same games; the cache holds at most CacheSize matches
    public static readonly TimeSpan CacheFor = TimeSpan.FromHours(6);
    public const int CacheSize = 300;

    public async Task<MatchDetail?> GetAsync(string matchId, CancellationToken ct = default)
    {
        if (cache.TryGetValue(matchId, out MatchDetail? cached))
            return cached;

        var match = await riot.GetMatchAsync(matchId, ct);
        if (match is null)
            return null;

        var detail = MatchDetails.From(matchId, match, await AssetsAsync(match, ct));
        // Without Data Dragon the icons are missing: show it, but ask again next time
        if (detail.Teams.SelectMany(t => t.Players).All(p => p.ChampionIconUrl is not null))
            cache.Set(matchId, detail, new MemoryCacheEntryOptions { Size = 1, SlidingExpiration = CacheFor });
        return detail;
    }

    private async Task<MatchAssets> AssetsAsync(RiotMatch match, CancellationToken ct)
    {
        var players = match.Info.Participants;
        var championIcons = new Dictionary<string, string?>();
        foreach (var champion in players.Select(p => p.ChampionName).Distinct())
            championIcons[champion] = await dataDragon.ChampionIconUrlAsync(champion, ct);

        var shop = (await catalog.ItemsAsync(ct) ?? []).ToDictionary(i => i.Id);
        var items = new Dictionary<int, MatchIcon>();
        foreach (var id in players.SelectMany(p => p.ItemIds()).Where(id => id != 0).Distinct())
        {
            // Items the shop list leaves out (Ornn upgrades, champion items) still get an icon, just no name
            var known = shop.GetValueOrDefault(id.ToString(System.Globalization.CultureInfo.InvariantCulture));
            items[id] = new MatchIcon(known?.Name ?? string.Empty, known?.IconUrl ?? await dataDragon.ItemIconUrlAsync(id, ct));
        }

        var spells = (await catalog.SummonerSpellsAsync(ct) ?? []).Where(s => s.Key != 0).ToDictionary(s => s.Key);
        var trees = await catalog.RuneTreesAsync(ct) ?? [];
        var runes = trees.SelectMany(t => t.Slots).SelectMany(s => s).ToDictionary(r => r.Id);
        return new MatchAssets(championIcons, items, spells, runes, trees.ToDictionary(t => t.Id));
    }
}
