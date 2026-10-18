using trevor.Infrastructure.Discord;

namespace trevor.Features.RateThis;

// Text of the "Rate this" reply: quoted message plus Trevor's verdict, AI input and canned lines
public static class RateThisMessage
{
    // A long message would push the verdict past Discord's 2000-char limit; quote only its start
    private const int QuoteMaxLength = 500;

    public const string UnknownAuthor = "Ktoś";
    public const string NothingToRate = "Tu nie ma żadnego tekstu. Mam oceniać obrazek? Nie jestem twoją mamą.";

    // Used when the AI is unavailable
    public const string FallbackVerdict = "Przeczytałem. Żałuję. 2/10, i to z litości.";

    public static string Format(string author, string content, string verdict) =>
        $"💬 **{author}:**\n{DiscordText.Quote(DiscordText.Truncate(content, QuoteMaxLength))}\n{verdict}";

    public static string AiInput(string author, string content) => $"Autor: {author}\nWiadomość:\n{content}";
}
