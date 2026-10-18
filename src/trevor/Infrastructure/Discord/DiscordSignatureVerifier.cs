using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Math.EC.Rfc8032;

namespace trevor.Infrastructure.Discord;

// https://discord.com/developers/docs/interactions/overview#setting-up-an-endpoint-validating-security-request-headers
public class DiscordSignatureVerifier(IOptions<DiscordOptions> options, ILogger<DiscordSignatureVerifier> logger)
    : IDiscordSignatureVerifier
{
    private const int SignatureLength = 64;

    public bool Verify(byte[] body, string? signatureHex, string? timestamp)
    {
        var discord = options.Value;
        if (discord.SkipSignatureValidation)
        {
            logger.LogWarning("Discord signature validation is disabled (DISCORD_SKIP_SIGNATURE)");
            return true;
        }

        if (string.IsNullOrEmpty(signatureHex) || string.IsNullOrEmpty(timestamp))
        {
            logger.LogWarning("Missing Discord signature headers");
            return false;
        }

        byte[] signature;
        try
        {
            signature = Convert.FromHexString(signatureHex);
        }
        catch (FormatException)
        {
            logger.LogWarning("Discord signature is not valid hex");
            return false;
        }

        if (signature.Length != SignatureLength)
        {
            logger.LogWarning("Discord signature has wrong length");
            return false;
        }

        // Discord signs the raw bytes: timestamp followed by the request body
        var timestampBytes = Encoding.UTF8.GetBytes(timestamp);
        var message = new byte[timestampBytes.Length + body.Length];
        timestampBytes.CopyTo(message, 0);
        body.CopyTo(message, timestampBytes.Length);

        var publicKey = Convert.FromHexString(discord.PublicKey);
        var isValid = Ed25519.Verify(signature, 0, publicKey, 0, message, 0, message.Length);
        if (!isValid)
            logger.LogWarning("Invalid Discord signature");

        return isValid;
    }
}
