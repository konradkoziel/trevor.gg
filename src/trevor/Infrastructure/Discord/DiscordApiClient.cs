using System.Text;
using System.Text.Json;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Infrastructure.Discord;

// Typed client for outgoing calls to Discord. URLs carry tokens/secrets, so they are never logged.
public class DiscordApiClient(HttpClient httpClient) : IDiscordApiClient
{
    public const string ApiBaseUrl = "https://discord.com/api/v10";

    public Task SendFollowupAsync(string applicationId, string interactionToken, DiscordResponseData message, CancellationToken ct = default) =>
        PostMessageAsync($"{ApiBaseUrl}/webhooks/{applicationId}/{interactionToken}", message, ct);

    public Task SendWebhookAsync(string webhookUrl, DiscordResponseData message, CancellationToken ct = default) =>
        PostMessageAsync(webhookUrl, message, ct);

    private async Task PostMessageAsync(string url, DiscordResponseData message, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(message, DiscordJson.Options);
        using var body = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await httpClient.PostAsync(url, body, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Discord returned {(int)response.StatusCode}: {error}", null, response.StatusCode);
        }
    }
}
