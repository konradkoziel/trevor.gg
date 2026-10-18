namespace trevor.Infrastructure.Riot.Models;

// Riot ID, e.g. "Nick#EUNE" → GameName "Nick", TagLine "EUNE"
public record RiotId(string GameName, string TagLine)
{
    public static bool TryParse(string? text, out RiotId id)
    {
        id = new RiotId(string.Empty, string.Empty);
        var value = text?.Trim();
        var hash = value?.LastIndexOf('#') ?? -1;
        if (value is null || hash <= 0 || hash == value.Length - 1)
            return false;

        id = new RiotId(value[..hash].Trim(), value[(hash + 1)..].Trim());
        return id.GameName.Length > 0 && id.TagLine.Length > 0;
    }

    public override string ToString() => $"{GameName}#{TagLine}";
}
