using Web.Api.Features.RiotAccount;

namespace Web.Api.Tests;

public sealed class FakeRiotLinkStore : IRiotLinkStore
{
    public List<RiotLink> Accounts { get; } = [];

    // Adds or replaces (same user, same PUUID), like the table store
    public void Add(RiotLink link)
    {
        Accounts.RemoveAll(a => a.DiscordId == link.DiscordId && a.Puuid == link.Puuid);
        Accounts.Add(link);
    }

    public Task<IReadOnlyList<RiotLink>> ListAccountsAsync(string discordId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<RiotLink>>(Accounts.Where(a => a.DiscordId == discordId).ToList());

    public Task<IReadOnlyList<RiotLink>> ListLinksAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<RiotLink>>(Accounts.ToList());

    public Task SaveLinkAsync(RiotLink link, CancellationToken ct = default)
    {
        Add(link);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string discordId, string puuid, CancellationToken ct = default)
    {
        Accounts.RemoveAll(a => a.DiscordId == discordId && a.Puuid == puuid);
        return Task.CompletedTask;
    }
}
