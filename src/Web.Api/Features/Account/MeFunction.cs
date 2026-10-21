using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Account;

// GET /api/me: who the session token belongs to; 401 tells the site to show "Log in" again
public class MeFunction(ISessionTokens sessions)
{
    [Function("Me")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "me")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        return user is null ? new UnauthorizedResult() : new OkObjectResult(user);
    }
}
