using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Builds;

// DELETE /api/builds/{id}: only the owner may
public class DeleteBuildFunction(ISessionTokens sessions, IBuildStore store)
{
    [Function("DeleteBuild")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "builds/{id}")] HttpRequest req,
        string id,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var (existing, denied) = await OwnedBuild.LoadAsync(store, id, user, ct);
        if (denied is not null)
            return denied;

        await store.DeleteAsync(id, ct);
        return new NoContentResult();
    }
}
