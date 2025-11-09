using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Math.EC.Rfc8032;
using System.Text;

namespace trevor.Common
{
    public class Authentication : IAuthentication
    {
        public bool VerifyRequest(string body, HttpHeadersCollection headers, ILogger log)
        {
            var discordPublicKey = Environment.GetEnvironmentVariable("DISCORD_API_KEY");
            if (string.IsNullOrWhiteSpace(discordPublicKey))
            {
                log.LogError("Missing DISCORD_API_KEY.");
                return false;
            }
            
            if (!headers.TryGetValues("X-Signature-Ed25519", out var sigValues) ||
                !headers.TryGetValues("X-Signature-Timestamp", out var tsValues))
            {
                log.LogWarning("Missing Discord signature headers.");
                return false;
            }

            var signatureHeader = sigValues.FirstOrDefault();
            var timestampHeader = tsValues.FirstOrDefault();

            if (signatureHeader is null || timestampHeader is null)
            {
                log.LogWarning("Empty Discord signature headers.");
                return false;
            }

            try
            {
                var signature = Convert.FromHexString(signatureHeader);
                var publicKey = Convert.FromHexString(discordPublicKey);
                var message = Encoding.UTF8.GetBytes(timestampHeader + body);

                var isValid = Ed25519.Verify(signature, 0, publicKey, 0, message, 0, message.Length);
                if (!isValid)
                    log.LogWarning("Invalid Discord signature.");

                return isValid;
            }
            catch (Exception ex)
            {
                log.LogError($"Error verifying signature: {ex.Message}");
                return false;
            }
        }
    }
}
