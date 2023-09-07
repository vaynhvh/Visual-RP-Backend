using Backend.Controllers;
using Backend.Models;
using Backend.Models.Appearance;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
using Backend.Modules.Vehicle;
using Backend.Modules.Workstation;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Vehicle
{
    class RXVehiclePassenger
    {
        [JsonProperty(PropertyName ="i")]
        public int seatid { get; set; }
        [JsonProperty(PropertyName = "s")]
        public bool used { get; set; } = false;
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class VehicleModule : RXModule
    {
        public VehicleModule() : base("Vehicle") { }

        //[HandleExceptions]
        public override async void LoadAsync()
        {
            RequireModule("Team");

            await Task.Delay(8000);

            using var db = new RXContext();

            var vehicles = await db.Vehicles.Where(x => x.Stored == false && x.ModelId != 0 && x.Position != "NULL" && x.Position != "" && x.Rotation != "" && x.Rotation != "NULL").ToListAsync();
            var teamVehicles = await db.TeamVehicles.Where(x => x.Stored == false && x.Position != "NULL" && x.Position != "" && x.Rotation != "" && x.Rotation != "NULL").ToListAsync();

            //await db.TeamVehicles.ForEachAsync(teamvehicle =>
            //{
           //    teamvehicle.Stored = true;
           // });

            await NAPI.Task.RunAsync(() =>
            {
                vehicles.ForEach(dbVehicle =>
                {

                    if (dbVehicle.Position == "0,0,0")
                    {
                        dbVehicle.Stored = true;
                        dbVehicle.GarageId = 15;
                        return;
                    }

                    /*if ((DateTime.Now - dbVehicle.LastMoved).TotalDays > 2)
                    {
                        dbVehicle.Stored = true;
                        dbVehicle.GarageId = 15;
                        return;
                    }*/

                    RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(dbVehicle.Hash), dbVehicle.Position.ToPos(), dbVehicle.Rotation.ToPos().Z, 0, 0, dbVehicle.Plate.ToUpper(), 255, true, true, 0);

                    vehicle.Id = dbVehicle.Id;
                    vehicle.ModelData = VehicleModelModule.VehicleModels.FirstOrDefault(x => x.Id == dbVehicle.ModelId);
                    vehicle.CustomPrimaryColor = new Color(dbVehicle.R, dbVehicle.G, dbVehicle.B);
                    vehicle.CustomSecondaryColor = new Color(dbVehicle.R, dbVehicle.G, dbVehicle.B);

                    vehicle.OwnerId = dbVehicle.OwnerId;
                    //vehicle.TeamId = dbVehicle.TeamId;
                    vehicle.ContainerId = dbVehicle.ContainerId;
                    vehicle.Plate = dbVehicle.Plate;
                    vehicle.Distance = dbVehicle.Distance;
                    vehicle.Fuel = dbVehicle.Fuel;
                    vehicle.Mods = new Dictionary<int, int>();
                    vehicle.Mods = JsonConvert.DeserializeObject<Dictionary<int, int>>(dbVehicle.Tuning);
                    vehicle.Registered = dbVehicle.Registered;
                    if (dbVehicle.VehicleKeys.IsValidJson<List<uint>>())
                        vehicle.VehicleKeys = JsonConvert.DeserializeObject<List<uint>>(dbVehicle.VehicleKeys);

                    vehicle.SetEngineStatus(false);
                    vehicle.SetLocked(true);
                });
            });

            await db.SaveChangesAsync();

            await NAPI.Task.RunAsync(() =>
            {
                teamVehicles.ForEach(dbVehicle =>
                {
                    var team = TeamModule.Teams.FirstOrDefault(x => x.Id == dbVehicle.TeamId);
                    if (team == null) return;

                    if (dbVehicle.Position == "0,0,0")
                    {
                        dbVehicle.Stored = true;
                        return;
                    }

                    RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(dbVehicle.Hash), dbVehicle.Position.ToPos(), dbVehicle.Rotation.ToPos().Z, 0, 0, team.ShortName, 255, true, true, 0);

                    vehicle.Id = dbVehicle.Id;
                    vehicle.ModelData = VehicleModelModule.VehicleModels.Find(x => x.Hash == dbVehicle.Hash);
                    vehicle.CustomPrimaryColor = new Color(dbVehicle.R, dbVehicle.G, dbVehicle.B);
                    vehicle.CustomSecondaryColor = new Color(dbVehicle.R, dbVehicle.G, dbVehicle.B);
                    vehicle.TeamId = dbVehicle.TeamId;
                    vehicle.TeamVehicleModel = dbVehicle.Hash;
                    vehicle.ContainerId = dbVehicle.ContainerId;
                    vehicle.Distance = dbVehicle.Distance;
                    vehicle.Fuel = dbVehicle.Fuel;
                    vehicle.Plate = "";
                    vehicle.OwnerId = 0;
                    vehicle.Mods = new Dictionary<int, int>();
                    vehicle.Mods = JsonConvert.DeserializeObject<Dictionary<int, int>>(dbVehicle.Tuning);
                    vehicle.RXLivery = dbVehicle.Livery;
                    vehicle.SetLocked(true);
                });
            });
        }

        /* public override async Task OnPlayerDeath(RXPlayer player, RXPlayer killer)
         {
             await NAPI.Task.RunAsync(() =>
             {
                 var vehicle = (RXVehicle)player.Vehicle;
                 if (vehicle == null) return;

                 vehicle.RemovePlayerFromOccupants(player);
             });
         }

         public override void OnPlayerDisconnect(RXPlayer player, DisconnectionType type, string reason)
         {
             lock (player)
             {
                 NAPI.Task.Run(() =>
                 {
                     lock (player)
                     {
                         var vehicle = (RXVehicle)player.Vehicle;
                         if (vehicle == null) return;

                         vehicle.RemovePlayerFromOccupants(player);
                     }
                 });
             }
         }*/

        //[HandleExceptions]
        public override async Task OnPlayerEnterVehicle(RXPlayer player, RXVehicle vehicle, sbyte seat)
        {
            await player.TriggerEventAsync("disableVehicleRadio");

            if (player == null || vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0) || vehicle.Id == 0 || await NAPI.Task.RunReturnAsync(() => !vehicle.HasSharedData("engineStatus"))) return;

            //vehicle.AddPlayerToVehicleOccupants(player, seat);

            if (seat == 0)
            {
                bool engine = await NAPI.Task.RunReturnAsync(() => vehicle.GetSharedData<bool>("engineStatus"));

                if (vehicle.ModelData != null)
                {
                    await player.TriggerEventAsync("setPlayerVehicleMultiplier", vehicle.ModelData.Multiplier);
                    await player.TriggerEventAsync("setNormalSpeed", vehicle, vehicle.ModelData.MaxKMH);
                }
                else
                {
                    await player.TriggerEventAsync("setPlayerVehicleMultiplier", 32);
                    await player.TriggerEventAsync("setNormalSpeed", vehicle, 300);
                }

                vehicle.LastDriver = await player.GetNameAsync();

                if (!engine) vehicle.SetEngineStatus(false);
            }

            float newVehicleHealth = await vehicle.GetHealthAsync() + await vehicle.GetBodyHealthAsync();
            await player.TriggerEventAsync("initialVehicleData", vehicle.Fuel.ToString().Replace(",", "."), (vehicle.ModelData == null ? 100 : vehicle.ModelData.Fuel).ToString().Replace(",", "."), newVehicleHealth.ToString().Replace(",", "."),
                2000.ToString().Replace(",", "."), (vehicle.ModelData == null ? 300 : await NAPI.Task.RunReturnAsync(() => vehicle.MaxSpeed)).ToString().Replace(",", "."), await NAPI.Task.RunReturnAsync(() => vehicle.Locked) ? "true" : "false", string.Format("{0:0.00}", vehicle.Distance).Replace(",", "."), await NAPI.Task.RunReturnAsync(() => vehicle.EngineStatus) ? "true" : "false");
        }

        //[HandleExceptions]
        public override async Task OnPlayerExitVehicle(RXPlayer player, RXVehicle vehicle)
        {
            if (player == null || vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0) || vehicle.Id == 0 || await NAPI.Task.RunReturnAsync(() => !vehicle.HasSharedData("engineStatus"))) return;

            if (await player.GetVehicleSeatAsync() == 0)
            {
                bool engine = await NAPI.Task.RunReturnAsync(() => vehicle.GetSharedData<bool>("engineStatus"));


                vehicle.SetEngineStatus(engine);
            }

            await NAPI.Task.RunAsync(() => NAPI.Player.SetPlayerCurrentWeapon(player, WeaponHash.Unarmed));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public void updateVehicleDistance(RXPlayer player, RXVehicle vehicle, double distance, double fuelDistance)
        {
            if (player == null || vehicle == null || !vehicle.Exists || !player.Exists) return;

            NAPI.Task.Run(() =>
            {
                if (vehicle.HasSharedData("ShopCar"))
                    player.WarpOutOfVehicle();
            });

            if (vehicle.ModelData == null && vehicle.TeamId == 0) return;

           // Logger.Debug(fuelDistance.ToString() + " - fueldistance");

            if (fuelDistance == 0) fuelDistance = 0.0008;

            vehicle.Distance += distance;
            var consumedFuel = (vehicle.ModelData == null ? 0.008f : vehicle.ModelData.FuelConsumption) * fuelDistance;
            vehicle.Fuel -= consumedFuel;
            if (vehicle.Fuel < 0) vehicle.Fuel = 0;

            var newFuel = vehicle.Fuel.ToString().Replace(",", ".");
            var newDistance = String.Format("{0:0.00}", vehicle.Distance).Replace(",", ".");
            var newVehicleHealth = NAPI.Vehicle.GetVehicleEngineHealth(vehicle) + NAPI.Vehicle.GetVehicleBodyHealth(vehicle);
            var newHealth = newVehicleHealth.ToString().Replace(",", ".");
            var newLockState = vehicle.Locked ? "true" : "false";

            player.TriggerEvent("updateVehicleData", newFuel, newDistance, newHealth, newLockState, vehicle.EngineStatus ? "true" : "false");
            player.TriggerEvent("setPlayerVehicleMultiplier", vehicle.ModelData == null ? 32 : vehicle.ModelData.Multiplier);

            if (vehicle.Fuel > 0.0) return;

            vehicle.SetEngineStatus(false);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Pressed_Num_3(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync() || await NAPI.Task.RunReturnAsync(() => player.VehicleSeat != 0 && player.VehicleSeat != 1) || !await player.CanInteractAntiFloodNoMSG(1)) return;

            await player.TriggerEventAsync("disableVehicleRadio");

            await new RXWindow("Radio").OpenWindow(player);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ToggleDoor(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            var vehicle = (RXVehicle)await NAPI.Task.RunReturnAsync(() => player.Vehicle);
            if (vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            if (vehicle.OwnerId != player.Id && !vehicle.HasPerm(player)) return;

            if (await NAPI.Task.RunReturnAsync(() => vehicle.Locked))
            {
                vehicle.SetLocked(false);
                await player.SendNotify("Fahrzeug aufgeschlossen!", 3500, "green", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
            }
            else
            {
                vehicle.SetLocked(true);
                await player.SendNotify("Fahrzeug zugeschlossen!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
            }

        }

        //Handbreak
        [RemoteEvent]
        public async Task Handbreak(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            var vehicle = (RXVehicle)await NAPI.Task.RunReturnAsync(() => player.Vehicle);
            if (vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            var vehicleSeat = await NAPI.Task.RunReturnAsync(() => player.VehicleSeat);
            if (vehicleSeat != 0) return;

            vehicle.Handbrake = !vehicle.Handbrake;
        }
        //[HandleExceptions]
        [RemoteEvent]
        public async Task ToggleEngine(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            var vehicle = (RXVehicle)await NAPI.Task.RunReturnAsync(() => player.Vehicle);
            if (vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            if (vehicle.OwnerId != player.Id && !vehicle.HasPerm(player)) return;

            var vehicleSeat = await NAPI.Task.RunReturnAsync(() => player.VehicleSeat);
            if (vehicleSeat != 0) return;

            if (vehicle.Fuel < 1 && !await NAPI.Task.RunReturnAsync(() => vehicle.EngineStatus))
            {
                await player.SendNotify("Dieses Fahrzeug hat kein Benzin mehr!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
                return;
            }

            if (await NAPI.Task.RunReturnAsync(() => vehicle.EngineStatus))
            {
                vehicle.SetEngineStatus(false);
                await player.SendNotify("Motor ausgeschaltet!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
            }
            else
            {
                vehicle.SetEngineStatus(true);
                await player.SendNotify("Motor eingeschaltet!", 3500, "green", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
            }

            await Task.Delay(1000);
            await player.TriggerEventAsync("disableVehicleRadio");
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task EjectExecute(RXPlayer player, int seatid)
        {
            if (!player.IsLoggedIn || seatid == 0 || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            var vehicle = await player.GetVehicleAsync();
            if (vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            var vehicleSeat = await player.GetVehicleSeatAsync();
            if (vehicleSeat != 0) return;

            foreach (Entity entity in vehicle.Occupants)
            {
                RXPlayer target = entity as RXPlayer;
                if (target == null) continue;

                if (await target.GetVehicleSeatAsync() == seatid)
                {
                    target.WarpOutOfVehicle();
                    await target.SendNotify("Du wurdest aus dem Fahrzeug rausgeworfen!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
                    await player.SendNotify("Spieler wurde aus dem Fahrzeug rausgeworfen!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
                    return;
                }
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Eject(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync()) return;

            var vehicle = (RXVehicle)await NAPI.Task.RunReturnAsync(() => player.Vehicle);
            if (vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            //if (vehicle.OwnerId != player.Id && !vehicle.HasPerm(player)) return;

            var vehicleSeat = await NAPI.Task.RunReturnAsync(() => player.VehicleSeat);
            if (vehicleSeat != 0) return;

            var ejectWindow = new RXWindow("Eject");

            List<RXVehiclePassenger> passengers = new List<RXVehiclePassenger>
            {
                new RXVehiclePassenger
                {
                    seatid = 0,
                    used = true
                }
            };

            NAPI.Task.Run(async () =>
            {
                vehicle.Occupants.OrderBy(x => ((RXPlayer)x).VehicleSeat).forEachAlternative(x =>
                {
                    if (x is RXPlayer target)
                    {
                        if (target.VehicleSeat != 0)
                        {
                            passengers.Add(new RXVehiclePassenger
                            {
                                seatid = target.VehicleSeat,
                                used = true
                            });
                        }
                    }
                });

                for (int i = 1; i < (vehicle.ModelData == null ? 4 : vehicle.ModelData.Seats); i++)
                {
                    if (passengers.FirstOrDefault(x => x.seatid == i) == null)
                    {
                        passengers.Add(new RXVehiclePassenger
                        {
                            seatid = i,
                            used = false
                        });
                    }
                }

                object dd = new
                {
                    d = passengers,
                };

                await ejectWindow.OpenWindow(player, dd);

            });
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ToggleDoorOutside(RXPlayer player, RXVehicle vehicle)
        {
            try
            {
                if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1) || vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

                if (vehicle.OwnerId != player.Id && !vehicle.HasPerm(player)) return;

                var positionPlayer = await NAPI.Task.RunReturnAsync(() => player.Position);
                var positionVehicle = await NAPI.Task.RunReturnAsync(() => vehicle.Position);

                if (positionPlayer.DistanceTo(positionVehicle) > 20f) return;

                if (await NAPI.Task.RunReturnAsync(() => vehicle.Locked))
                {
                    vehicle.SetLocked(false);
                    await player.SendNotify("Fahrzeug aufgeschlossen!", 3500, "green", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
                }
                else
                {
                    vehicle.SetLocked(true);
                    await player.SendNotify("Fahrzeug zugeschlossen!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
                }
                await player.PlayAnimationAsync(48, "anim@mp_player_intmenu@key_fob@", "fob_click_fp");

            }
            catch (Exception e) 
            {
                RXLogger.Print(e.Message);
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Repair(RXPlayer player, RXVehicle vehicle)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1) || vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            var positionPlayer = await NAPI.Task.RunReturnAsync(() => player.Position);
            var positionVehicle = await NAPI.Task.RunReturnAsync(() => vehicle.Position);

            if (positionPlayer.DistanceTo(positionVehicle) > 20f) return;

            var itemModel = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Werkzeugkasten");
            if (itemModel == null) return;

            if (player.Container.GetItemAmount(itemModel) < 1)
            {
                return;
            }

            await InventoryModule.useInventoryItem(player, player.Container.GetSlotOfSimilairSingleItems(itemModel));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task REQUEST_VEHICLE_INFORMATION(RXPlayer player, RXVehicle vehicle)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1) || vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            var positionPlayer = await NAPI.Task.RunReturnAsync(() => player.Position);
            var positionVehicle = await NAPI.Task.RunReturnAsync(() => vehicle.Position);

            if (positionPlayer.DistanceTo(positionVehicle) > 20f) return;

            var msg = "";

            msg += "Nummernschild: " + vehicle.Plate;
            msg += "Modell: " + (vehicle.ModelData == null ? vehicle.TeamVehicleModel : vehicle.ModelData.Name);
            msg += "Seriennummer: " + vehicle.Id;

            await player.SendNotify(msg, 10000, "DodgerBlue", "KFZ");
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task REQUEST_VEHICLE_TOGGLE_SEATBELT(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            var vehicle = (RXVehicle)await NAPI.Task.RunReturnAsync(() => player.Vehicle);
            if (vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            if (await NAPI.Task.RunReturnAsync(() => player.Seatbelt))
            {
                await NAPI.Task.RunAsync(() => player.Seatbelt = false);
                await player.SendNotify("Du hast dich abgeschnallt!", 3500, "red");
            }
            else
            {
                await NAPI.Task.RunAsync(() => player.Seatbelt = true);
                await player.SendNotify("Du hast dich angeschnallt!", 3500, "green");
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ToggleTrunkOutside(RXPlayer player, RXVehicle vehicle)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1) || vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            var positionPlayer = await NAPI.Task.RunReturnAsync(() => player.Position);
            var positionVehicle = await NAPI.Task.RunReturnAsync(() => vehicle.Position);

            if (positionPlayer.DistanceTo(positionVehicle) > 20f) return;

            if (await NAPI.Task.RunReturnAsync(() => vehicle.Locked))
            {
                await player.SendNotify("Fahrzeug zugeschlossen!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");

                return;
            }
            int door = 5;

            if (door == 5)
            {
                if (await NAPI.Task.RunReturnAsync(() => vehicle.TrunkOpen))
                {
                    await NAPI.Task.RunAsync(() => vehicle.TrunkOpen = false);
                    await player.SendNotify("Kofferraum zugeschlossen!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
                }
                else
                {
                    await NAPI.Task.RunAsync(() => vehicle.TrunkOpen = true);
                    await player.SendNotify("Kofferraum aufgeschlossen!", 3500, "green", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
                }
            }
            await player.PlayAnimationAsync(48, "anim@mp_player_intmenu@key_fob@", "fob_click_fp");

        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ToggleTrunk(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            int door = 5;
            var vehicle = (RXVehicle)await NAPI.Task.RunReturnAsync(() => player.Vehicle);
            if (vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            var vehicleSeat = await NAPI.Task.RunReturnAsync(() => player.VehicleSeat);
            if (vehicleSeat != 0) return;

            if (await NAPI.Task.RunReturnAsync(() => vehicle.Locked))
            {
                await player.SendNotify("Fahrzeug zugeschlossen!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");

                return;
            }

            if (door == 5)
            {
                if (await NAPI.Task.RunReturnAsync(() => vehicle.TrunkOpen))
                {
                    await NAPI.Task.RunAsync(() => vehicle.TrunkOpen = false);
                    await player.SendNotify("Kofferraum zugeschlossen!", 3500, "red", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
                }
                else
                {
                    await NAPI.Task.RunAsync(() => vehicle.TrunkOpen = true);
                    await player.SendNotify("Kofferraum aufgeschlossen!", 3500, "green", $"({vehicle.Id}) - ({vehicle.ModelData.Name})");
                }
            }

        }

        //[HandleExceptions]
        public override async Task PressedL(RXPlayer player)
        {
            try
            {
                if (await player.GetIsInVehicleAsync())
                {

                    await ToggleDoor(player);
                }
                else
                {
                    RXVehicle vehicle = await NAPI.Task.RunReturnAsync(() => VehicleController.GetClosestVehicle(player.Position));
                    if (vehicle == null) return;

                    await ToggleDoorOutside(player, vehicle);
                }

            } catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        //[HandleExceptions]
        public override async Task PressedK(RXPlayer player)
        {
            if (await player.GetIsInVehicleAsync())
                await ToggleTrunk(player);
            else
            {
                RXVehicle vehicle = await NAPI.Task.RunReturnAsync(() => VehicleController.GetClosestVehicle(player.Position));
                if (vehicle == null) return;

                await ToggleTrunkOutside(player, vehicle);
            }
        }
    }
}
