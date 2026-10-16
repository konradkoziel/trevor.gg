using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using trevor.Model;
using trevor.Model.Response.cs;

namespace trevor.Commands.Core
{
    public class CommandHandler(HttpClient httpClient, ILogger<CommandHandler> logger) : ICommandHandler
    {
        public async Task<DiscordResponse> HandleCommandAsync(
            ICommand command,
            DiscordInteraction interaction,
            JsonSerializerOptions jsonSerializerOptions)
        {
            if (command.IsDefferedType)
            {
                _ = Task.Run(async () =>
                {
                    var followupUrl = $"https://discord.com/api/v10/webhooks/{interaction.ApplicationId}/{interaction.Token}";

                    string commandResponse;
                    try
                    {
                        commandResponse = await command.ExecuteAsync(interaction);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Deferred command {Command} failed", interaction.Data?.Name);
                        commandResponse = "Coś się zjebało po mojej stronie. Spróbuj jeszcze raz.";
                    }

                    try
                    {
                        var message = new DiscordResponseData(Content: commandResponse);
                        var json = JsonSerializer.Serialize(message, jsonSerializerOptions);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync(followupUrl, content);
                        if (!response.IsSuccessStatusCode)
                            logger.LogError("Discord followup failed with {StatusCode}: {Body}",
                                (int)response.StatusCode, await response.Content.ReadAsStringAsync());
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to send Discord followup for {Command}", interaction.Data?.Name);
                    }
                });

                return new DiscordResponse(Type: 5);
            }

            var commandResponseSync = await command.ExecuteAsync(interaction);
            return new DiscordResponse(
                Type: 4,
                Data: new DiscordResponseData(Content: commandResponseSync)
            );
        }
    }
}
