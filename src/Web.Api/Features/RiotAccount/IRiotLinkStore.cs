namespace Web.Api.Features.RiotAccount;

public interface IRiotLinkStore
{
    Task<RiotLink?> GetLinkAsync(string discordId, CancellationToken ct = default);

    // Every link, for the leaderboard, the profiles and the one-owner check
    Task<IReadOnlyList<RiotLink>> ListLinksAsync(CancellationToken ct = default);

    // Replaces the user's earlier link
    Task SaveLinkAsync(RiotLink link, CancellationToken ct = default);

    Task RemoveAsync(string discordId, CancellationToken ct = default);
}
