using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// POST /api/me/riot/verify: is the challenge icon set now? Then the account is the user's
public class VerifyRiotLinkFunction(ISessionTokens sessions, IRiotApiClient riot, IRiotLinkStore store, TimeProvider time)
{
    [Function("VerifyRiotLink")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "me/riot/verify")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var challenge = await store.GetChallengeAsync(user.DiscordId, ct);
        if (challenge is null)
            return new BadRequestObjectResult(new ApiError(RiotAccountMessages.NoChallenge));
        if (challenge.ExpiresAt <= time.GetUtcNow())
            return new BadRequestObjectResult(new ApiError(RiotAccountMessages.ChallengeExpired));

        return await RiotCalls.RunAsync(riot, async () =>
        {
            var summoner = await riot.GetSummonerAsync(challenge.Puuid, ct);
            if (summoner?.ProfileIconId != challenge.IconId)
                return new BadRequestObjectResult(new ApiError(RiotAccountMessages.WrongIcon));

            await store.CompleteAsync(new RiotLink(user.DiscordId, challenge.Puuid, challenge.RiotId, time.GetUtcNow()), ct);
            return new OkObjectResult(new RiotAccountView(challenge.RiotId, null));
        });
    }
}
