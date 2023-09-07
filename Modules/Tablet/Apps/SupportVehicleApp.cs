using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Garage;
using Backend.MySql;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Tablet.Apps
{
    class SupportVehicleApp : RXModule
    {
        public SupportVehicleApp() : base("SupportApp") { }

        [RemoteEvent]
        public async Task SupportSetGarage(RXPlayer player, string vehicleIdStr)
        {
            if (!player.CanInteract() || string.IsNullOrEmpty(vehicleIdStr) || !player.InAduty) return;

            if (!uint.TryParse(vehicleIdStr, out uint vehicleId)) return;

            using var db = new RXContext();

            var dbVehicle = db.Vehicles.FirstOrDefault(x => x.Id == vehicleId);
            if (dbVehicle == null)
            {
                await player.SendNotify("Fahrzeug nicht gefunden.", 3500);
                return;
            }

            if (dbVehicle.Stored)
            {
                await player.SendNotify("Fahrzeug bereits eingeparkt!", 3500);
                return;
            }

            dbVehicle.Stored = true;

            await db.SaveChangesAsync();

            await NAPI.Task.RunAsync(() =>
            {
                var vehicle = VehicleController.FindVehicleById(vehicleId);
                if (vehicle != null)
                {
                    vehicle.Occupants.forEachAlternative(o =>
                    {
                        if (o is RXPlayer)
                        {
                            var target = (RXPlayer)o;

                            target.WarpOutOfVehicle();
                        }
                    });

                    vehicle.Delete();
                }
            });

            await player.SendNotify("Fahrzeug eingeparkt!");
        }

        [RemoteEvent]
        public async Task SupportGoToVehicle(RXPlayer player, string vehicleIdStr)
        {
            if (!player.CanInteract() || string.IsNullOrEmpty(vehicleIdStr) || !player.InAduty) return;

            if (!uint.TryParse(vehicleIdStr, out uint vehicleId)) return;

            await NAPI.Task.RunAsync(async () =>
            {
                var vehicle = VehicleController.FindVehicleById(vehicleId);
                if (vehicle != null)
                {
                    player.Position = vehicle.Position;

                    await player.SendNotify("Zum Fahrzeug teleportiert!");
                }
                else
                {
                    await player.SendNotify("Fahrzeug nicht gefunden!");
                }
            });
        }

        [RemoteEvent]
        public async Task requestVehicleData(RXPlayer player, string vehicleIdStr)
        {
            if (!player.CanInteract() || string.IsNullOrEmpty(vehicleIdStr) || !player.InAduty) return;

            if (!uint.TryParse(vehicleIdStr, out uint vehicleId)) return;

            using var db = new RXContext();

            var dbVehicle = db.Vehicles.FirstOrDefault(x => x.Id == vehicleId);
            if (dbVehicle == null)
            {
                await player.SendNotify("Fahrzeug nicht gefunden.", 3500);
                return;
            }

            RXGarage garage = GarageModule.Garages.FirstOrDefault(x => x.Id == dbVehicle.GarageId);
            if (garage == null)
            {
                await player.SendNotify("Fahrzeuggarage nicht gefunden.", 3500);
                garage = GarageModule.Garages.FirstOrDefault();
            }

            dynamic vehicleData = new
            {
                id = vehicleId,
                vehiclehash = dbVehicle.Hash,
                inGarage = dbVehicle.Stored,
                garage = garage.Name
            };

            var window = new RXWindow("SupportVehicleProfile");

            await window.TriggerEvent(player, "responseVehicleData", JsonConvert.SerializeObject(new List<dynamic>() { vehicleData }));
        }

        [RemoteEvent]
        public async Task requestSupportVehicleList(RXPlayer player, string playerIdStr)
        {
            if (!player.CanInteract() || string.IsNullOrEmpty(playerIdStr) || !player.InAduty) return;

            if (!uint.TryParse(playerIdStr, out uint playerId)) return;

            using var db = new RXContext();

            var vehicles = db.Vehicles.Where(x => x.OwnerId == playerId);
            if (vehicles == null || vehicles.Count() < 1) return;

            List<dynamic> clientVehicles = new List<dynamic>();

            await vehicles.forEach(vehicle =>
            {
                RXGarage garage = GarageModule.Garages.FirstOrDefault(x => x.Id == vehicle.GarageId);
                if (garage == null) garage = GarageModule.Garages.FirstOrDefault();

                dynamic vehicleData = new
                {
                    id = vehicle.Id,
                    vehiclehash = vehicle.Hash,
                    inGarage = vehicle.Stored,
                    garage = garage.Name
                };

                clientVehicles.Add(vehicleData);
            });

            var window = new RXWindow("SupportVehicleList");

            await window.TriggerEvent(player, "responseVehicleList", JsonConvert.SerializeObject(clientVehicles));
        }
    }
}
