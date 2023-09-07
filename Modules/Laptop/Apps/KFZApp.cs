using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Garage;
using Backend.Modules.Vehicle;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Laptop.Apps
{
    class KFZApp : RXModule
    {
        public KFZApp() : base("KFZApp", new RXWindow("FahrzeugUebersichtApp")) { }

        [RemoteEvent]
        public async Task OpenVehOver(RXPlayer player)
        {
            if (!player.IsLoggedIn) return;

            var window = new RXWindow("VehicleOverview");

            using var db = new RXContext();

            List<OverviewVehicle> overviewVehicles = new List<OverviewVehicle>();

            var vehicles = await NAPI.Task.RunReturnAsync(() => VehicleController.GetValidVehicles().Where(x => x.HasPerm(player)).ToList());

            List<DbVehicle> dbVehicles = await db.Vehicles.Where(x => x.Stored == true && x.OwnerId == player.Id).ToListAsync();

            foreach (var vehicle in vehicles)
            {
                if (vehicle.ModelData != null)
                {
                    var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id);
                    if (dbVehicle == null) continue;

                    RXGarage garage = GarageModule.Garages.FirstOrDefault(x => x.Id == dbVehicle.GarageId);
                    if (garage == null) continue;

                    overviewVehicles.Add(new OverviewVehicle
                    {
                        Id = vehicle.Id,
                        Vehiclehash = vehicle.ModelData.Name,
                        GarageName = garage.Name,
                        Notiz = "-",
                        InGarage = false,
                        CarCor = await NAPI.Task.RunReturnAsync(() => new CarCoordinate
                        {
                            position_x = vehicle.Position.X,
                            position_y = vehicle.Position.Y,
                            position_z = vehicle.Position.Z,
                        })
                    });
                }
            }

            foreach (var dbVehicle in dbVehicles)
            {
                RXGarage garage = GarageModule.Garages.FirstOrDefault(x => x.Id == dbVehicle.GarageId);
                if (garage == null) continue;

                var model = VehicleModelModule.VehicleModels.FirstOrDefault(x => x.Id == dbVehicle.ModelId);
                if (model == null) continue;

                overviewVehicles.Add(new OverviewVehicle
                {
                    Id = dbVehicle.Id,
                    Vehiclehash = model.Name,
                    GarageName = garage.Name,
                    InGarage = true,
                    Notiz = "-",
                    CarCor = new CarCoordinate
                    {
                        position_x = garage.Position.X,
                        position_y = garage.Position.Y,
                        position_z = garage.Position.Z,
                    }
                });
            }



            object veh = new
            {
                i = 0,
                data = overviewVehicles,
            };

            await window.OpenWindow(player, veh);

        }

        [RemoteEvent]
        public async Task GetOverviewVehicles(RXPlayer player, uint side)
        {
            if (!player.IsLoggedIn) return;

            var window = new RXWindow("VehicleOverview");

            using var db = new RXContext();

            List<OverviewVehicle> overviewVehicles = new List<OverviewVehicle>();

            
            var vehicles = await NAPI.Task.RunReturnAsync(() => VehicleController.GetValidVehicles().Where(x => x.HasPerm(player)).ToList());
            var frakvehicles = await NAPI.Task.RunReturnAsync(() => VehicleController.GetValidVehiclesIncludeTeam().Where(x => x.TeamId == player.Team.Id).ToList());

            List<DbVehicle> dbVehicles = await db.Vehicles.Where(x => x.Stored == true && x.OwnerId == player.Id).ToListAsync();
            List<DbTeamVehicle> dbTeamVehicles = await db.TeamVehicles.Where(x => x.Stored == true && x.TeamId == player.Team.Id).ToListAsync();

            if (side == 1)
            {
                foreach (var vehicle in vehicles)
                {
                    if (vehicle.ModelData != null)
                    {
                        var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id);
                        if (dbVehicle == null) continue;

                        RXGarage garage = GarageModule.Garages.FirstOrDefault(x => x.Id == dbVehicle.GarageId);
                        if (garage == null) continue;


                        if (dbVehicle.VehicleKeys.IsValidJson<List<uint>>())
                            vehicle.VehicleKeys = JsonConvert.DeserializeObject<List<uint>>(dbVehicle.VehicleKeys);

                        if (!vehicle.VehicleKeys.Contains(player.Id)) continue;

                        overviewVehicles.Add(new OverviewVehicle
                        {
                            Id = vehicle.Id,
                            Vehiclehash = vehicle.ModelData.Name,
                            GarageName = garage.Name,
                            Notiz = "-",
                            InGarage = false,
                            CarCor = await NAPI.Task.RunReturnAsync(() => new CarCoordinate
                            {
                                position_x = vehicle.Position.X,
                                position_y = vehicle.Position.Y,
                                position_z = vehicle.Position.Z,
                            })
                        });
                    }
                }

                foreach (var dbVehicle in dbVehicles)
                {
                    RXGarage garage = GarageModule.Garages.FirstOrDefault(x => x.Id == dbVehicle.GarageId);
                    if (garage == null) continue;

                    var model = VehicleModelModule.VehicleModels.FirstOrDefault(x => x.Id == dbVehicle.ModelId);
                    if (model == null) continue;

                    var VehicleKeys = new List<uint>();

                    if (dbVehicle.VehicleKeys.IsValidJson<List<uint>>())
                        VehicleKeys = JsonConvert.DeserializeObject<List<uint>>(dbVehicle.VehicleKeys);

                    if (!VehicleKeys.Contains(player.Id)) continue;


                    overviewVehicles.Add(new OverviewVehicle
                    {
                        Id = dbVehicle.Id,
                        Vehiclehash = model.Name,
                        GarageName = garage.Name,
                        InGarage = true,
                        Notiz = "-",
                        CarCor = new CarCoordinate
                        {
                            position_x = garage.Position.X,
                            position_y = garage.Position.Y,
                            position_z = garage.Position.Z,
                        }
                    });
                }

            } else if (side == 2)
            {
                foreach (var vehicle in frakvehicles)
                {
                    if (vehicle.ModelData != null)
                    {
                        var dbVehicle = await db.TeamVehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id);
                        if (dbVehicle == null) continue;

                

                        overviewVehicles.Add(new OverviewVehicle
                        {
                            Id = vehicle.Id,
                            Vehiclehash = vehicle.ModelData.Name,
                            GarageName = dbVehicle.Stored ? "Eingeparkt" : "Ausgeparkt",
                            Notiz = "-",
                            InGarage = false,
                            CarCor = await NAPI.Task.RunReturnAsync(() => new CarCoordinate
                            {
                                position_x = vehicle.Position.X,
                                position_y = vehicle.Position.Y,
                                position_z = vehicle.Position.Z,
                            })
                        });
                    }
                }

                foreach (var dbVehicle in dbTeamVehicles)
                {
          
                    var model = VehicleModelModule.VehicleModels.FirstOrDefault(x => x.Hash == dbVehicle.Hash);
                    if (model == null) continue;


                    overviewVehicles.Add(new OverviewVehicle
                    {
                        Id = dbVehicle.Id,
                        Vehiclehash = model.Name,
                        GarageName = dbVehicle.Stored ? "Eingeparkt" : "Ausgeparkt",
                        InGarage = true,
                        Notiz = "-",
                        CarCor = new CarCoordinate
                        {
                            position_x = player.Team.Garage.X,
                            position_y = player.Team.Garage.Y,
                            position_z = player.Team.Garage.Z,
                        }
                    });
                }
            }


            object veh = new
            {
                i = side,
                data = overviewVehicles,
            };

            await player.TriggerEventAsync("SendOverviewVehicles", NAPI.Util.ToJson(veh));

        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task requestVehicleOverviewByCategory(RXPlayer player, int category)
        {
            if (!player.IsLoggedIn) return;

            using var db = new RXContext();

            List<OverviewVehicle> overviewVehicles = new List<OverviewVehicle>();

            var vehicles = await NAPI.Task.RunReturnAsync(() => VehicleController.GetValidVehicles().Where(x => x.HasPerm(player)).ToList());

            List<DbVehicle> dbVehicles = await db.Vehicles.Where(x => x.Stored == true && x.OwnerId == player.Id).ToListAsync();

            foreach (var vehicle in vehicles)
            {
                if (vehicle.ModelData != null)
                {
                    var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id);
                    if (dbVehicle == null) continue;

                    RXGarage garage = GarageModule.Garages.FirstOrDefault(x => x.Id == dbVehicle.GarageId);
                    if (garage == null) continue;

                    overviewVehicles.Add(new OverviewVehicle
                    {
                        Id = vehicle.Id,
                        Vehiclehash = vehicle.ModelData.Name,
                        GarageName = garage.Name,
                        InGarage = false,
                        CarCor = await NAPI.Task.RunReturnAsync(() => new CarCoordinate
                        {
                            position_x = vehicle.Position.X,
                            position_y = vehicle.Position.Y,
                            position_z = vehicle.Position.Z,
                        })
                    });
                }
            }

            foreach (var dbVehicle in dbVehicles)
            {
                RXGarage garage = GarageModule.Garages.FirstOrDefault(x => x.Id == dbVehicle.GarageId);
                if (garage == null) continue;

                var model = VehicleModelModule.VehicleModels.FirstOrDefault(x => x.Id == dbVehicle.ModelId);
                if (model == null) continue;

                overviewVehicles.Add(new OverviewVehicle
                {
                    Id = dbVehicle.Id,
                    Vehiclehash = model.Name,
                    GarageName = garage.Name,
                    InGarage = true,
                    CarCor = new CarCoordinate
                    {
                        position_x = garage.Position.X,
                        position_y = garage.Position.Y,
                        position_z = garage.Position.Z,
                    }
                });
            }

            await this.Window.TriggerEvent(player, "responseVehicleOverview", JsonConvert.SerializeObject(overviewVehicles));
        }
    }
}
