using System.Net;
using System.Net.Http.Json;
using Lol.Shared.Riot.Models;
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

    public async Task<WebTracking> SetTrackedAsync(string riotId, bool tracked, CancellationToken ct = default)
    {
        if (!IsConfigured)
            return WebTracking.Unavailable;
        if (!RiotId.TryParse(riotId, out var id))
            return WebTracking.NotLinked;

        var path = $"api/bot/tracked/{Uri.EscapeDataString(id.GameName)}/{Uri.EscapeDataString(id.TagLine)}";
        using var request = Request(tracked ? HttpMethod.Put : HttpMethod.Delete, path);
        try
        {
            using var response = await httpClient.SendAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return WebTracking.NotLinked;
            if (response.IsSuccessStatusCode)
                return WebTracking.Switched;
            logger.LogWarning("Website API answered {Status}", (int)response.StatusCode);
            return WebTracking.Unavailable;
        }
        catch (Exception ex) when (IsCallFailure(ex, ct))
        {
            logger.LogWarning(ex, "Website API call failed");
            return WebTracking.Unavailable;
        }
    }

    public Task<IReadOnlyList<WebBuildCard>?> GetBuildsAsync(string discordId, string champion, CancellationToken ct = default) =>
        GetAsync<IReadOnlyList<WebBuildCard>>(
            $"api/bot/players/{Uri.EscapeDataString(discordId)}/builds?champion={Uri.EscapeDataString(champion)}", ct);

    // 404 is an answer (nothing there); anything else that fails is logged and treated the same, so a broken site
    // only takes away the website features, not the commands
    private async Task<T?> GetAsync<T>(string path, CancellationToken ct) where T : class
    {
        if (!IsConfigured)
            return null;

        using var request = Request(HttpMethod.Get, path);
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
        catch (Exception ex) when (IsCallFailure(ex, ct) || ex is System.Text.Json.JsonException)
        {
            logger.LogWarning(ex, "Website API call failed");
            return null;
        }
    }

    private HttpRequestMessage Request(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, $"{options.Value.Url!.TrimEnd('/')}/{path}");
        request.Headers.Add(KeyHeader, options.Value.Key);
        return request;
    }

    // The site is down or slow; a cancelled run is not a failure of the site
    private static bool IsCallFailure(Exception ex, CancellationToken ct) =>
        ex is HttpRequestException || ex is TaskCanceledException && !ct.IsCancellationRequested;
}
