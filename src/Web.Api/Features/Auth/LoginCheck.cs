namespace Web.Api.Features.Auth;

// Valid: the state came back as sent. WasSilent: it was the attempt without the consent screen
public record LoginCheck(bool Valid, bool WasSilent);
