namespace Lol.Shared.Riot;

// Riot's static asset CDN (champion icons etc.)
public interface IDataDragon
{
    // Null when Data Dragon can't be reached; callers just show no icon
    Task<string?> ChampionIconUrlAsync(string championName, CancellationToken ct = default);
}
