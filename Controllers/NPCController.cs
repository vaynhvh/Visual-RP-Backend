using Backend.Models;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Controllers
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class NPCController : Script
    {
        //[HandleExceptions]
        [ServerEvent(Event.PlayerConnected)]
        public async Task OnPlayerJoinAsync(RXPlayer player)
        {
            foreach (NPC npc in Resource.ServerNpcs)
            {
                await player.TriggerEventAsync("loadNpc", npc.PedHash, npc.Position.X, npc.Position.Y, npc.Position.Z, npc.Heading, npc.Dimension);
            }
        }
    }
}
