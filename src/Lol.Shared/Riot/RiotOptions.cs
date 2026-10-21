namespace Lol.Shared.Riot;

// Env vars RIOT_API_KEY (optional — LoL commands say so when it's missing) and RIOT_PLATFORM
public class RiotOptions
{
    public string? ApiKey { get; set; }

    // Platform routing value of the server the players are on, e.g. eun1 (EUNE), euw1 (EUW)
    public string Platform { get; set; } = "eun1";

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);
}
