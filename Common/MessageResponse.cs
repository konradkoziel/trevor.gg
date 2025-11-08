using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using trevor.Commands;
using trevor.Model;

namespace trevor.Common
{
    public static class MessageResponse
    {
        public static ContentResult CreateResponse(string content)
        {
            var json = JsonSerializer.Serialize(new
            {
                type = 4,
                data = new
                {
                    content
                },
                statusCode = 200
            });
            return new ContentResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        public static ContentResult CreateDeferredResponse()
        {
            var json = JsonSerializer.Serialize(new { type = 5 });
            return new ContentResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = 200
            };
        }
    }
}
