using Backend.Models;
using Backend.Modules.Discord;
using Backend.Utils;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Controllers
{
    ////[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class CommandController : Script
    {
        //[HandleExceptions]
        [RemoteEvent]
        public async Task PlayerChat(RXPlayer player, string input)
        {
            if (!player.IsLoggedIn) return;

            if (string.IsNullOrEmpty(input)) return;

            IEnumerable<MethodInfo> commands = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass)
                .SelectMany(x => x.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(RXCommand), false).FirstOrDefault() != null);

            RXLogger.Print((await player.GetNameAsync()) + " " + input);

            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " " + input, DiscordModule.CommandWebhook));

            string[] array = input.Replace(input.Split(" ")[0] + " ", "").Split(" ");

            MethodInfo method = commands.FirstOrDefault(m => m.GetCustomAttributes(typeof(RXCommand), false) != null && m.GetCustomAttributes(typeof(RXCommand), false).Length > 0 && ((RXCommand)m.GetCustomAttributes(typeof(RXCommand), false)[0]) != null && ((RXCommand)m.GetCustomAttributes(typeof(RXCommand), false)[0]).Name.ToLower() == input.Split(" ")[0].Replace(" ", "").Replace("/", "").ToLower() && ((RXCommand)m.GetCustomAttributes(typeof(RXCommand), false)[0]).Permission <= player.Rank.Permission);
            if (method == null) return;

            object instance = Activator.CreateInstance(method.DeclaringType);

            object[] parameters =
            {
                player,
                array
            };

            method.Invoke(instance, parameters);
        }
    }
}
