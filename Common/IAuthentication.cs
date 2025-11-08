using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trevor.Common
{
    public interface IAuthentication
    {
        bool VerifyRequest(string body, IHeaderDictionary headers, ILogger log);
    }
}
