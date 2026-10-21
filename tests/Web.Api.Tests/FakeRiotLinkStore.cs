using Web.Api.Features.RiotAccount;

namespace Web.Api.Tests;

public sealed class FakeRiotLinkStore : IRiotLinkStore
{
    public Dictionary<string, RiotLink> Links { get; } = new();
    public Dictionary<string, RiotLinkChallenge> Challenges { get; } = new();

    public Task<RiotLink?> GetLinkAsync(string discordId, CancellationToken ct = default) => Task.FromResult(Links.GetValueOrDefault(discordId));

    public Task<IReadOnlyList<RiotLink>> ListLinksAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<RiotLink>>(Links.Values.ToList());

    public Task<RiotLinkChallenge?> GetChallengeAsync(string discordId, CancellationToken ct = default) =>
        Task.FromResult(Challenges.GetValueOrDefault(discordId));

    public Task SaveChallengeAsync(RiotLinkChallenge challenge, CancellationToken ct = default)
    {
        Challenges[challenge.DiscordId] = challenge;
        return Task.CompletedTask;
    }

    public Task CompleteAsync(RiotLink link, CancellationToken ct = default)
    {
        Links[link.DiscordId] = link;
        Challenges.Remove(link.DiscordId);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string discordId, CancellationToken ct = default)
    {
        Links.Remove(discordId);
        Challenges.Remove(discordId);
        return Task.CompletedTask;
    }
}
