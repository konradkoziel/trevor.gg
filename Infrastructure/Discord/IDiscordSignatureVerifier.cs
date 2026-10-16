namespace trevor.Infrastructure.Discord;

public interface IDiscordSignatureVerifier
{
    bool Verify(byte[] body, string? signatureHex, string? timestamp);
}
