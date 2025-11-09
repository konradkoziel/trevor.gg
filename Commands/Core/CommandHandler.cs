using System.Text;
using System.Text.Json;
using trevor.Model;
using trevor.Model.Response.cs;

namespace trevor.Commands.Core
{
    public class CommandHandler(HttpClient httpClient) : ICommandHandler
    {
        public async Task<DiscordResponse> HandleCommandAsync(
            ICommand command,
            DiscordInteraction interaction,
            JsonSerializerOptions jsonSerializerOptions)
        {
            if (interaction?.Type == 1)
                return new DiscordResponse(Type: 1);
            
            if (command.IsDefferedType)
            {
                _ = Task.Run(async () =>
                {
                    var followupUrl = $"https://discord.com/api/v10/webhooks/{interaction!.ApplicationId}/{interaction.Token}";
                    var commandResponse = await command.ExecuteAsync(interaction);

                    var message = new DiscordResponseData(Content: commandResponse);
                    var json = JsonSerializer.Serialize(message, jsonSerializerOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await httpClient.PostAsync(followupUrl, content);
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