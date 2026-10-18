using trevor.Core;
using trevor.Infrastructure.Discord;
using trevor.Infrastructure.Discord.Models;
using trevor.Infrastructure.OpenAi;

namespace trevor.Features.RateThis;

// "Rate this" in a message's context menu (right-click → Apps): Trevor judges that message
public class RateThisCommand(ITrevorAi ai) : ICommand
{
    private const int VerdictMaxTokens = 150;

    public string Name => "Rate this";

    // An AI call can exceed Discord's 3 s limit
    public bool IsDeferred => true;

    public async Task<DiscordResponseData> ExecuteAsync(DiscordInteraction interaction, CancellationToken ct = default)
    {
        var message = interaction.GetTargetMessage();
        if (string.IsNullOrWhiteSpace(message?.Content))
            return DiscordResponseData.FromText(RateThisMessage.NothingToRate);

        var author = message.Author?.DisplayName ?? RateThisMessage.UnknownAuthor;
        var verdict = await ai.ReplyAsync(TrevorPrompts.MessageVerdict, RateThisMessage.AiInput(author, message.Content), VerdictMaxTokens, ct)
                      ?? RateThisMessage.FallbackVerdict;
        return DiscordResponseData.FromText(RateThisMessage.Format(author, message.Content, verdict));
    }
}
