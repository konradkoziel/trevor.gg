using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace trevor.Infrastructure.Web;

// Typed client for /api/bot/... of the website; the shared key goes in X-Trevor-Key
public class WebApiClient(HttpClient httpClient, IOptions<WebApiOptions> options, ILogger<WebApiClient> logger) : IWebApiClient
{
    public const string KeyHeader = "X-Trevor-Key";

    public bool IsConfigured => options.Value.IsConfigured;

    public string? SiteUrl => options.Value.SiteUrl;

    public Task<LinkedPlayer?> GetLinkedPlayerAsync(string discordId, CancellationToken ct = default) =>
        GetAsync<LinkedPlayer>($"api/bot/players/{Uri.EscapeDataString(discordId)}", ct);

    public Task<IReadOnlyList<LinkedPlayer>?> GetTrackedPlayersAsync(CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<LinkedPlayer>>("api/bot/tracked", ct);

    public Task<IReadOnlyList<WebBuildCard>?> GetBuildsAsync(string discordId, string champion, CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<WebBuildCard>>(
            $"api/bot/players/{Uri.EscapeDataString(discordId)}/builds?champion={Uri.EscapeDataString(champion)}", ct);

    // 404 is an answer (nothing there); anything else that fails is logged and treated the same, so a broken site
    // only takes away the website features, not the commands
    private async Task<T?> GetAsync<T>(string path, CancellationToken ct) where T : class
    {
        if (!IsConfigured)
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{options.Value.Url!.TrimEnd('/')}/{path}");
        request.Headers.Add(KeyHeader, options.Value.Key);
        try
        {
            using var response = await httpClient.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Website API answered {Status}", (int)response.StatusCode);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<T>(ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Website API call failed");
            return null;
        }
    }
}
