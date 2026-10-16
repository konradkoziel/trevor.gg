using OpenAI.Chat;
using trevor.Core;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;

namespace trevor.Features.Ask;

public class AskCommand(ChatClient chatClient) : ICommand
{
    public string Name => "ask";
    public bool IsDeferred => true;

    public async Task<string> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default)
    {
        var question = interaction.GetOption("question");
        if (string.IsNullOrWhiteSpace(question))
            return "Zadaj pytanie, geniuszu.";

        var result = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(TrevorPrompts.Persona),
                new UserChatMessage(question),
            ],
            new ChatCompletionOptions
            {
                MaxOutputTokenCount = 200
            },
            ct);

        var answer = result.Value.Content.FirstOrDefault()?.Text ?? "Brak odpowiedzi. Nawet ja nie mam na to słów.";
        var quoted = string.Join("\n", question.Split('\n').Select(line => $"> {line}"));
        return $"**🧠 Pytanie:**\n{quoted}\n{answer}";
    }
}
