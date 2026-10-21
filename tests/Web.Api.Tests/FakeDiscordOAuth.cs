using Web.Api.Infrastructure.Discord;

namespace Web.Api.Tests;

// User = null plays Discord rejecting the code; member = null a user who isn't on the server
public sealed class FakeDiscordOAuth(DiscordOAuthUser? user = null, DiscordGuildMember? member = null) : IDiscordOAuthClient
{
    public string? ExchangedCode { get; private set; }

    public Uri AuthorizeUrl(string state, bool silent) =>
        new($"https://discord.test/authorize?state={state}&silent={silent.ToString().ToLowerInvariant()}");

    public Task<DiscordIdentity?> GetIdentityAsync(string code, CancellationToken ct = default)
    {
        ExchangedCode = code;
        return Task.FromResult(user is null ? null : new DiscordIdentity(user, member));
    }
}
