namespace Web.Api.Infrastructure.Discord;

// The two Discord calls of the authorization code flow: https://discord.com/developers/docs/topics/oauth2
public interface IDiscordOAuthClient
{
    // The login page Discord shows the user; state comes back unchanged to the callback
    Uri AuthorizeUrl(string state);

    // Null when Discord rejects the code (expired, reused) or can't be reached
    Task<DiscordOAuthUser?> GetUserAsync(string code, CancellationToken ct = default);
}
