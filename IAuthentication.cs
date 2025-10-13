using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trevor
{
    public interface IAuthentication
    {
        public Task<bool> VerifyRequestAsync(HttpRequest req, ILogger log);
    }
}
