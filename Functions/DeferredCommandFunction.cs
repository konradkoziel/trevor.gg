using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using trevor.Core;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Functions;

// Runs slow commands (e.g. /ask) after the HTTP function has answered Discord with "thinking…".
// A failed followup throws, so the queue retries it (maxDequeueCount in host.json), then moves it to the poison queue.
public class DeferredCommandFunction(
    ILogger<DeferredCommandFunction> logger,
    InteractionHandler handler,
    DiscordApiClient discord)
{
    public const string QueueName = "deferred-commands";

    [Function("DeferredCommands")]
    public async Task Run([QueueTrigger(QueueName)] string message, CancellationToken ct)
    {
        var interaction = JsonSerializer.Deserialize<DiscordInteraction>(message, DiscordJson.Options)
                          ?? throw new InvalidOperationException("Empty deferred interaction message");

        var content = await handler.ExecuteDeferredAsync(interaction, ct);
        await discord.SendFollowupAsync(interaction.ApplicationId, interaction.Token, content, ct);

        logger.LogInformation("Sent followup for interaction {InteractionId} command {Command}",
            interaction.Id, interaction.Data?.Name);
    }
}
