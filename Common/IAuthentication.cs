using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;

namespace trevor.Common
{
    public interface IAuthentication
    {
        bool VerifyRequest(string body, HttpHeadersCollection headers, ILogger log);
    }
}
