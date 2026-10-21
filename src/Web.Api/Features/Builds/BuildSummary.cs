namespace Web.Api.Features.Builds;

// One row of "Moje buildy"
public record BuildSummary(string Id, string Name, string ChampionId, DateTimeOffset UpdatedAt);
