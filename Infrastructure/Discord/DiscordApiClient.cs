using System.Text;
using System.Text.Json;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Infrastructure.Discord;

// Typed client for outgoing calls to Discord. URLs carry tokens/secrets, so they are never logged.
public class DiscordApiClient(HttpClient httpClient)
{
    public const string ApiBaseUrl = "https://discord.com/api/v10";

    public Task SendFollowupAsync(string applicationId, string interactionToken, string content, CancellationToken ct = default) =>
        PostMessageAsync($"{ApiBaseUrl}/webhooks/{applicationId}/{interactionToken}", content, ct);

    public Task SendWebhookAsync(string webhookUrl, string content, CancellationToken ct = default) =>
        PostMessageAsync(webhookUrl, content, ct);

    private async Task PostMessageAsync(string url, string content, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(DiscordResponseData.FromText(content), DiscordJson.Options);
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
