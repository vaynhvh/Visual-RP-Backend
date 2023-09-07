using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Crime;
using Backend.Modules.Native;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Backend.Modules.Laptop.Apps
{

    public class RestrictedZoneObject
    {
        [JsonProperty("i")]
        public int Id { get; set; }

        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("d")]
        public string Datum { get; set; }

        [JsonProperty("pt")]
        public string pt { get; set; }

        [JsonProperty("ut")]
        public string ut { get; set; }

        [JsonProperty("dt")]
        public string dt { get; set; }

        [JsonProperty("r")]
        public int radius { get; set; }

        [JsonProperty("o")]
        public string Officer { get; set; }

        [JsonIgnore]
        public Vector3 ZonePos { get; set; }

    }

    class PoliceEditPersonApp : RXModule
    {

        public PoliceEditPersonApp() : base("PoliceEditPersonApp", new RXWindow("PoliceEditPersonApp")) { }


        public static List<RestrictedZoneObject> sperrzonen = new List<RestrictedZoneObject>();

        [RemoteEvent]
        public async Task savePersonData(RXPlayer dbPlayer, string playername, string address, string membership, string phone, string info)
        {
            if (dbPlayer == null) return;

            if (!dbPlayer.Team.IsState())
            {
                await dbPlayer.SendNotify("Keine Berechtigung!");
                return;
            }

            RXPlayer foundPlayer = await PlayerController.FindPlayerByName(playername);
            if (foundPlayer == null) return;

            foundPlayer.PlayerCrimeData.Address = address;
            foundPlayer.PlayerCrimeData.Membership = membership;
            foundPlayer.PlayerCrimeData.Phone = phone;
            foundPlayer.PlayerCrimeData.Note = info;

            using var db = new RXContext();

            var settings = await db.PlayerCrimeData.FirstOrDefaultAsync(x => x.PlayerId == foundPlayer.Id);
            if (settings == null) return;

            settings.Address = address;
            settings.Membership = membership;
            settings.Phone = phone;
            settings.Note = info;

            await db.SaveChangesAsync();
        }

        [RemoteEvent]
        public async Task requestPersonData(RXPlayer dbPlayer, string p_Name)
        {
            if (dbPlayer == null) return;

            var foundPlayer = await PlayerController.FindPlayerByName(p_Name);
            if (foundPlayer == null) return;

            await this.Window.TriggerEvent(dbPlayer, "responsePersonData", NAPI.Util.ToJson(foundPlayer.PlayerCrimeData));
        }

        [RemoteEvent]
        public async Task GetPolPlayerWanteds(RXPlayer p_Player, int p_ID)
        {
            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == p_ID);
            if (dbPlayer == null) return;

            var l_Crimes = await db.PlayerCrimes.Where(x => x.PlayerId == p_ID).ToListAsync();
            List<PlayerActiveCrimes> l_List = new List<PlayerActiveCrimes>();

            try
            {
                foreach (var l_Reason in l_Crimes)
                {
                    var crime = await db.NewCrimes.FirstOrDefaultAsync(x => x.i == l_Reason.CrimeId);

                    if (crime == null) continue;

                    var officer = "-";
                    if (l_Reason.OfficerId == 0)
                    {
                        officer = "Leistelle [BLITZER]";
                    }
                    else
                    {
                        var target = await db.Players.FirstOrDefaultAsync(x => x.Id == l_Reason.OfficerId);
                        officer = target.Username;
                    }

                    l_List.Add(new PlayerActiveCrimes() { Id = (int)crime.i, Name = crime.n, Costs = crime.p, Jailtime = crime.j, Officer = officer, Date = l_Reason.Uhrzeit });
                }
                var l_Json = NAPI.Util.ToJson(l_List);
                await p_Player.TriggerEventAsync("SendPolPlayerWanteds", l_Json);
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        [RemoteEvent]
        public async Task PublishZone(RXPlayer dbPlayer, string zd, string zp, string zu, string zdelete, int radius)
        {
            if (dbPlayer == null) return;

            if (!dbPlayer.Team.IsState()) return;

            var zone = new RestrictedZoneObject() { Id = sperrzonen.Count + 1, Datum = DateTime.Now.ToString("dd\\/MM\\/yyyy h\\:mm"), ZonePos = await dbPlayer.GetPositionAsync(), pt = zp, dt = zdelete, ut = zu, Name = zd, radius = radius, Officer = await dbPlayer.GetNameAsync() };

            var players = PlayerController.GetPlayers();

            

            foreach (var hund in players)
            {
                await hund.TriggerEventAsync("createZoneBlip", zone.Id, zone.ZonePos, zone.Name, zone.radius);
            }
            RX.SendGlobalNotifyToAll(zone.pt, 8000, "red", Icon.LSPD);
            sperrzonen.Add(zone);
            await dbPlayer.TriggerEventAsync("RsRestrictedZones", NAPI.Util.ToJson(sperrzonen));

        }

        [RemoteEvent]
        public async Task RemoveAllRestrictedZones(RXPlayer dbPlayer)
        {
            if (dbPlayer == null) return;

            if (!dbPlayer.Team.IsState()) return;


            sperrzonen = new List<RestrictedZoneObject>();
            RX.SendGlobalNotifyToAll("Es wurden alle Sperrzonen aufgehoben!", 8000, "red", Icon.LSPD);

            var players = PlayerController.GetPlayers();

            foreach (var hund in players)
            {
                await hund.TriggerEventAsync("DeleteAllZoneRsBlips");
            }
            await dbPlayer.TriggerEventAsync("RsRestrictedZones", NAPI.Util.ToJson(sperrzonen));

        }

        [RemoteEvent]
        public async Task RemoveRestrictedZone(RXPlayer dbPlayer, int zoneid)
        {
            if (dbPlayer == null) return;

            if (!dbPlayer.Team.IsState()) return;

            var zone = sperrzonen.Find(x => x.Id == zoneid);
            if (zone == null) return;   

            RX.SendGlobalNotifyToAll(zone.dt, 8000, "red", Icon.LSPD);
            var players = PlayerController.GetPlayers();

            foreach (var hund in players)
            {
                await hund.TriggerEventAsync("DeleteZoneRsBlips", zone.Id);
            }
            sperrzonen.Remove(zone);
            await dbPlayer.TriggerEventAsync("RsRestrictedZones", NAPI.Util.ToJson(sperrzonen));

        }

        

        public static async Task loadAllBlips(RXPlayer player)
        {
            foreach (RestrictedZoneObject zone in sperrzonen)
            {

                await player.TriggerEventAsync("createZoneBlip", zone.Id, zone.ZonePos, zone.Name, zone.radius);

            }
        }

        [RemoteEvent]
        public async Task RqRestrictedZones(RXPlayer p_Player)
        {
            await p_Player.TriggerEventAsync("RsRestrictedZones", NAPI.Util.ToJson(sperrzonen));
        }
        [RemoteEvent]
        public async Task requestJailCosts(RXPlayer p_Player, string p_Name)
        {
            var dbPlayer = await PlayerController.FindPlayerByName(p_Name);
            if (dbPlayer == null) return;

            await this.Window.TriggerEvent(p_Player, "responseJailCosts", CrimeModule.CalcJailCosts(dbPlayer).ToString());
        }

        [RemoteEvent]
        public async Task requestJailTime(RXPlayer p_Player, string p_Name)
        {
            try
            {
                var dbPlayer = await PlayerController.FindPlayerByName(p_Name);
                if (dbPlayer == null) return;


                await this.Window.TriggerEvent(p_Player, "responseJailTime", CrimeModule.CalcJailTime(dbPlayer).ToString());
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }
    }
}
