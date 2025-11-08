using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using trevor.Commands.Core;
using trevor.Common;
using trevor.Model;

namespace trevor.Commands
{
    public class AskCommand : ICommand
    {
        private readonly ChatClient _chatClient;

        public AskCommand(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<string> ExecuteAsync(DiscordInteraction interaction)
        {
            var message = interaction?.Data?.Options?.FirstOrDefault(o => o.Name == "question")?.Value?.ToString();
            var result = await _chatClient.CompleteChatAsync(
            [
                new SystemChatMessage("Pisz jak najkrócej i konretnie."),
                new UserChatMessage(message)
            ],
            new ChatCompletionOptions
            {
                MaxOutputTokenCount = 200
            });
            var response = result.Value.Content[0].Text;
            return response;
        }
    }
}
