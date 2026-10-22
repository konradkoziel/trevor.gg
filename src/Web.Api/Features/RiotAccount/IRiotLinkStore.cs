namespace Web.Api.Features.RiotAccount;

public interface IRiotLinkStore
{
    // The user's accounts, in no particular order (RiotLinks.Ordered sorts them)
    Task<IReadOnlyList<RiotLink>> ListAccountsAsync(string discordId, CancellationToken ct = default);

    // Every account of every user, for the leaderboard, the profiles and the one-owner check
    Task<IReadOnlyList<RiotLink>> ListLinksAsync(CancellationToken ct = default);

    // Adds the account or replaces it (same user, same PUUID)
    Task SaveLinkAsync(RiotLink link, CancellationToken ct = default);

    Task RemoveAsync(string discordId, string puuid, CancellationToken ct = default);
}
