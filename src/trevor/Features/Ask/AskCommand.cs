using trevor.Core;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.OpenAi;

namespace trevor.Features.Ask;

public class AskCommand(ITrevorAi ai) : ICommand
{
    public string Name => "ask";
    public bool IsDeferred => true;

    public async Task<DiscordResponseData> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default)
    {
        var question = interaction.GetOption("question");
        if (string.IsNullOrWhiteSpace(question))
            return DiscordResponseData.FromText("Zadaj pytanie, geniuszu.");

        var answer = await ai.ReplyAsync(string.Empty, question, ct: ct)
                     ?? "Brak odpowiedzi. Nawet ja nie mam na to słów.";
        return DiscordResponseData.FromText($"**🧠 Pytanie:**\n{DiscordText.Quote(question)}\n{answer}");
    }
}
