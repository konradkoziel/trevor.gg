namespace Web.Api.Features.Matches;

// An item, summoner spell or rune shown in a match: its name for the tooltip and its icon
public record MatchIcon(string Name, string? IconUrl);
