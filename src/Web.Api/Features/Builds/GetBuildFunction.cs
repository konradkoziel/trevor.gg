using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Builds;

// GET /api/builds/{id}: a shared build, readable by anyone with the link
public class GetBuildFunction(ISessionTokens sessions, IBuildStore store)
{
    [Function("GetBuild")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "builds/{id}")] HttpRequest req,
        string id,
        CancellationToken ct)
    {
        var build = BuildIds.IsWellFormed(id) ? await store.GetAsync(id, ct) : null;
        if (build is null)
            return new NotFoundResult();

        var user = await sessions.ReadAsync(req, ct);
        return new OkObjectResult(new BuildResponse(build.Id, build.OwnerName, user?.DiscordId == build.OwnerId, build.UpdatedAt, build.Build));
    }
}
