using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;

namespace Web.Api.Features.Home;

// This week's free champions; one Riot call per hour per app instance (the rotation changes on Tuesdays)
public class FreeRotation(IRiotApiClient riot, IDataDragon dataDragon, TimeProvider time)
{
    public static readonly TimeSpan CacheFor = TimeSpan.FromHours(1);

    private (IReadOnlyList<Champion> Champions, DateTimeOffset At)? _cached;

    // Null when Data Dragon is down; Riot errors propagate (RiotCalls turns them into 503)
    public async Task<IReadOnlyList<Champion>?> ChampionsAsync(CancellationToken ct = default)
    {
        if (_cached is { } cached && time.GetUtcNow() - cached.At < CacheFor)
            return cached.Champions;

        var champions = await dataDragon.ChampionsAsync(ct);
        if (champions is null)
            return null;

        var keys = (await riot.GetFreeChampionKeysAsync(ct)).ToHashSet();
        var free = champions.Where(c => keys.Contains(c.Key)).ToList();
        _cached = (free, time.GetUtcNow());
        return free;
    }
}
