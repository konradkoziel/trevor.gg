using trevor.Infrastructure.Discord.Models;

namespace trevor.Infrastructure.Discord;

// Outgoing messages to Discord; an interface so features that post on their own can be tested
public interface IDiscordApiClient
{
    Task SendFollowupAsync(string applicationId, string interactionToken, DiscordResponseData message, CancellationToken ct = default);

    Task SendWebhookAsync(string webhookUrl, string content, CancellationToken ct = default);

    Task SendWebhookAsync(string webhookUrl, DiscordResponseData message, CancellationToken ct = default);
}
