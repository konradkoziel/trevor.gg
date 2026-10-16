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
            var message = interaction?.Data?.Options?.FirstOrDefault(o => o.Name == "question")?.Value;
            if (string.IsNullOrWhiteSpace(message))
                return "Zadaj pytanie, geniuszu.";

            var result = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage("Zachowuj się jak Trevor — sarkastyczny, krytyczny i szczery asystent. Nazywasz się Trevor Philips. Rzucaj podobnymi tekstami do tej postaci z gry GTA V. Odpowiadaj krótko, rzeczowo, często w kontekście gry League of Legends. Nie bądź uprzejmy, tylko bezpośredni. Odpowiadaj po polsku."),
                new UserChatMessage(message),
            ],
            new ChatCompletionOptions
            {
                MaxOutputTokenCount = 200
            });
            var response = result.Value.Content.FirstOrDefault()?.Text ?? "Brak odpowiedzi. Nawet ja nie mam na to słów.";
            return $"**🧠 Pytanie:**\n> {message}\n{response}";
        }
    }
}
