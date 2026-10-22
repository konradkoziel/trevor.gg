using Web.Api.Features.RiotAccount;

namespace Web.Api.Tests;

public sealed class FakeRiotLinkStore : IRiotLinkStore
{
    public Dictionary<string, RiotLink> Links { get; } = new();

    public Task<RiotLink?> GetLinkAsync(string discordId, CancellationToken ct = default) => Task.FromResult(Links.GetValueOrDefault(discordId));

    public Task<IReadOnlyList<RiotLink>> ListLinksAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<RiotLink>>(Links.Values.ToList());

    public Task SaveLinkAsync(RiotLink link, CancellationToken ct = default)
    {
        Links[link.DiscordId] = link;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string discordId, CancellationToken ct = default)
    {
        Links.Remove(discordId);
        return Task.CompletedTask;
    }
}
