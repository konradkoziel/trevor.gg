namespace Web.Api.Features.Share;

// What a chat app shows for a shared link: Open Graph title, description and image, plus where the page sends people
public record LinkPreview(string Title, string Description, string? ImageUrl, string TargetUrl);
