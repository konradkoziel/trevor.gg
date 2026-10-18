namespace trevor.Infrastructure.Discord;

public class DiscordOptions
{
    // Application public key (hex) from the Developer Portal, env var DISCORD_API_KEY
    public string PublicKey { get; set; } = string.Empty;

    // Webhook for the channel with tracked players' match reports, env var DISCORD_URL_MATCHES
    public string? MatchesWebhookUrl { get; set; }

    // Local testing only, env var DISCORD_SKIP_SIGNATURE=true
    public bool SkipSignatureValidation { get; set; }

    public bool HasValidPublicKey =>
        PublicKey.Length == 64 && PublicKey.All(Uri.IsHexDigit);
}
