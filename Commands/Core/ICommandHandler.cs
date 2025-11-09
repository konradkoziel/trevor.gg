using System.Text.Json;
using trevor.Model;
using trevor.Model.Response.cs;

namespace trevor.Commands.Core
{
    public interface ICommandHandler
    {
        public Task<DiscordResponse> HandleCommandAsync(ICommand command, DiscordInteraction interaction, JsonSerializerOptions jsonSerializerOptions);
    }
}
