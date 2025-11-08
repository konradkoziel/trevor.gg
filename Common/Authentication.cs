using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Math.EC.Rfc8032;
using System;
using System.Text;

namespace trevor.Common
{


    public class Authentication : IAuthentication
    {
        public bool VerifyRequest(string body, IHeaderDictionary headers, ILogger log)
        {
            var discordPublicKey = Environment.GetEnvironmentVariable("DISCORD_API_KEY");
            if (string.IsNullOrWhiteSpace(discordPublicKey))
            {
                log.LogError("Missing DISCORD_API_KEY.");
                return false;
            }

            if (!headers.TryGetValue("X-Signature-Ed25519", out var signatureHeader) ||
                !headers.TryGetValue("X-Signature-Timestamp", out var timestampHeader))
            {
                log.LogWarning("Missing Discord signature headers.");
                return false;
            }

            var signature = Convert.FromHexString(signatureHeader);
            var publicKey = Convert.FromHexString(discordPublicKey);
            var timestamp = timestampHeader.ToString();

            var message = Encoding.UTF8.GetBytes(timestamp + body);
            bool isValid = Ed25519.Verify(signature, 0, publicKey, 0, message, 0, message.Length);

            if (!isValid)
                log.LogWarning("Invalid Discord signature.");

            return isValid;
        }

        public Task<bool> VerifyRequestAsync(string body, IHeaderDictionary headers, ILogger log)
        {
            throw new NotImplementedException();
        }
    }
}
