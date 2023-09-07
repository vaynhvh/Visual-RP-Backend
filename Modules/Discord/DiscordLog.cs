using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Modules.Discord
{
    class DiscordLog
    {
        public DateTime Time { get; set; } = DateTime.Now;

        public string Webhook { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public DiscordLog(string title, string message, string webhook = "https://discord.com/api/webhooks/1142591276332630076/T38bs62F7-oxOwP6OelbQFnBYCWYAO3oo87Obg3s5hDam6yFgWTZsBmLGwhIBy3YnyIR")
        {
            Webhook = webhook;
            Message = message;
            Title = title;
        }
    }
}
