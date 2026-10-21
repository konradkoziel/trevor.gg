using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Builds;

// PUT /api/builds/{id}: change a build; only its owner may
public class UpdateBuildFunction(ISessionTokens sessions, BuildValidator validator, IBuildStore store, TimeProvider time)
{
    [Function("UpdateBuild")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "builds/{id}")] HttpRequest req,
        string id,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var (existing, denied) = await OwnedBuild.LoadAsync(store, id, user, ct);
        if (denied is not null)
            return denied;

        var (build, rejection) = await BuildRequest.ReadAsync(req, validator, ct);
        if (rejection is not null)
            return rejection;

        await store.SaveAsync(existing! with { OwnerName = user.Name, UpdatedAt = time.GetUtcNow(), Build = build! }, ct);
        return new OkObjectResult(new BuildCreated(id));
    }
}
