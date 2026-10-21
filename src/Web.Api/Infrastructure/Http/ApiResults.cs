using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Infrastructure.Http;

public static class ApiResults
{
    // Game data changes once per patch, so browsers may keep it for an hour
    private const string StaticDataCacheControl = "public, max-age=3600";

    // 200 with browser caching, or 503 when the data source (Data Dragon) gave nothing
    public static IActionResult StaticData(HttpRequest req, object? data)
    {
        if (data is null)
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

        req.HttpContext.Response.Headers.CacheControl = StaticDataCacheControl;
        return new OkObjectResult(data);
    }
}
