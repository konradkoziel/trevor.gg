using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Tests;

public sealed class FakeDiscordApi : IDiscordApiClient
{
    public List<(string Url, DiscordResponseData Message)> Webhooks { get; } = [];
    public bool Fail { get; set; }

    public Task SendFollowupAsync(string applicationId, string interactionToken, DiscordResponseData message, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task SendWebhookAsync(string webhookUrl, string content, CancellationToken ct = default) =>
        SendWebhookAsync(webhookUrl, DiscordResponseData.FromText(content), ct);

    public Task SendWebhookAsync(string webhookUrl, DiscordResponseData message, CancellationToken ct = default)
    {
        if (Fail)
            return Task.FromException(new HttpRequestException("Discord is down"));
        Webhooks.Add((webhookUrl, message));
        return Task.CompletedTask;
    }
}
