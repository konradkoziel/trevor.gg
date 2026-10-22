using Lol.Shared.Riot.Models;

namespace Lol.Shared.Riot;

// Riot's static asset CDN (champion icons etc.)
public interface IDataDragon
{
    // Null when Data Dragon can't be reached; callers just show no icon
    Task<string?> ChampionIconUrlAsync(string championName, CancellationToken ct = default);

    // Null when Data Dragon can't be reached
    Task<string?> ProfileIconUrlAsync(int iconId, CancellationToken ct = default);

    // Any item by id, also ones the shop list leaves out (Ornn upgrades, champion items); null when Data Dragon can't be reached
    Task<string?> ItemIconUrlAsync(int itemId, CancellationToken ct = default);

    // Every champion of the latest patch, sorted by name; null when Data Dragon can't be reached
    Task<IReadOnlyList<Champion>?> ChampionsAsync(CancellationToken ct = default);
}
