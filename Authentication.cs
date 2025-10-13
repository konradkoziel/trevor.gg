using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Math.EC.Rfc8032;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trevor
{
    public class Authentication : IAuthentication
    {

        public async Task<bool> VerifyRequestAsync(HttpRequest req, ILogger log)
        {

            var DiscordPublicKey = Environment.GetEnvironmentVariable("DISCORD_API_KEY");
            if (!req.Headers.TryGetValue("X-Signature-Ed25519", out var signatureHeader) ||
                !req.Headers.TryGetValue("X-Signature-Timestamp", out var timestampHeader))
            {
                log.LogWarning("Missing Discord signature headers.");
                return false;
            }
            string signatureHex = signatureHeader.ToString();
            string timestamp = timestampHeader.ToString();
            string body;
            using (var reader = new StreamReader(req.Body))
            {
                body = await reader.ReadToEndAsync();
            }
            byte[] message = Encoding.UTF8.GetBytes(timestamp + body);
            byte[] signature = Convert.FromHexString(signatureHex);
            byte[] publicKey = Convert.FromHexString(DiscordPublicKey);
            bool isValid = Ed25519.Verify(signature, 0, publicKey, 0, message, 0, message.Length);
            if (!isValid)
            {
                log.LogWarning("Invalid Discord signature.");
                return false;
            }
            return true;
        }
    }
}
