using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Builds;

// GET /api/me/builds: the logged-in user's builds, newest first
public class MyBuildsFunction(ISessionTokens sessions, IBuildStore store)
{
    [Function("MyBuilds")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me/builds")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var builds = await store.ListByOwnerAsync(user.DiscordId, ct);
        return new OkObjectResult(builds.Select(b => new BuildSummary(b.Id, b.Build.Name, b.Build.ChampionId, b.UpdatedAt)).ToList());
    }
}
