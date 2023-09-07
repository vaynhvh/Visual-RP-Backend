using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Native;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Minijobs.Mower
{
    class MowerModule : RXModule
    {
        public MowerModule() : base("Mower") { }

        public static int MowerJobVehMarkId = 20;
        public static Vector3 MowerGetPoint = new Vector3(-949.348, 332.97, 71.3311);
        public static Vector3 MowerSpawnPoint = new Vector3(-938.013, 329.984, 70.8813);
        public static float MowerSpawnRotation = 267.621f;
        public static Vector3 MowerMowPoint = new Vector3(-980.331, 318.863, 70.0861);
        public static List<RXPlayer> PlayersInJob = new List<RXPlayer>();

        public override async void LoadAsync()
        {
            var mcb = await NAPI.Entity.CreateMCB(MowerGetPoint, new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.VerticalCylinder, false);

            mcb.ColShape.Message = new RXMessage
            {
                Text = "Benutze E um den Minijob zu verwalten!",
                Color = "green",
                Duration = 3500,
                Title = "Gärtnerei"
            };

            mcb.ColShape.Action = async player => await StartMowing(player);
        }

        public override async Task OnTenSecond()
        {
            foreach (var iPlayer in PlayersInJob.ToList())
            {
                var vehicle = (RXVehicle)await NAPI.Task.RunReturnAsync(() => iPlayer.Vehicle);
                uint vehmodel = 0;
                if (vehicle != null)
                {
                    vehmodel = await vehicle.GetModelAsync();
                }

                if (vehmodel == 0) continue;
                if (await iPlayer.GetIsInVehicleAsync() && vehicle.HasData("loadage") && vehmodel == (uint)VehicleHash.Mower)
                {
                  
                    var playerpos = await iPlayer.GetPositionAsync();
                    if (await vehicle.GetSpeed(iPlayer) > 5.0f && playerpos.DistanceTo(MowerMowPoint) < 30.0f)
                    {
                        if (iPlayer.HasData("lastRasenPoint"))
                        {
                            if (iPlayer.GetData<Vector3>("lastRasenPoint").DistanceTo(playerpos) < 4.0f) continue; //Anti Kreisfahren
                        }
                        iPlayer.SetData("lastRasenPoint", playerpos);

                        Random random = new Random();
                        int rnd = random.Next(4, 9);
                        vehicle.SetData("loadage", (vehicle.GetData<int>("loadage") + rnd));
                        await iPlayer.SendNotify($"Du hast Rasen gemäht! Es befinden sich nun " + vehicle.GetData<int>("loadage") + "kg in deinem Rasenmäher!");
                    }
                }
            }
        }

        public static async Task StartMowing(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;
            var nativeMenu = new NativeMenu("Minijob", "Gärtnerei", new List<NativeItem>()
            {
                new NativeItem("Schließen", player => player.CloseNativeMenu()),
                new NativeItem("Minijob beginnen", async player => {
                    player.CloseNativeMenu();

                    if (PlayersInJob.Contains(player))
                    {

                        await player.SendNotify("Du arbeitest bereits!");

                        return;
                    }

            await NAPI.Task.RunAsync(async () =>
            {

                RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(VehicleHash.Mower, MowerSpawnPoint, MowerSpawnRotation, 0, 0);//await MP.Vehicles.NewAsync((int)hash, await player.Player.GetPositionAsync());
                vehicle.NumberPlate = "KOKSAL";
                vehicle.SetSharedData("engineStatus", true);
                vehicle.SetSharedData("lockedStatus", false);
                vehicle.JobId = 20;
                vehicle.OwnerId = player.Id;
                vehicle.SetData("loadage", 0);



                await Task.Delay(100);

                await NAPI.Task.RunAsync(() => player.SetIntoVehicle(vehicle, 0));

                MinijobHandler.JobVehicles.Add(vehicle);
                await player.SendNotify("Du arbeitest nun als Gärtner! Fahre mit deinem Fahrzeug auf der Wiese!");
                PlayersInJob.Add(player);
                });

                    }),
                new NativeItem("Minijob beenden", async player => {
                    player.CloseNativeMenu();
                    RXVehicle sxVehicle = MinijobHandler.GetJobVehicle(player, MowerModule.MowerJobVehMarkId);
                        if(sxVehicle != null)
                        {
                            int loadage = sxVehicle.GetData<int>("loadage");
                            int verdienst = loadage * 10;

                            
                            await player.GiveMoney(verdienst);
                                        await player.SendNotify("Du hast " + verdienst + " $ verdient! Viel Spaß damit!");

                            PlayersInJob.Remove(player);

                        await MinijobHandler.RemoveJobVehicleIfExist(player);
                        }


                }),
            });


            player.ShowNativeMenu(nativeMenu);
        }

    }
}
