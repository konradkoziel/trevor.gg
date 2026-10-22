namespace Web.Api.Features.Builds;

// One row of "Moje buildy" and of a player's profile
public record BuildSummary(string Id, string Name, string ChampionId, DateTimeOffset UpdatedAt)
{
    public static BuildSummary Of(StoredBuild build) => new(build.Id, build.Build.Name, build.Build.ChampionId, build.UpdatedAt);
}
