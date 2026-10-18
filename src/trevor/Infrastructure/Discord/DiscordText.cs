namespace trevor.Infrastructure.Discord;

public static class DiscordText
{
    // Cuts text to a Discord length limit, marking the cut with an ellipsis
    public static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..(maxLength - 1)] + "…";

    // Markdown block quote; every line needs its own "> " or the quote ends after the first one
    public static string Quote(string text) =>
        string.Join("\n", text.Split('\n').Select(line => $"> {line}"));
}
