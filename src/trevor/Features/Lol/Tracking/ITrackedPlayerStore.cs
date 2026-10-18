namespace trevor.Features.Lol.Tracking;

public interface ITrackedPlayerStore
{
    Task<IReadOnlyList<TrackedPlayer>> ListAsync(CancellationToken ct = default);

    // Adds the player, or overwrites them when already tracked
    Task AddAsync(TrackedPlayer player, CancellationToken ct = default);

    // False when the player wasn't tracked
    Task<bool> RemoveAsync(string puuid, CancellationToken ct = default);

    Task SetLastMatchAsync(string puuid, string matchId, CancellationToken ct = default);
}
