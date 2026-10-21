using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Web.Api.Infrastructure.Discord;

public class DiscordOAuthClient(HttpClient httpClient, IOptions<DiscordOAuthOptions> options, ILogger<DiscordOAuthClient> logger)
    : IDiscordOAuthClient
{
    public const string AuthorizeEndpoint = "https://discord.com/oauth2/authorize";
    public const string TokenEndpoint = "https://discord.com/api/v10/oauth2/token";
    public const string CurrentUserEndpoint = "https://discord.com/api/v10/users/@me";

    // Id, name and avatar: no e-mail
    private const string IdentifyScope = "identify";

    // Membership of one given server (nickname, server avatar); Discord shows no list of the user's other servers
    private const string GuildMemberScope = "guilds.members.read";

    private DiscordOAuthOptions Options => options.Value;

    public static string GuildMemberEndpoint(string guildId) => $"{CurrentUserEndpoint}/guilds/{Uri.EscapeDataString(guildId)}/member";

    public Uri AuthorizeUrl(string state, bool silent)
    {
        var query = new Dictionary<string, string?>
        {
            ["client_id"] = Options.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = Options.RedirectUri,
            ["scope"] = Options.IsGuildOnly ? $"{IdentifyScope} {GuildMemberScope}" : IdentifyScope,
            ["state"] = state,
        };
        if (silent)
            query["prompt"] = "none";
        return new Uri(QueryHelpers.AddQueryString(AuthorizeEndpoint, query));
    }

    public async Task<DiscordIdentity?> GetIdentityAsync(string code, CancellationToken ct = default)
    {
        try
        {
            var accessToken = await ExchangeCodeAsync(code, ct);
            if (accessToken is null)
                return null;

            var user = await GetAsync<DiscordOAuthUser>(CurrentUserEndpoint, accessToken, ct);
            if (user is null)
                return null;
            if (!Options.IsGuildOnly)
                return new DiscordIdentity(user, null);

            var member = await GetMemberAsync(Options.GuildId!, accessToken, ct);
            return member.Failed ? null : new DiscordIdentity(user, member.Member);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Discord OAuth call failed");
            return null;
        }
    }

    // 404 = not on the server (a valid answer); anything else that isn't 200 = Discord failed
    private async Task<(DiscordGuildMember? Member, bool Failed)> GetMemberAsync(string guildId, string accessToken, CancellationToken ct)
    {
        using var response = await SendAsync(GuildMemberEndpoint(guildId), accessToken, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return (null, false);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Discord guild member lookup answered {Status}", (int)response.StatusCode);
            return (null, true);
        }
        return (await response.Content.ReadFromJsonAsync<DiscordGuildMember>(ct), false);
    }

    private async Task<T?> GetAsync<T>(string url, string accessToken, CancellationToken ct)
    {
        using var response = await SendAsync(url, accessToken, ct);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>(ct);

        logger.LogWarning("Discord {Endpoint} answered {Status}", new Uri(url).AbsolutePath, (int)response.StatusCode);
        return default;
    }

    private Task<HttpResponseMessage> SendAsync(string url, string accessToken, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return httpClient.SendAsync(request, ct);
    }

    private async Task<string?> ExchangeCodeAsync(string code, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = Options.RedirectUri,
            }),
        };
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Options.ClientId}:{Options.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            // The body may echo the code, so only the status is logged
            logger.LogWarning("Discord token exchange answered {Status}", (int)response.StatusCode);
            return null;
        }
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        return token?.AccessToken;
    }

    private sealed record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
}
