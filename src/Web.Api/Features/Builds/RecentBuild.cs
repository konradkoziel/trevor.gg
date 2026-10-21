namespace Web.Api.Features.Builds;

// One row of "Najnowsze buildy ekipy" on the home page
public record RecentBuild(string Id, string Name, string ChampionId, string OwnerName, DateTimeOffset UpdatedAt);
