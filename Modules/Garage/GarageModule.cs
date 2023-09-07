using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Faction;
using Backend.Modules.Vehicle;
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

namespace Backend.Modules.Garage
{
    public class RXGarageNew
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "hg")]
        public bool Storage { get; set; } = false;

        [JsonProperty(PropertyName = "data")]
        public List<GarageVehicle> data { get; set; }
    }
    public class RXGarage
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "hg")]
        public bool Storage = false;

        [JsonIgnore]
        public Vector3 Position { get; set; }

        [JsonIgnore]
        public float NPCHeading { get; set; }

        [JsonIgnore]
        public List<RXGarageSpawn> Spawns { get; set; } = new List<RXGarageSpawn>();

        [JsonIgnore]
        public string NPCHash { get; set; } = "a_f_y_business_01";

        public RXGarage(uint id, string name, Vector3 position, float npc_heading, List<RXGarageSpawn> spawns, string npc)
        {
            Id = id;
            Name = name;
            Position = position;
            Spawns = spawns;
            NPCHeading = npc_heading;
            NPCHash = npc;
        }
    }

    public class GarageVehicle
    {

        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; } = "";
        public string Plate { get; set; } = "";

        [JsonProperty(PropertyName = "no")]
        public string Notice { get; set; } = "";

        public GarageVehicle(uint id, string name, string plate, string notice = "")
        {
            Id = id;
            Name = name;
            Plate = plate;
            Notice = notice;
        }
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class GarageModule : RXModule
    {
        public GarageModule() : base("Garage", new RXWindow("Garage")) { }

        public static List<RXGarage> Garages = new List<RXGarage>();

        //[HandleExceptions]
        public override void LoadAsync()
        {
            Garages = new List<RXGarage>
            {
                new RXGarage(1, "Vespucci", new Vector3(-1184.2845, -1509.452, 3.548), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, 126.469f, new Vector3(-1184.7214, -1492.882, 3.2796707))
                }, "u_m_y_mani"),

                new RXGarage(2, "Pillbox", new Vector3(-313.81174, -1034.3071, 29.430506), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, -113.43376f, new Vector3(-305.34454, -1012.9885, 29.285078))
                }, "cs_milton"),

                new RXGarage(3, "Würfelpark", new Vector3(100.46749, -1073.2855, 28.274118), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, -1.5121901f, new Vector3(121.32157, -1082.3136, 28.093328))
                }, "u_m_y_militarybum"),

                new RXGarage(4, "Vinewood", new Vector3(638.3967, 206.5143, 96.5042), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, -112.68891f, new Vector3(627.5314, 196.76143, 96.212105))
                }, "s_m_y_devinsec_01"),

                new RXGarage(5, "Harmony", new Vector3(614.6637, 2784.969, 42.381184), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, 1.2953043f, new Vector3(618.0704, 2790.8306, 41.096012))
                }, "ig_jimmyboston"),

                new RXGarage(6, "Universität", new Vector3(-1684.601, 58.23332, 62.92697), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, 161.44273f, new Vector3(-1685.1252, 49.431946, 62.924133))
                }, "u_m_y_chip"),

                new RXGarage(7, "Schneiderei", new Vector3(-226, -2494, 5), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, 39.95556f, new Vector3(-224, -2485, 6))
                }, "cs_manuel"),

                new RXGarage(8, "Davis", new Vector3(-73.88013, -2004.112, 17.27527), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, 353.058f, new Vector3(-81.46315, -2005.426, 17.59334))
                }, "g_m_y_ballaorig_01"),

                new RXGarage(9, "Flughafen", new Vector3(-984.3403, -2640.988, 12.852915), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, -123.34593f, new Vector3(-987.20685, -2645.407, 12.873976))
                }, "s_m_y_uscg_01"),

                new RXGarage(10, "Rockford", new Vector3(-728.04517, -63.06201, 40.653107), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, 24.013073f, new Vector3(-733.1606, -72.167366, 40.647232))
                }, "u_m_m_jewelsec_01"),

                new RXGarage(14, "Rockford Tiefgarage", new Vector3(-840.92957, -399.21387, 30.471682), -65f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, -155.75f, new Vector3(-829.4268, -391.92184, 31.325254))
                }, "u_m_m_jewelsec_01"),

                new RXGarage(11, "Mirrorpark", new Vector3(1036.261, -763.1047, 56.892986), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, 93.02585f, new Vector3(1046.7023, -774.50836, 56.91756))
                }, "ig_lamardavis"),

                new RXGarage(12, "Paleto Bay", new Vector3(53.09, 6338.29, 30.38), 5f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, 26.55f, new Vector3(31, 6351, 30.6))
                }, "csb_trafficwarden"),

                new RXGarage(13, "Cypress", new Vector3(1188.0487, -1759.8359, 38.553436), -55.186874f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, -40.112362f, new Vector3(1194.2191, -1765.447, 39.187775)),
                    new RXGarageSpawn(2, -36.554295f, new Vector3(1197.4236, -1767.7839, 39.47325)),
                    new RXGarageSpawn(3, -11.424365f, new Vector3(1210.9462, -1770.5333, 39.933884))
                }, "g_m_y_salvagoon_01"),

                new RXGarage(15, "Impound", new Vector3(392.69028f, -1641.1368f, 27.375248f), -73.298065f, new List<RXGarageSpawn>
                {
                    new RXGarageSpawn(1, -132.08353f, new Vector3( 406.25125f, -1644.5864f, 29.29204f)),
                }, "g_m_y_salvagoon_01"),
            };

            Garages.ForEach(async garage =>
            {
                new NPC((PedHash)NAPI.Util.GetHashKey(garage.NPCHash), garage.Position.Add(new Vector3(0, 0, 1)), garage.NPCHeading, 0u);

                var mcb = await NAPI.Entity.CreateMCB(garage.Position, new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.VerticalCylinder, true, 473, 3, garage.Name);

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um die Garage zu öffnen!",
                    Color = "lightblue",
                    Duration = 3500,
                    Title = "Garage " + garage.Name
                };

                mcb.ColShape.Action = async player => await OpenGarage(player, garage.Id);
            });
        }

        //[HandleExceptions]
        public async Task OpenGarage(RXPlayer player, uint garageId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            await NAPI.Task.RunAsync(() => player.ResetData("teamGarage"));

            var garage = Garages.FirstOrDefault(x => x.Id == garageId);
            if (garage == null) return;

            using var db = new RXContext();

            List<DbVehicle> dbVehicles = await db.Vehicles.Where(x => x.Stored == true && x.OwnerId == player.Id && (x.GarageId == garageId || x.GarageId == 0)).ToListAsync();

            List<GarageVehicle> garageVehicles = new List<GarageVehicle>();
            garageVehicles = dbVehicles.ConvertAll(x => new GarageVehicle(x.Id, VehicleModelModule.VehicleModels.FirstOrDefault(model => model.Id == x.ModelId)?.Name, x.Plate.ToUpper()));

            await this.Window.OpenWindow(player, new RXGarageNew() { Id = garageId, Name = garage.Name, Storage = false, data = garageVehicles });

            await NAPI.Task.RunAsync(() => player.SetData("garageId", garageId));
        }

        [RemoteEvent]
        public async Task AddVehicleToFavorites(RXPlayer player, int vehid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;


            using var db = new RXContext();

            var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehid && x.OwnerId == player.Id);

            dbVehicle.Fav = true;
            await db.SaveChangesAsync();


        }
        [RemoteEvent]
        public async Task RemoveVehicleFromFavorites(RXPlayer player, int vehid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;


            using var db = new RXContext();

            var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehid && x.OwnerId == player.Id);

            dbVehicle.Fav = false;

            await db.SaveChangesAsync();

        }

        [RemoteEvent]
        public async Task GetFavoriteVehicles(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;


            using var db = new RXContext();

            List<DbVehicle> dbVehicles = await db.Vehicles.Where(x => x.Stored == true && x.Fav == true && x.OwnerId == player.Id).ToListAsync();

            List<GarageVehicle> garageVehicles = new List<GarageVehicle>();
            garageVehicles = dbVehicles.ConvertAll(x => new GarageVehicle(x.Id, VehicleModelModule.VehicleModels.FirstOrDefault(model => model.Id == x.ModelId)?.Name, x.Plate.ToUpper()));

            string lol = "";
            foreach (GarageVehicle gv in garageVehicles)
            {
                lol += gv.Id + ",";
            }

            lol += "-1";

            await player.TriggerEventAsync("SendFavoriteVehicles", lol);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task GetInparkVehicles(RXPlayer player, uint garageId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;
            List<GarageVehicle> garageVehicles = new List<GarageVehicle>();

            if (await NAPI.Task.RunReturnAsync(() => player.HasData("teamGarage")))
            {




                List<RXVehicle> vehicles = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllVehicles().ToList().ConvertAll(v => (RXVehicle)v).Where(x => x.Position.DistanceTo(player.Position) <= 25 && x.ModelData != null && x.TeamId != 0 && x.TeamId == player.Team.Id).ToList());

                garageVehicles = vehicles.ConvertAll(x => new GarageVehicle(x.Id, x.ModelData.Name, x.Plate.ToUpper()));

            }
            else
            {

                var garage = Garages.FirstOrDefault(x => x.Id == garageId);
                if (garage == null) return;



                List<RXVehicle> vehicles = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllVehicles().ToList().ConvertAll(v => (RXVehicle)v).Where(x => x.Position.DistanceTo(player.Position) <= 25 && x.ModelData != null && x.OwnerId == player.Id).ToList());

                garageVehicles = vehicles.ConvertAll(x => new GarageVehicle(x.Id, x.ModelData.Name, x.Plate.ToUpper()));

            }
            await player.TriggerEventAsync("SendInparkVehicles", JsonConvert.SerializeObject(garageVehicles));
        }

        [RemoteEvent]
        public async Task Park(RXPlayer player, RXVehicle vehicle)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (vehicle == null) return;

            if (!vehicle.HasPerm(player)) return;

            var ppos = await player.GetPositionAsync();

            foreach (var garage in Garages)
            {
                if (garage.Position.DistanceTo(ppos) < 40)
                {
                    if (garage.Id == 15)
                    {
                        await player.SendNotify("Du kannst dein Fahrzeug nicht auf dem Abschlepphof einlagern!");
                        return;
                    }

                    await NAPI.Task.RunAsync(() =>
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
                    });

                    using var db = new RXContext();

                    var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id && x.OwnerId == player.Id);
                    if (dbVehicle == null) return;

                    dbVehicle.Stored = true;
                    dbVehicle.GarageId = garage.Id;

                    await db.SaveChangesAsync();


                    await player.SendNotify("Das Fahrzeug wurde erfolgreich eingeparkt!", 3500, "green", "Garage " + garage.Name);

                    return;
                }
            }

            if (player.Team != null && player.Team.Id != 0)
            {
                if (player.Team.Garage.DistanceTo(ppos) < 40)
                {
                    await NAPI.Task.RunAsync(() =>
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
                    });

                    using var db = new RXContext();

                    var dbVehicle = await db.TeamVehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id && x.TeamId == player.TeamId);
                    if (dbVehicle == null) return;

                    dbVehicle.Stored = true;

                    await db.SaveChangesAsync();
                }

                await player.SendNotify("Das Fahrzeug wurde erfolgreich eingeparkt!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
                return;
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ParkOut(RXPlayer player, string state, uint garageId, uint vehicleId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || (state != "takeout" && state != "takein")) return;

            if (await NAPI.Task.RunReturnAsync(() => player.HasData("teamGarage")))
            {
                await TeamModule.requestTeamVehicle(player, state, garageId, vehicleId);
            }
            else
            {

                var garage = Garages.FirstOrDefault(x => x.Id == garageId);
                if (garage == null) return;

                if (state == "takeout") // Ausparken
                {
                    RXGarageSpawn garageSpawn = null;

                    List<RXVehicle> vehicles = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllVehicles().ConvertAll(x => (RXVehicle)x));

                    foreach (var spawn in garage.Spawns)
                    {
                        if (vehicles.FirstOrDefault(v => NAPI.Task.RunReturn(() => v.Position).DistanceTo(spawn.Position) <= 2f) == null)
                        {
                            garageSpawn = spawn;

                            break;
                        }
                    }

                    if (garageSpawn == null)
                    {
                        await player.SendNotify("Es ist aktuell kein Parkplatz für das Fahrzeug vorhanden!", 3500, "red", "Garage " + garage.Name);

                        return;
                    }

                    using var db = new RXContext();

                    var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId && x.OwnerId == player.Id && (x.GarageId == garageId || x.GarageId == 0));
                    if (dbVehicle == null) return;

                    dbVehicle.Stored = false;

                    if (garage.Id == 15)
                    {
                        dbVehicle.GarageId = 2;
                        await player.SendNotify("Du hast erfolgreich dein Fahrzeug vom Abschlepphof geholt!", 3550, "green", "Garage - Impound");
                        await player.TakeMoney(1000);
                    }
                    else
                        await player.SendNotify("Das Fahrzeug wurde erfolgreich ausgeparkt!", 3500, "green", "Garage " + garage.Name);

                    await db.SaveChangesAsync();

                    await NAPI.Task.RunAsync(() =>
                    {
                        RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(dbVehicle.Hash), garageSpawn.Position, garageSpawn.Heading, 0, 0, dbVehicle.Plate.ToUpper(), 255, true, true, player.Dimension);

                        vehicle.Id = dbVehicle.Id;
                        vehicle.ModelData = VehicleModelModule.VehicleModels.FirstOrDefault(x => x.Id == dbVehicle.ModelId);
                        vehicle.Fuel = dbVehicle.Fuel;
                        vehicle.CustomPrimaryColor = new Color(dbVehicle.R, dbVehicle.G, dbVehicle.B);
                        vehicle.OwnerId = dbVehicle.OwnerId;
                        //vehicle.TeamId = dbVehicle.TeamId;
                        vehicle.ContainerId = dbVehicle.ContainerId;
                        vehicle.Plate = dbVehicle.Plate;
                        vehicle.Distance = dbVehicle.Distance;

                        vehicle.SetEngineStatus(false);
                        vehicle.SetLocked(true);
                    });
                }
                else if (state == "takein") // Einparken
                {
                    if (garage.Id == 15)
                    {
                        await player.SendNotify("Du kannst dein Fahrzeug nicht auf dem Abschlepphof einlagern!");
                        return;
                    }

                    await NAPI.Task.RunAsync(() =>
                    {
                        var veh = NAPI.Pools.GetAllVehicles().ConvertAll(v => (RXVehicle)v).FirstOrDefault(x => x.Id == vehicleId && x.HasPerm(player));
                        if (veh == null) return;

                        veh.Occupants.forEachAlternative(o =>
                        {
                            if (o is RXPlayer)
                            {
                                var target = (RXPlayer)o;

                                target.WarpOutOfVehicle();
                            }
                        });
                        veh.Delete();
                    });

                    using var db = new RXContext();

                    var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId && x.OwnerId == player.Id);
                    if (dbVehicle == null) return;

                    dbVehicle.Stored = true;
                    dbVehicle.GarageId = garageId;

                    await db.SaveChangesAsync();

                    await player.SendNotify("Das Fahrzeug wurde erfolgreich eingeparkt!", 3500, "green", "Garage " + garage.Name);
                }

                //await player.SendNotify("Das Fahrzeug wurde erfolgreich " + (state == "takeout" ? "aus" : "ein") + "geparkt!", 3500, "green", "Garage " + garage.Name);
            }
        }
    }
}
