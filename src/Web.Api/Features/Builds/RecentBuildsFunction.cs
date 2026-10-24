using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Builds;

// GET /api/recent-builds: the newest builds of everyone, for logged-in users
// (not /api/builds/recent, which would read like the id "recent")
public class RecentBuildsFunction(ISessionTokens sessions, IBuildStore store)
{
    public const int Count = 6;

    [Function("RecentBuilds")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recent-builds")] HttpRequest req,
        CancellationToken ct)
    {
        if (await sessions.ReadAsync(req, ct) is null)
            return new UnauthorizedResult();

        var builds = await store.ListRecentAsync(Count, ct);
        return new OkObjectResult(builds.Select(RecentBuild.From).ToList());
    }
}
