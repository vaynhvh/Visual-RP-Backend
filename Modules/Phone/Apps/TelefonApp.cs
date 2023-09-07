using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Leitstellen;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    public class PhoneCall
    {
        public int Id { get; set; }
        public uint Player1 { get; set; }
        public uint Player2 { get; set; }
    }

    class TelefonApp : RXModule
    {
        public TelefonApp() : base("TelefonApp", new RXWindow("TelefonApp")) { }

        public static List<PhoneCall> Calls = new List<PhoneCall>();

        /*public static List<DbPhoneCallHistory> Histories = new List<DbPhoneCallHistory>();

        //[HandleExceptions]
        public override async Task OnTwoSecond()
        {
            using var db = new RXContext();

            List<DbPhoneCallHistory> copyPhoneCallHistories = new List<DbPhoneCallHistory>();

            TransferDBContextValues(await db.PhoneCallHistories.ToListAsync(), phoneCallHistory => copyPhoneCallHistories.Add(phoneCallHistory));

            Histories = copyPhoneCallHistories;
        }

        //[HandleExceptions]
        public async Task AddPhoneCallHistory(RXPlayer player, string number, string method)
        {
            using var db = new RXContext();

            await db.PhoneCallHistories.AddAsync(new DbPhoneCallHistory
            {
                PlayerId = player.Id,
                Number = number,
                Method = method,
                Time = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            });

            await db.SaveChangesAsync();
        }*/

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public static async Task CancelCall(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            var call = findCall(player);
            if (call == null) return;


            await player.TriggerEventAsync("CallApp", 0, 0);
            var target = await PlayerController.FindPlayerByPhoneNumber(player.Phone == call.Player1 ? call.Player2 : call.Player1);
            if (target == null) return;
            await target.TriggerEventAsync("CallApp", 0, 0);


            await player.TriggerEventAsync("setCallingPlayer", "");
            await target.TriggerEventAsync("setCallingPlayer", "");
            await player.TriggerEventAsync("SaltyChat_EndCall", target.Handle.Value);
            await target.TriggerEventAsync("SaltyChat_EndCall", player.Handle.Value);


            await NAPI.Task.RunAsync(() =>
            {
                target.ResetSharedData("InCall");
                player.ResetSharedData("InCall");
            });

            Calls.Remove(call);
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task AcceptCall(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            var call = findCall(player);
            if (call == null) return;

            await player.TriggerEventAsync("componentServerEvent", "CallManageApp", "acceptCall");

            var target = await PlayerController.FindPlayerByPhoneNumber(player.Phone == call.Player1 ? call.Player2 : call.Player1);
            if (target == null) return;

            await target.TriggerEventAsync("componentServerEvent", "CallManageApp", "acceptCall");

            await player.TriggerEventAsync("setCallingPlayer", target.VoiceHash);
            await target.TriggerEventAsync("setCallingPlayer", player.VoiceHash);

            await player.TriggerEventAsync("CallApp", target.Phone, 2);
            await target.TriggerEventAsync("CallApp", player.Phone, 2);
            await player.TriggerEventAsync("SaltyChat_EstablishedCall", target.Handle.Value);
            await target.TriggerEventAsync("SaltyChat_EstablishedCall", player.Handle.Value);
            await target.TriggerEventAsync("VoiceStop", "phoneDialtone");
            await player.TriggerEventAsync("VoiceStop", "phoneDialtone");

            await NAPI.Task.RunAsync(() =>
            {
                target.SetSharedData("InCall", player.Name);
                player.SetSharedData("InCall", target.Name);
            });
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task Call(RXPlayer player, uint number)
        {
            if (!player.CanInteract() || !await player.CanInteractAntiFloodNoMSG(2) || number == player.Phone) return;

            if (player.PhoneSettings != null && player.PhoneSettings.FlyMode)
            {
                await player.SendNotify("Anrufen nicht möglich, du bist im Flugmodus!");
                return;
            }
            RXPlayer target = null;

            TeamLeitstellenObject teamLeitstellenObject = LeitstellenModule.GetByAcceptor(player);

            if (LeitstellenModule.TeamNumberPhones.ContainsKey((int)number))
            {
                teamLeitstellenObject = LeitstellenModule.GetLeitstelleByNumber((int)number);

                if (teamLeitstellenObject == null || teamLeitstellenObject.Acceptor == null)
                {
                    await player.SendNotify("Diese Leitstelle ist derzeit nicht erreichbar!");
                    await player.TriggerEventAsync("VoiceSound", "phoneBusytone", false, "phoneSound");
                    return;
                }
     

                target = teamLeitstellenObject.Acceptor;            
            }
            if (teamLeitstellenObject == null)
            {

                target = await PlayerController.FindPlayerByPhoneNumber(number);
                if (target == null || (target != null && target.PhoneSettings != null && target.PhoneSettings.FlyMode))
                {
                    await player.SendNotify("Die angegebene Rufnummer ist derzeit nicht erreichbar!");
                    await player.TriggerEventAsync("VoiceSound", "phoneBusytone", false, "phoneSound");
                    return;
                }

                if (target.PhoneSettings != null && target.PhoneSettings.DenyCalls)
                {
                    await player.SendNotify("Die angegebene Rufnummer hat eingehende Anrufe blockiert!");
                    await player.TriggerEventAsync("VoiceSound", "phoneBusytone", false, "phoneSound");
                    return;
                }
            }

            if (findCall(target) != null)
            {
                await player.SendNotify("Die angegebene Rufnummer ist derzeit im Gespräch!");
                await player.TriggerEventAsync("VoiceSound", "phoneBusytone", false, "phoneSound");
                return;
            }

            PhoneCall call = new PhoneCall
            {
                Id = Calls.Count,
                Player1 = player.Phone,
                Player2 = target.Phone
            };

            Calls.Add(call);

            if (teamLeitstellenObject == null)
            {

                await player.TriggerEventAsync("CallApp", target.Phone, 0);

                await target.TriggerEventAsync("CallApp", player.Phone, 1);

            }
            else
            {

                await player.TriggerEventAsync("CallApp", target.Phone, 0);

                await target.TriggerEventAsync("CallApp", teamLeitstellenObject.Number, 1);
            }
        }

        public static PhoneCall findCall(RXPlayer player)
        {
            PhoneCall phoneCall = Calls.FirstOrDefault(call => call.Player1 == player.Phone || call.Player2 == player.Phone);

            return phoneCall;
        }
    }
}
