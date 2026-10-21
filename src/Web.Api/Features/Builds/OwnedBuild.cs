using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Builds;

// Shared by update and delete: the build must exist and belong to the logged-in user
public static class OwnedBuild
{
    public static async Task<(StoredBuild? Build, IActionResult? Rejection)> LoadAsync(
        IBuildStore store, string id, SessionUser user, CancellationToken ct)
    {
        var existing = BuildIds.IsWellFormed(id) ? await store.GetAsync(id, ct) : null;
        if (existing is null)
            return (null, new NotFoundResult());
        if (existing.OwnerId != user.DiscordId)
            return (null, new ObjectResult(new ApiError(BuildMessages.NotYours)) { StatusCode = StatusCodes.Status403Forbidden });
        return (existing, null);
    }
}
