namespace Web.Api.Features.Builds;

// A build as kept in storage, with who made it
public record StoredBuild(string Id, string OwnerId, string OwnerName, DateTimeOffset UpdatedAt, SavedBuild Build);
