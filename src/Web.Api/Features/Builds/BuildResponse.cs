namespace Web.Api.Features.Builds;

// GET /api/builds/{id}: anyone with the link may look; IsMine lets the site offer editing.
// The owner's Discord id stays on the server, only their name is shown.
public record BuildResponse(string Id, string OwnerName, bool IsMine, DateTimeOffset UpdatedAt, SavedBuild Build);
