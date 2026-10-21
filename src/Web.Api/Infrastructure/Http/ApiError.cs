namespace Web.Api.Infrastructure.Http;

// Body of a 4xx the site shows to the user as it is
public record ApiError(string Error);
