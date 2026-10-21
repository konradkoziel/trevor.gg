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

    // Only id, name and avatar: no e-mail, no servers
    private const string Scope = "identify";

    private DiscordOAuthOptions Options => options.Value;

    public Uri AuthorizeUrl(string state) => new(QueryHelpers.AddQueryString(AuthorizeEndpoint, new Dictionary<string, string?>
    {
        ["client_id"] = Options.ClientId,
        ["response_type"] = "code",
        ["redirect_uri"] = Options.RedirectUri,
        ["scope"] = Scope,
        ["state"] = state,
    }));

    public async Task<DiscordOAuthUser?> GetUserAsync(string code, CancellationToken ct = default)
    {
        try
        {
            var accessToken = await ExchangeCodeAsync(code, ct);
            if (accessToken is null)
                return null;

            using var request = new HttpRequestMessage(HttpMethod.Get, CurrentUserEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Discord /users/@me answered {Status}", (int)response.StatusCode);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<DiscordOAuthUser>(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Discord OAuth call failed");
            return null;
        }
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
