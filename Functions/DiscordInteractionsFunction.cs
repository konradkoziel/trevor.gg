using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using trevor.Core;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Functions;

public class DiscordInteractionsFunction(
    ILogger<DiscordInteractionsFunction> logger,
    IDiscordSignatureVerifier signatureVerifier,
    InteractionHandler handler)
{
    // Keep the name: the Interactions Endpoint URL in the Developer Portal is /api/DiscordCommands
    [Function("DiscordCommands")]
    public async Task<InteractionOutput> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        using var buffer = new MemoryStream();
        await req.Body.CopyToAsync(buffer, ct);
        var body = buffer.ToArray();

        var signature = req.Headers["X-Signature-Ed25519"].FirstOrDefault();
        var timestamp = req.Headers["X-Signature-Timestamp"].FirstOrDefault();
        if (!signatureVerifier.Verify(body, signature, timestamp))
            return new InteractionOutput { Result = new UnauthorizedResult() };

        DiscordInteraction? interaction;
        try
        {
            interaction = JsonSerializer.Deserialize<DiscordInteraction>(body, DiscordJson.Options);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Invalid interaction payload");
            interaction = null;
        }

        if (interaction is null)
            return new InteractionOutput { Result = new BadRequestResult() };

        var result = await handler.HandleAsync(interaction, ct);

        logger.LogInformation(
            "Interaction {InteractionId} type {Type} command {Command} in guild {GuildId} → response {ResponseType} in {Elapsed} ms",
            interaction.Id, interaction.Type, interaction.Data?.Name, interaction.GuildId,
            result.Response.Type, sw.ElapsedMilliseconds);

        return new InteractionOutput
        {
            Result = new ContentResult
            {
                Content = JsonSerializer.Serialize(result.Response, DiscordJson.Options),
                ContentType = "application/json",
                StatusCode = StatusCodes.Status200OK
            },
            DeferredInteraction = result.EnqueueDeferred
                ? JsonSerializer.Serialize(interaction, DiscordJson.Options)
                : null
        };
    }
}

public class InteractionOutput
{
    [HttpResult]
    public required IActionResult Result { get; init; }

    // Null means nothing is written to the queue
    [QueueOutput(DeferredCommandFunction.QueueName)]
    public string? DeferredInteraction { get; init; }
}
