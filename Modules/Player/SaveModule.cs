using Backend.Controllers;
using Backend.Models;
using Backend.MySql;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;
//using PostSharp.Patterns.Diagnostics;
//using PostSharp.Extensibility;

namespace Backend.Modules.Player
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class SaveModule : RXModule
    {
        public SaveModule() : base("Save") { }

        //[HandleExceptions]
        public override Task OnTenMinute()
        {
            string file = $"C:\\Backups\\{ DateTime.Now.ToString("dd-MM-yyyy HH-mm") }.sql";
            string connection = Configuration.ConnectionString;

            connection += "charset=utf8;convertzerodatetime=true;";

            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                using (MySqlCommand cmd = new MySqlCommand()) 
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = conn;
                        conn.Open();
                        mb.ExportToFile(file);
                        conn.Close();
                    }
                }
            }

            RXLogger.Print("Backup Database...", LogType.SUCCESS);

            return Task.CompletedTask;
        }

        //[HandleExceptions]
        public async override Task OnTenSecond()
        {
            if (!Configuration.DevMode)
                NAPI.Task.Run(() => NAPI.World.SetTime(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));

            using var db = new RXContext();

            foreach (var player in PlayerController.GetValidPlayers())
            {
                var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
                if (dbPlayer == null) continue;

                if (await player.GetDimensionAsync() == 0)
                    dbPlayer.Position = (await player.GetPositionAsync()).FromPos();

                dbPlayer.Level = player.Level;
                dbPlayer.Paytime = player.Paytime;
                dbPlayer.InDuty = player.InDuty;
                dbPlayer.Sport = player.Sport;
                dbPlayer.Thirst = player.Thirst;
                dbPlayer.Hunger = player.Hunger;
                dbPlayer.Stress = player.Stress;
                dbPlayer.HP = await player.GetHealthAsync();
                dbPlayer.Armor = await player.GetArmorAsync();
                dbPlayer.HouseId = player.HouseId;
                dbPlayer.LuckyWheel = player.LuckyWheel;
                dbPlayer.Blackmoney = player.Blackmoney;
                dbPlayer.Warns = player.Warns;
                dbPlayer.WorkstationId = player.WorkstationId;
                dbPlayer.Jailtime = player.Jailtime;
                dbPlayer.FunkFav = JsonConvert.SerializeObject(player.FunkFav);
                dbPlayer.Storages = JsonConvert.SerializeObject(player.Storages);
                dbPlayer.WalletAdress = player.WalletAdress;
                dbPlayer.LastSeen = DateTime.Now;
                dbPlayer.WalletValue = player.WalletValue;
                dbPlayer.Weapons = NAPI.Util.ToJson(player.Weapons);
            }

            foreach (var vehicle in VehicleController.GetValidVehicles())
            {

                var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id);
                if (dbVehicle == null) continue;

                var position = await NAPI.Task.RunReturnAsync(() => vehicle.Position);
                var rotation = await NAPI.Task.RunReturnAsync(() => vehicle.Rotation);
                var locked = await NAPI.Task.RunReturnAsync(() => vehicle.Locked);
                var primaryColor = await NAPI.Task.RunReturnAsync(() => vehicle.PrimaryColor);
                var secondaryColor = await NAPI.Task.RunReturnAsync(() => vehicle.SecondaryColor);
                var fuel = vehicle.Fuel;
                var distance = vehicle.Distance;

                dbVehicle.Position = position.FromPos();
                dbVehicle.Rotation = rotation.FromPos();
                dbVehicle.R = await NAPI.Task.RunReturnAsync(() => vehicle.CustomPrimaryColor.Red);
                dbVehicle.G = await NAPI.Task.RunReturnAsync(() => vehicle.CustomPrimaryColor.Green);
                dbVehicle.B = await NAPI.Task.RunReturnAsync(() => vehicle.CustomPrimaryColor.Blue);

                if (vehicle.Mods == null)
                {
                    vehicle.Mods = new Dictionary<int, int>();
                }
                dbVehicle.Tuning = JsonConvert.SerializeObject(vehicle.Mods);
                dbVehicle.Fuel = fuel;
                dbVehicle.Distance = distance;

            }

            foreach (var vehicle in VehicleController.GetValidVehiclesIncludeTeam())
                {
                    var dbVehicle = await db.TeamVehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id);
                    if (dbVehicle == null) continue;

                    var position = await NAPI.Task.RunReturnAsync(() => vehicle.Position);
                    var rotation = await NAPI.Task.RunReturnAsync(() => vehicle.Rotation);
                    var locked = await NAPI.Task.RunReturnAsync(() => vehicle.Locked);
                    var primaryColor = await NAPI.Task.RunReturnAsync(() => vehicle.PrimaryColor);
                    var secondaryColor = await NAPI.Task.RunReturnAsync(() => vehicle.SecondaryColor);
                    var fuel = vehicle.Fuel;
                    var distance = vehicle.Distance;

                    dbVehicle.Position = position.FromPos();
                    dbVehicle.Rotation = rotation.FromPos();
                dbVehicle.R = await NAPI.Task.RunReturnAsync(() => vehicle.CustomPrimaryColor.Red);
                dbVehicle.G = await NAPI.Task.RunReturnAsync(() => vehicle.CustomPrimaryColor.Green);
                dbVehicle.B = await NAPI.Task.RunReturnAsync(() => vehicle.CustomPrimaryColor.Blue);

                if (vehicle.Mods == null)
                    {
                        vehicle.Mods = new Dictionary<int, int>();
                    }
                    dbVehicle.Tuning = JsonConvert.SerializeObject(vehicle.Mods);
                    dbVehicle.Fuel = fuel;
                    dbVehicle.Distance = distance;
                }
            

            await db.SaveChangesAsync();
        }
    }
}
