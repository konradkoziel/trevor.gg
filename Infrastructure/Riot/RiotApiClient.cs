using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace trevor.Infrastructure.Riot;

public interface IRiotApiClient
{
    bool IsConfigured { get; }

    // Null when the Riot ID doesn't exist
    Task<RiotAccount?> GetAccountAsync(RiotId riotId, CancellationToken ct = default);

    Task<IReadOnlyList<LeagueEntry>> GetLeagueEntriesAsync(string puuid, CancellationToken ct = default);
}

public class RiotApiException(HttpStatusCode statusCode)
    : Exception($"Riot API returned {(int)statusCode}")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

// Typed client: the rest of the app sees RiotAccount/LeagueEntry, never Riot's HTTP details.
// Retries, timeouts and 429 back-off come from the resilience handler registered in Program.cs.
public class RiotApiClient(HttpClient httpClient, IOptions<RiotOptions> options) : IRiotApiClient
{
    public bool IsConfigured => options.Value.HasApiKey;

    public async Task<RiotAccount?> GetAccountAsync(RiotId riotId, CancellationToken ct = default)
    {
        var region = RiotRouting.AccountRegion(options.Value.Platform);
        var url = $"https://{region}.api.riotgames.com/riot/account/v1/accounts/by-riot-id/" +
                  $"{Uri.EscapeDataString(riotId.GameName)}/{Uri.EscapeDataString(riotId.TagLine)}";

        using var response = await SendAsync(url, ct);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<RiotAccount>(ct);
    }

    public async Task<IReadOnlyList<LeagueEntry>> GetLeagueEntriesAsync(string puuid, CancellationToken ct = default)
    {
        var url = $"https://{options.Value.Platform.ToLowerInvariant()}.api.riotgames.com/lol/league/v4/entries/by-puuid/" +
                  Uri.EscapeDataString(puuid);

        using var response = await SendAsync(url, ct);
        EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<List<LeagueEntry>>(ct) ?? [];
    }

    private Task<HttpResponseMessage> SendAsync(string url, CancellationToken ct)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("RIOT_API_KEY is not set");

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Riot-Token", options.Value.ApiKey);
        return httpClient.SendAsync(request, ct);
    }

    private static void EnsureSuccess(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            throw new RiotApiException(response.StatusCode);
    }
}
