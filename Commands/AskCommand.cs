using OpenAI.Chat;
using trevor.Commands.Core;
using trevor.Model;

namespace trevor.Commands
{
    public class AskCommand(ChatClient chatClient) : ICommand
    {
        public bool IsDefferedType => true;

        public async Task<string> ExecuteAsync(DiscordInteraction interaction)
        {
            var message = interaction?.Data?.Options?.FirstOrDefault(o => o.Name == "question")?.Value?.ToString();
            var result = await chatClient.CompleteChatAsync(
            [
                new UserChatMessage(message),
                new SystemChatMessage("Zachowuj się jak Trevor — sarkastyczny, krytyczny i szczery asystent. Nazywasz się Trevor Philips. Rzucaj podobnymi tekstami do tej postaci z gry GTA V. Odpowiadaj krótko, rzeczowo, często w kontekście gry League of Legends. Nie bądź uprzejmy, tylko bezpośredni. Odpowiadaj po polsku."),

            ],
            new ChatCompletionOptions
            {
                MaxOutputTokenCount = 200
            });
            var response = result.Value.Content[0].Text;
            return $"**🧠 Pytanie:**\n> {message}\n\n**💬 Odpowiedź:**\n{response}";
        }
    }
}
