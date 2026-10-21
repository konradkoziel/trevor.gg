namespace Web.Api.Features.Builds;

// DataUnavailable: Data Dragon is down, so the build can't be checked (503); Error: what's wrong with it (400)
public record BuildValidation(bool DataUnavailable, string? Error)
{
    public static readonly BuildValidation Ok = new(false, null);
    public static readonly BuildValidation Unavailable = new(true, null);

    public static BuildValidation Invalid(string error) => new(false, error);
}
