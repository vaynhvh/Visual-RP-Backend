using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Crime;
using Backend.Modules.Discord;
using Backend.Modules.Faction;
using Backend.Modules.Gangwar;
using Backend.Modules.Laptop.Apps;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Backend.Modules.Staatsfraktionen.DPOS
{
    class DPOSModule : RXModule
    {
        public DPOSModule() : base("DPOSModule") { }

        public static List<Vector3> dpos = new List<Vector3>();

        public override async void LoadAsync()
        {
            dpos.Add(new Vector3(-434.009, 6136, 31.478));
            dpos.Add(new Vector3(1674.38, 3823.05, 34.342));
            dpos.Add(new Vector3(714.628, -1383.5, 26.229));
            dpos.Add(new Vector3(-793.599, -1501.04, -0.090427));
            dpos.Add(new Vector3(-3156.39, 1131.1, 20.8485));
            dpos.Add(new Vector3(2904.5, 4383.5, 50.2662));
            dpos.Add(new Vector3(400.954, -1632.14, 29.292));
            dpos.Add(new Vector3(-1610.51, -818.22, 9.89718));

            foreach (var pos in dpos)
            {
                var mcb = await NAPI.Entity.CreateMCB(pos, new Color(255, 140, 0), 0u, 2f, 2f, false, MarkerType.UpsideDownCone);

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um ein Fahrzeug in den Impound zu schicken",
                    Color = "green",
                    Duration = 3500,
                    Title = "DPOS",
                    RestrictedToTeam = 6,
                };

               

                mcb.ColShape.Action = async player => await OpenImpound(player, pos);
            }
        }

        [RemoteEvent]
        public async Task SetVehicleImpound(RXPlayer player, string grund, uint vehicleid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;


            var veh = VehicleController.GetValidVehicles().Find(x => x.Id == vehicleid);
            using var db = new RXContext();
            if (veh == null) return;

            var dbveh = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleid);

            if (dbveh == null) return;

            dbveh.Stored = true;
            dbveh.GarageId = 15;
                 
            await veh.DeleteAsync();
            await db.SaveChangesAsync();

            DiscordModule.Logs.Add(new DiscordLog("DPOS", (await player.GetNameAsync()) + " hat das Fahrzeug mit der ID " + vehicleid + " mit der Begründung: " + grund + " in den Impound geliefert.", "https://discord.com/api/webhooks/1142591911501246514/H2ns56TZPfbCzDu1p7JGtDdUNTKt9ZnW520usWEtT7QuuCfzsMiHYKrBqON_gHA9k7v7"));

            await player.SendNotify("Du hast erfolgreich das Fahrzeug in den Impound geschickt!");
            await player.GiveMoney(1000);
        }

        [RemoteEvent]
        public async Task SetVehicleImpoundTeam(RXPlayer player, string grund, uint vehicleid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;


            var veh = VehicleController.GetValidVehiclesIncludeTeam().Find(x => x.Id == vehicleid);
            using var db = new RXContext();
            if (veh == null) return;

            var dbveh = await db.TeamVehicles.FirstOrDefaultAsync(x => x.Id == vehicleid);

            if (dbveh == null) return;

            dbveh.Stored = true;

            await veh.DeleteAsync();
            await db.SaveChangesAsync();

            DiscordModule.Logs.Add(new DiscordLog("DPOS", (await player.GetNameAsync()) + " hat das Fahrzeug mit der ID " + vehicleid + " mit der Begründung: " + grund + " in den Impound geliefert.", "https://canary.discord.com/api/webhooks/1142047452585791600/rlrcoriTBxh1zJgjqRSo9PoolpdRIhi2lHDBBkXMcqAySYizjMYfXrIXHOBMlOkfh5dz"));

            await player.SendNotify("Du hast erfolgreich das Fahrzeug in den Impound geschickt!");
            await player.GiveMoney(1000);

        }

        public async Task OpenImpound(RXPlayer player, Vector3 pos)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;

            if (player.Team.Id != 6)
            {
                return;
            }

            var veh = VehicleController.GetClosestVehicle(pos, 10f, 0);

            if (veh == null) return;

            if (veh.TeamId == 0)
            {


                object confirmationBoxObject = new
                {
                    t = "Beschlagnahmungsgrund?",
                    e = "SetVehicleImpound",
                    d = veh.Id,
                };

                var confirmation = new RXWindow("Input");

                await confirmation.OpenWindow(player, confirmationBoxObject);


            } else
            {

                object confirmationBoxObject = new
                {
                    t = "Beschlagnahmungsgrund?",
                    e = "SetVehicleImpoundTeam",
                    d = veh.Id,
                };

                var confirmation = new RXWindow("Input");

                await confirmation.OpenWindow(player, confirmationBoxObject);
            }



        }
    }
}
