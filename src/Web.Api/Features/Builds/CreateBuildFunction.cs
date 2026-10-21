using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Builds;

// POST /api/builds: save a new build of the logged-in user
public class CreateBuildFunction(ISessionTokens sessions, BuildValidator validator, IBuildStore store, TimeProvider time)
{
    // Plenty for one person, and it keeps the "my builds" scan small
    public const int MaxBuildsPerUser = 50;

    [Function("CreateBuild")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "builds")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var (build, rejection) = await BuildRequest.ReadAsync(req, validator, ct);
        if (rejection is not null)
            return rejection;

        if ((await store.ListByOwnerAsync(user.DiscordId, ct)).Count >= MaxBuildsPerUser)
            return new BadRequestObjectResult(new ApiError(BuildMessages.TooManyBuilds));

        var id = BuildIds.New();
        await store.SaveAsync(new StoredBuild(id, user.DiscordId, user.Name, time.GetUtcNow(), build!), ct);
        return new ObjectResult(new BuildCreated(id)) { StatusCode = StatusCodes.Status201Created };
    }
}
