using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using trevor.Model;

namespace trevor
{
    public static class MessageResponse
    {
        public static ContentResult CreateResponse(Command cmd, DiscordInteraction interaction)
        {
            var cs = new CommandService(cmd, interaction);
            var response = cs.GetCommandResponse();


            var json = JsonSerializer.Serialize(new
            {
                type = 4,
                data = new
                {
                    content = response
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
    }
}
