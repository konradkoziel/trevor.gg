namespace Web.Api.Features.RiotAccount;

public interface IRiotLinkStore
{
    Task<RiotLink?> GetLinkAsync(string discordId, CancellationToken ct = default);

    // Every verified link, for the leaderboard
    Task<IReadOnlyList<RiotLink>> ListLinksAsync(CancellationToken ct = default);

    Task<RiotLinkChallenge?> GetChallengeAsync(string discordId, CancellationToken ct = default);

    // Replaces an earlier challenge of the same user
    Task SaveChallengeAsync(RiotLinkChallenge challenge, CancellationToken ct = default);

    // Stores the link and drops the challenge it came from
    Task CompleteAsync(RiotLink link, CancellationToken ct = default);

    // Drops the link and any pending challenge
    Task RemoveAsync(string discordId, CancellationToken ct = default);
}
