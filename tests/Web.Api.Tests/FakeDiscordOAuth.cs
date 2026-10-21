using Web.Api.Infrastructure.Discord;

namespace Web.Api.Tests;

// User = null plays Discord rejecting the code
public sealed class FakeDiscordOAuth(DiscordOAuthUser? user = null) : IDiscordOAuthClient
{
    public string? ExchangedCode { get; private set; }

    public Uri AuthorizeUrl(string state) => new($"https://discord.test/authorize?state={state}");

    public Task<DiscordOAuthUser?> GetUserAsync(string code, CancellationToken ct = default)
    {
        ExchangedCode = code;
        return Task.FromResult(user);
    }
}
