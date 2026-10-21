using System.Text.Json;
using Lol.Shared.Riot;
using Lol.Shared.Riot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Web.Api.Infrastructure.Http;
using Web.Api.Infrastructure.Sessions;

namespace Web.Api.Features.RiotAccount;

// POST /api/me/riot/challenge: look the account up and pick the icon the user has to set
public class StartRiotLinkFunction(
    ISessionTokens sessions, IRiotApiClient riot, IRiotLinkStore store, IDataDragon dataDragon, TimeProvider time, Random random)
{
    // Time to open the client, change the icon and come back
    public static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(15);

    [Function("StartRiotLink")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "me/riot/challenge")] HttpRequest req,
        CancellationToken ct)
    {
        var user = await sessions.ReadAsync(req, ct);
        if (user is null)
            return new UnauthorizedResult();

        var body = await ReadAsync(req, ct);
        if (!RiotId.TryParse(body?.RiotId, out var riotId))
            return new BadRequestObjectResult(new ApiError(RiotAccountMessages.BadRiotId));

        return await RiotCalls.RunAsync(riot, async () =>
        {
            var account = await riot.GetAccountAsync(riotId, ct);
            if (account is null)
                return new BadRequestObjectResult(new ApiError(RiotAccountMessages.NoSuchAccount));
            var summoner = await riot.GetSummonerAsync(account.Puuid, ct);
            if (summoner is null)
                return new BadRequestObjectResult(new ApiError(RiotAccountMessages.NeverPlayed));

            var name = RiotNames.RiotId(account) ?? riotId.ToString();
            var challenge = new RiotLinkChallenge(user.DiscordId, account.Puuid, name,
                VerificationIcons.Pick(summoner.ProfileIconId, random), time.GetUtcNow() + ChallengeLifetime);
            await store.SaveChallengeAsync(challenge, ct);
            return new OkObjectResult(await RiotAccountViews.ChallengeAsync(challenge, dataDragon, ct));
        });
    }

    private static async Task<StartRiotLinkRequest?> ReadAsync(HttpRequest req, CancellationToken ct)
    {
        try
        {
            return await req.ReadFromJsonAsync<StartRiotLinkRequest>(ct);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            return null;
        }
    }
}
