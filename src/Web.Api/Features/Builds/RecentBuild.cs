namespace Web.Api.Features.Builds;

// One row of a list of the team's builds ("Najnowsze buildy ekipy", the builds of one champion)
public record RecentBuild(string Id, string Name, string ChampionId, string OwnerName, DateTimeOffset UpdatedAt)
{
    public static RecentBuild From(StoredBuild b) => new(b.Id, b.Build.Name, b.Build.ChampionId, b.OwnerName, b.UpdatedAt);
}
