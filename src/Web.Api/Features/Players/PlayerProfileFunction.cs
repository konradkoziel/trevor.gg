using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Features.RiotAccount;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.Players;

// GET /api/players/{name}/{tag}: a linked friend's profile; their Riot ID and games, so logged-in users only
public class PlayerProfileFunction(ISessionTokens sessions, IRiotApiClient riot, PlayerProfiles profiles)
{
    public const string NotFound = "Nie znam gościa. Albo nie podpiął konta, albo go zmyśliłeś.";

    [Function("PlayerProfile")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "players/{name}/{tag}")] HttpRequest req,
        string name,
        string tag,
        CancellationToken ct)
    {
        if (await sessions.ReadAsync(req, ct) is null)
            return new UnauthorizedResult();

        var link = await profiles.FindAsync(name, tag, ct);
        if (link is null)
            return new NotFoundObjectResult(new ApiError(NotFound));

        return await RiotCalls.RunAsync(riot, async () => new OkObjectResult(await profiles.ForAsync(link, ct)));
    }
}
