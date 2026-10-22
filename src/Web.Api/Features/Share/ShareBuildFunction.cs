using Lol.Shared.Riot;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using Web.Api.Features.Bot;
using Web.Api.Features.Builds;

namespace Web.Api.Features.Share;

// GET /api/share/{id}: the link "Kopiuj link" copies. Discord shows a preview of the build, people land on /b/{id}
public class ShareBuildFunction(IOptions<ShareOptions> options, IBuildStore store, IDataDragon dataDragon, IDataDragonCatalog catalog)
{
    // Long enough to spare Data Dragon when a link is posted around, short enough that an edit shows up soon
    private const string CacheControl = "public, max-age=300";

    [Function("ShareBuild")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "share/{id}")] HttpRequest req,
        string id,
        CancellationToken ct)
    {
        var siteUrl = options.Value.SiteUrl;
        var stored = BuildIds.IsWellFormed(id) ? await store.GetAsync(id, ct) : null;
        if (stored is null)
            return new RedirectResult(BuildCards.Url(siteUrl, id));

        var champion = (await dataDragon.ChampionsAsync(ct))?.FirstOrDefault(c => c.Id == stored.Build.ChampionId);
        var data = await BuildDataSource.LoadAsync(catalog, ct);

        req.HttpContext.Response.Headers.CacheControl = CacheControl;
        return new ContentResult
        {
            Content = LinkPreviewPage.Html(BuildPreviews.Create(stored, champion, data, siteUrl)),
            ContentType = "text/html; charset=utf-8",
            StatusCode = StatusCodes.Status200OK,
        };
    }
}
