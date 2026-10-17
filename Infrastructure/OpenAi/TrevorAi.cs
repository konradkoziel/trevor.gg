using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace trevor.Infrastructure.OpenAi;

// One place for every AI call: Trevor's persona, output limits and failure handling.
// Commands pass only their own instruction and data.
public interface ITrevorAi
{
    // Null when the model fails or returns nothing, so callers can fall back to a canned line
    Task<string?> ReplyAsync(string instruction, string input, int maxOutputTokens = 200, CancellationToken ct = default);
}

public class TrevorAi(ChatClient chatClient, ILogger<TrevorAi> logger) : ITrevorAi
{
    public async Task<string?> ReplyAsync(string instruction, string input, int maxOutputTokens = 200, CancellationToken ct = default)
    {
        try
        {
            var result = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage($"{TrevorPrompts.Persona}\n\n{instruction}"),
                    new UserChatMessage(input),
                ],
                new ChatCompletionOptions { MaxOutputTokenCount = maxOutputTokens },
                ct);

            var text = result.Value.Content.FirstOrDefault()?.Text?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "AI reply failed");
            return null;
        }
    }
}
