using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Web.Api.Features.Share;

// A tiny HTML page: Discord's crawler reads the meta tags (it runs no JavaScript, so the React site can't give it any),
// people are sent straight on to the site
public static class LinkPreviewPage
{
    public const string SiteName = "Trevor.gg";

    // The site's gold, used by Discord for the embed's side bar
    private const string ThemeColor = "#c8aa6e";

    // Polish letters stay readable in the page source; <, >, & and quotes are still encoded
    private static readonly HtmlEncoder Encoder = HtmlEncoder.Create(UnicodeRanges.All);

    public static string Html(LinkPreview preview)
    {
        var e = Encoder;
        var title = e.Encode(preview.Title);
        var description = e.Encode(preview.Description);
        var target = e.Encode(preview.TargetUrl);
        var image = preview.ImageUrl is null
            ? ""
            : $"""
              <meta property="og:image" content="{e.Encode(preview.ImageUrl)}">
              <meta name="twitter:card" content="summary_large_image">
              """;

        return $"""
            <!doctype html>
            <html lang="pl">
            <head>
            <meta charset="utf-8">
            <title>{title}</title>
            <meta property="og:site_name" content="{SiteName}">
            <meta property="og:type" content="website">
            <meta property="og:title" content="{title}">
            <meta property="og:description" content="{description}">
            <meta property="og:url" content="{target}">
            <meta name="description" content="{description}">
            <meta name="theme-color" content="{ThemeColor}">
            {image}
            <link rel="canonical" href="{target}">
            <meta http-equiv="refresh" content="0; url={target}">
            </head>
            <body><a href="{target}">{title}</a></body>
            </html>
            """;
    }
}
