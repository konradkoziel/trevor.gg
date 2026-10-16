using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using trevor.Infrastructure.Discord;

namespace trevor.Tests;

public class DiscordSignatureVerifierTests
{
    private readonly byte[] _privateKey = new byte[Ed25519.SecretKeySize];
    private readonly byte[] _publicKey = new byte[Ed25519.PublicKeySize];

    public DiscordSignatureVerifierTests()
    {
        Ed25519.GeneratePrivateKey(new SecureRandom(), _privateKey);
        Ed25519.GeneratePublicKey(_privateKey, 0, _publicKey, 0);
    }

    private DiscordSignatureVerifier Verifier(bool skip = false) =>
        new(Options.Create(new DiscordOptions
            {
                PublicKey = Convert.ToHexString(_publicKey),
                SkipSignatureValidation = skip
            }),
            NullLogger<DiscordSignatureVerifier>.Instance);

    private string Sign(string timestamp, byte[] body)
    {
        var message = Encoding.UTF8.GetBytes(timestamp).Concat(body).ToArray();
        var signature = new byte[Ed25519.SignatureSize];
        Ed25519.Sign(_privateKey, 0, message, 0, message.Length, signature, 0);
        return Convert.ToHexString(signature).ToLowerInvariant();
    }

    [Fact]
    public void ValidSignature_IsAccepted()
    {
        var body = Encoding.UTF8.GetBytes("""{"type":1,"note":"zażółć"}""");

        Assert.True(Verifier().Verify(body, Sign("1700000000", body), "1700000000"));
    }

    [Fact]
    public void TamperedBody_IsRejected()
    {
        var body = Encoding.UTF8.GetBytes("""{"type":1}""");
        var signature = Sign("1700000000", body);

        Assert.False(Verifier().Verify(Encoding.UTF8.GetBytes("""{"type":2}"""), signature, "1700000000"));
    }

    [Fact]
    public void WrongTimestamp_IsRejected()
    {
        var body = Encoding.UTF8.GetBytes("""{"type":1}""");

        Assert.False(Verifier().Verify(body, Sign("1700000000", body), "1700000001"));
    }

    [Theory]
    [InlineData(null, "1700000000")]
    [InlineData("abc", null)]
    [InlineData("not-hex", "1700000000")]
    [InlineData("abcd", "1700000000")]
    public void MissingOrMalformedHeaders_AreRejected(string? signature, string? timestamp)
    {
        Assert.False(Verifier().Verify("{}"u8.ToArray(), signature, timestamp));
    }

    [Fact]
    public void SkipFlag_AcceptsAnything()
    {
        Assert.True(Verifier(skip: true).Verify("{}"u8.ToArray(), null, null));
    }
}
