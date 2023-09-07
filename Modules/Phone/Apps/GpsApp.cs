using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Bank;
using Backend.Modules.Blitzer;
using Backend.Modules.Vehicle;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    public class GPSCategory
    {
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "d")]
        public List<GPSPosition> Locations { get; set; }
        public GPSCategory(string name, List<GPSPosition> pos)
        {
            Name = name;
            Locations = pos;
        }
    }

    public class GPSPosition
    {
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "x")]
        public float X { get; set; }

        [JsonProperty(PropertyName = "y")]
        public float Y { get; set; }

        public GPSPosition(string name, Vector3 pos)
        {
            Name = name;
            X = pos.X;
            Y = pos.Y;
        }
    }

    class GpsApp : RXModule
    {
        public GpsApp() : base("GpsApp", new RXWindow("GpsLocationList")) { }

        public static List<GPSCategory> gpsCategories = new List<GPSCategory>()
            {
                new GPSCategory("Banken", new List<GPSPosition>()),
                new GPSCategory("Farming", new List<GPSPosition>()),
                new GPSCategory("Workstations", new List<GPSPosition>()),
                new GPSCategory("Vehicleshops", new List<GPSPosition>())
            };

        public override void LoadAsync()
        {

            var banks = gpsCategories.FirstOrDefault(x => x.Name == "Banken");
            if (banks == null) return;

            foreach (MainBank bank in BankModule.Banks)
            {
                banks.Locations.Add(new GPSPosition(bank.Name == "" ? "Pacific Staatsbank" : bank.Name, bank.Position));
            }

     
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task GetNavigationPublic(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            var list = gpsCategories.ToList();

            await player.TriggerEventAsync("SendNavigationPublic", NAPI.Util.ToJson(list));

        }

        [RemoteEvent]
        public async Task saveRadarSetting(RXPlayer player, bool radarActive)
        { 
            if (!player.CanInteract()) return;

            player.PhoneSettings.RadarActive = radarActive;

            using var db = new RXContext();

            var settings = await db.PhoneSettings.FirstOrDefaultAsync(x => x.PlayerId == player.Id);
            if (settings == null) return;

            settings.RadarActive = radarActive;
         
            await db.SaveChangesAsync();
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task GetNavigationVehicles(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            List<GPSCategory> categories = new List<GPSCategory>();

            var privateVehicles = new GPSCategory("Persönliche Fahrzeuge", new List<GPSPosition>());
            var teamVehicles = new GPSCategory("Fraktion", new List<GPSPosition>());

            var vehicles = await NAPI.Task.RunReturnAsync(() => VehicleController.GetValidVehiclesIncludeTeam().Where(x => x.HasPerm(player)).ToList());

            foreach (var vehicle in vehicles)
            {
                if (vehicle.ModelData != null)
                {
                    privateVehicles.Locations.Add(new GPSPosition("(" + vehicle.Plate + ") (" + vehicle.Id + ") " + vehicle.ModelData.Name, await NAPI.Task.RunReturnAsync(() => vehicle.Position)));
                }
                else if (vehicle.ModelData == null && player.TeamId != 0 && player.Team != null && vehicle.TeamId == player.TeamId)
                {
                    teamVehicles.Locations.Add(new GPSPosition("(" + player.Team.ShortName + ") (" + vehicle.Id + ") " + vehicle.TeamVehicleModel, await NAPI.Task.RunReturnAsync(() => vehicle.Position)));
                }
            }

            if (privateVehicles.Locations.Count > 0) categories.Add(privateVehicles);

            if (player.TeamId != 0 && player.Team != null && teamVehicles.Locations.Count > 0) categories.Add(teamVehicles);

            await player.TriggerEventAsync("SendNavigationVehicles", NAPI.Util.ToJson(categories));
        }
    }
}
