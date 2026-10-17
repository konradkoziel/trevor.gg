using Microsoft.Extensions.Logging;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Core;

public class InteractionHandler(CommandRegistry registry, ILogger<InteractionHandler> logger)
{
    public const string ErrorMessage = "Coś się zjebało po mojej stronie. Spróbuj jeszcze raz.";

    public async Task<InteractionResult> HandleAsync(DiscordInteraction interaction, CancellationToken ct = default)
    {
        if (interaction.Type == InteractionType.Ping)
            return new InteractionResult(DiscordResponse.Pong);

        if (interaction.Type != InteractionType.ApplicationCommand)
            return new InteractionResult(DiscordResponse.Message("Nie wiem, czego ode mnie chcesz."));

        var command = registry.Find(interaction.Data?.Name);
        if (command is null)
            return new InteractionResult(DiscordResponse.Message(UnknownCommandMessage(interaction)));

        if (command.IsDeferred)
            return new InteractionResult(DiscordResponse.Deferred, EnqueueDeferred: true);

        return new InteractionResult(DiscordResponse.Message(await ExecuteAsync(command, interaction, ct)));
    }

    // Runs a deferred command from the queue; returns the followup message
    public async Task<DiscordResponseData> ExecuteDeferredAsync(DiscordInteraction interaction, CancellationToken ct = default)
    {
        var command = registry.Find(interaction.Data?.Name);
        return command is null
            ? DiscordResponseData.FromText(UnknownCommandMessage(interaction))
            : await ExecuteAsync(command, interaction, ct);
    }

    private async Task<DiscordResponseData> ExecuteAsync(ICommand command, DiscordInteraction interaction, CancellationToken ct)
    {
        try
        {
            return await command.ExecuteAsync(interaction, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Command {Command} failed for interaction {InteractionId}", command.Name, interaction.Id);
            return DiscordResponseData.FromText(ErrorMessage);
        }
    }

    private static string UnknownCommandMessage(DiscordInteraction interaction) =>
        $"Nie znam komendy `/{interaction.Data?.Name}`. Wymyśliłeś ją sobie?";
}
