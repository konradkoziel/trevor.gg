namespace Web.Api.Infrastructure.Discord;

// The Discord calls of the authorization code flow: https://discord.com/developers/docs/topics/oauth2
public interface IDiscordOAuthClient
{
    // The login page Discord shows the user; state comes back unchanged to the callback.
    // Silent: skip the consent screen when the user already agreed (Discord answers with an error otherwise)
    Uri AuthorizeUrl(string state, bool silent);

    // Null when Discord rejects the code (expired, reused) or can't be reached
    Task<DiscordIdentity?> GetIdentityAsync(string code, CancellationToken ct = default);
}
