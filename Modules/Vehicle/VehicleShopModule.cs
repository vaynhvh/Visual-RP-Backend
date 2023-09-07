using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Bank;
using Backend.Modules.Faction;
using Backend.Modules.Native;
using Backend.Modules.Phone.Apps;
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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Vehicle
{

    public class RXVehicleShopSpawn
    {
        public uint Id { get; set; }
        public float Heading { get; set; }
        public Vector3 Position { get; set; }

        public RXVehicleShopSpawn(uint id, float heading, Vector3 position)
        {
            Id = id;
            Heading = heading;
            Position = position;
        }
    }

    public class RXVehicleShopOffer
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public int Price { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public bool Live { get; set; } = false;
        
        public RXVehicleShopOffer(uint id, string name, string model, int price, Vector3 position, float heading = 0f, bool live = false)
        {
            Id = id;
            Name = name;
            Model = model;
            Price = price;
            Position = position;
            Heading = heading;
            Live = live;
        }
    }

    public class VehicleShopNew
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<VehicleShopModelNew> Data { get; set; }



    }

    public class VehicleShopModelNew
    {
        [JsonProperty(PropertyName = "vi")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "c")]
        public string Classification { get; set; }

        [JsonProperty(PropertyName = "s")]
        public int InventorySize { get; set; }
        [JsonProperty(PropertyName = "k")]
        public int InventoryWeight { get; set; }

        [JsonProperty(PropertyName = "t")]
        public int Seats { get; set; }
        [JsonProperty(PropertyName = "p")]
        public int Price { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "ieins")]
        public string Image1 { get; set; }

        [JsonProperty(PropertyName = "izwei")]
        public string Image2 { get; set; }

    }

    public class RXVehicleShop
    {

        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public string NPCHash { get; set; }
        public float NPCHeading { get; set; }
        public List<RXVehicleShopOffer> Offers { get; set; }
        public List<RXVehicleShopSpawn> Spawns { get; set; }
        public bool TeamShop { get; set; } = false;
        public TeamType Type { get; set; }

        public RXVehicleShop(uint id, string name, Vector3 position, float npc_heading, string npc, List<RXVehicleShopOffer> offers, List<RXVehicleShopSpawn> spawns, bool teamShop = false, TeamType teamType = TeamType.Gang)
        {
            Id = id;
            Name = name;
            Position = position;
            NPCHeading = npc_heading;
            NPCHash = npc;
            Offers = offers;
            Spawns = spawns;
            TeamShop = teamShop;
            Type = teamType;
        }
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class VehicleShopModule : RXModule
    {
        public VehicleShopModule() : base("VehicleShop") { }

        public static List<RXVehicleShop> VehicleShops = new List<RXVehicleShop>();

        //[HandleExceptions]
        public override async void LoadAsync()
        {

            using var db = new RXContext();


            foreach (DbVehicleShop shop in await db.VehicleShops.ToListAsync())
            {
                RXVehicleShop vehshop = new RXVehicleShop(shop.Id, shop.Name, shop.Position.ToPos(), shop.NPCHeading, shop.NPCHash, new List<RXVehicleShopOffer>(), new List<RXVehicleShopSpawn>(), shop.TeamShop, (TeamType)shop.Teams);
                VehicleShops.Add(vehshop);
            }

            foreach (DbVehicleShopOffers offer in await db.VehicleShopsOffers.ToListAsync())
            {
                RXVehicleShopOffer vehoffer = new RXVehicleShopOffer(offer.Id, offer.Name, offer.Model, offer.Price, offer.Position.ToPos(), offer.Heading, offer.Live);
                VehicleShops.Find(x => x.Id == offer.VehShopId).Offers.Add(vehoffer);
            }
            foreach (DbVehicleShopSpawn spawn in await db.VehicleShopsSpawns.ToListAsync())
            {
                RXVehicleShopSpawn vehspawn = new RXVehicleShopSpawn(spawn.Id, spawn.Heading, spawn.Position.ToPos());
                VehicleShops.Find(x => x.Id == spawn.VehShopId).Spawns.Add(vehspawn);
            }
            /*       VehicleShops = new List<RXVehicleShop>
                   {
                       new RXVehicleShop(1, "Premium Deluxe Motorsport", new Vector3(-33.863148, -1101.5636, 26.422363), 76.679344f, "a_f_y_business_01", new List<RXVehicleShopOffer>
                       {
                           new RXVehicleShopOffer(1, "Adder", "adder", 25000, new Vector3(-40.102394, -1095.9955, 26.422363), -157.27077f, true),
                           new RXVehicleShopOffer(2, "Itali GTO", "italigto", 42500, new Vector3(-44.26475, -1095.1648, 26.422321), -159.41941f, true),
                           new RXVehicleShopOffer(3, "Ignus", "ignus", 72500, new Vector3(-48.951748, -1093.9539, 26.422325), -152.26884f, true),
                           new RXVehicleShopOffer(4, "Starling", "starling", 185000, new Vector3(), 0, false),
                           new RXVehicleShopOffer(5, "Journey", "journey", 75000, new Vector3(), 0, false),
                           new RXVehicleShopOffer(6, "Camper", "camper", 75000, new Vector3(), 0, false),

                       }, new List<RXVehicleShopSpawn>
                       {
                           new RXVehicleShopSpawn(1, 131.36244f, new Vector3(-16.723864, -1078.8575, 26.67208))
                       }),
                       new RXVehicleShop(2, "Gang-Fahrzeughändler", new Vector3(-602.08844, -1122.1565, 22.324247), -90.93371f, "g_m_y_mexgoon_02", new List<RXVehicleShopOffer>
                       {
                           new RXVehicleShopOffer(1, "Schafter (Gepanzert)", "schafter5", 185000, new Vector3(), 0, false),
                           new RXVehicleShopOffer(2, "Jugular", "jugular", 185000, new Vector3(), 0, false),
                           new RXVehicleShopOffer(3, "Sultan", "sultan2", 185000, new Vector3(), 0, false),
                           new RXVehicleShopOffer(4, "Baller (Gepanzert)", "baller5", 185000, new Vector3(), 0, false),
                       }, new List<RXVehicleShopSpawn>(), true, TeamType.Gang),
                       new RXVehicleShop(3, "Mafia-Fahrzeughändler", new Vector3(-700.13367, -147.16032, 37.84557), -61.403465f, "cs_martinmadrazo", new List<RXVehicleShopOffer>
                       {
                           new RXVehicleShopOffer(1, "Cognoscenti (Gepanzert)", "Cognoscenti2", 185000, new Vector3(), 0, false),
                           new RXVehicleShopOffer(2, "Cognoscenti", "Cognoscenti", 185000, new Vector3(), 0, false),
                           new RXVehicleShopOffer(3, "Tailgater", "taligater2", 185000, new Vector3(), 0, false),
                           new RXVehicleShopOffer(4, "XLS (Gepanzert)", "xls2", 185000, new Vector3(), 0, false),
                       }, new List<RXVehicleShopSpawn>(), true, TeamType.Mafia),
                   };
            */



            VehicleShops.ForEach(async shop =>
            {
                new NPC((PedHash)NAPI.Util.GetHashKey(shop.NPCHash), shop.Position.Add(new Vector3(0, 0, 0)), shop.NPCHeading, 0u);

                var mcb = await NAPI.Entity.CreateMCB(shop.Position, new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.VerticalCylinder, !shop.TeamShop, 595, 13, shop.Name);

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um ein Auto zu kaufen!",
                    Color = "orange",
                    Duration = 3500,
                    Title = shop.Name,
                    OnlyBadFaction = shop.TeamShop
                };

                mcb.ColShape.Action = async player =>
                {
                    if ((shop.TeamShop && player.TeamId != 0 && player.Team != null && player.Team.Type == shop.Type && player.Teamrank > 9) || !shop.TeamShop)
                        await OpenShop(player, shop.Id);
                };

                shop.Offers.ForEach(offer =>
                {
                    if (offer.Live)
                    {
                        NAPI.Task.Run(async () =>
                        {
                            var model = VehicleModelModule.VehicleModels.FirstOrDefault(x => x.Hash == offer.Model);
                            if (model == null) return;

                            RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(offer.Model), offer.Position, offer.Heading, 0, 0, "SHOP");

                            vehicle.SetLocked(true);
                            vehicle.SetEngineStatus(false);

                            vehicle.SetSharedData("ShopCar", JsonConvert.SerializeObject(offer));

                            var mcbVehicle = await NAPI.Entity.CreateMCB(offer.Position, new Color(255, 255, 255), 0u, 3f);

                            var msg = "";

                            msg += "Modell: " + offer.Name;
                            msg += " | Höchstleistung: " + model.MaxKMH + " km/h";
                            msg += " | Lagerraum: " + (model.InventoryWeight / 1000) + " KG";
                            msg += " | Kofferraum: " + model.InventorySize + " Slots";
                            msg += " | Preis: " + offer.Price.FormatMoneyNumber();

                            mcbVehicle.ColShape.Message = new RXMessage
                            {
                                Text = msg,
                                Color = "dgray",
                                Duration = 10000,
                                Title = "Fahrzeuginformation",
                                OnlyBadFaction = shop.TeamShop
                            };


                        });
                    }
                });
            });

            var vehicleshops = GpsApp.gpsCategories.FirstOrDefault(x => x.Name == "Vehicleshops");
            if (vehicleshops == null) return;

            foreach (var shops in VehicleShops)
            {
                vehicleshops.Locations.Add(new GPSPosition(shops.Name, shops.Position));
            }
        }

        //[HandleExceptions]
        public override Task OnTenSecond()
        {
            NAPI.Task.Run(() =>
            {
                foreach (var vehicle in VehicleController.GetVehicles().Where(x => x.HasSharedData("ShopCar")))
                {
                    if (vehicle == null) continue;

                    var offerStr = vehicle.GetSharedData<string>("ShopCar");
                    if (offerStr == null) continue;

                    var offer = JsonConvert.DeserializeObject<RXVehicleShopOffer>(offerStr);
                    if (offer == null) continue;

                    vehicle.Repair();

            //        vehicle.Position = offer.Position.Subtract(new Vector3(0, 0, 0.5));
                    vehicle.Rotation.Z = offer.Heading;

                    vehicle.SetLocked(true);
                    vehicle.SetEngineStatus(false);
                }
            });

            return Task.CompletedTask;
        }

        //[HandleExceptions]
        public async Task OpenShop(RXPlayer player, uint shopId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            var shop = VehicleShops.FirstOrDefault(x => x.Id == shopId);
            if (shop == null) return;

            RXWindow VehicleShop = new RXWindow("VehicleShop");


            List<VehicleShopModelNew> vhoffer = new List<VehicleShopModelNew>();

            foreach (RXVehicleShopOffer vo in shop.Offers)
            {
                try
                {
       

                    vhoffer.Add(new VehicleShopModelNew() { Id = vo.Id, Name = vo.Name, InventorySize = VehicleModelModule.VehicleModels.Find(x => x.Hash == vo.Model).InventorySize, InventoryWeight = VehicleModelModule.VehicleModels.Find(x => x.Hash == vo.Model).InventoryWeight, Classification = VehicleModelModule.VehicleModels.Find(x => x.Hash == vo.Model).Type, Price = vo.Price, Seats = VehicleModelModule.VehicleModels.Find(x => x.Hash == vo.Model).Seats, Image1 = "http://91.212.121.209/vehimages/" + vo.Model + ".png", Image2 = "http://91.212.121.209/vehimages/" + vo.Model + ".png" });

                } catch (Exception e)
                {
                    RXLogger.Print(e.Message);
                }
                }
            VehicleShopNew vh = new VehicleShopNew() { Id = shop.Id, Name = shop.Name, Data = vhoffer };

            await VehicleShop.OpenWindow(player, vh);

        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task BuyVehicle(RXPlayer player, string shopIdStr, string offerIdStr, int r, int g, int b)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (int.TryParse(shopIdStr, out var shopId) && int.TryParse(offerIdStr, out var offerId))
            {
                var shop = VehicleShops.FirstOrDefault(x => x.Id == shopId);
                if (shop == null) return;

                var offer = shop.Offers.FirstOrDefault(x => x.Id == offerId);
                if (offer == null) return;

                if (!shop.TeamShop)
                {
                    var model = VehicleModelModule.VehicleModels.FirstOrDefault(x => x.Hash == offer.Model);
                    if (model == null) return;

                    RXVehicleShopSpawn vehicleShopSpawn = null;

                    List<RXVehicle> vehicles = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllVehicles().ConvertAll(x => (RXVehicle)x));

                    foreach (var spawn in shop.Spawns)
                    {
                        if (vehicles.FirstOrDefault(v => NAPI.Task.RunReturn(() => v.Position).DistanceTo(spawn.Position) <= 2f) == null)
                        {
                            vehicleShopSpawn = spawn;

                            break;
                        }
                    }

                    if (vehicleShopSpawn == null)
                    {
                        await player.SendNotify("Es ist aktuell kein Parkplatz für das Fahrzeug vorhanden!", 3500, "red", shop.Name);

                        return;
                    }

                    PaymentModule.CreatePayment(player, offer.Price, async player =>
                    {
                        await RX.GiveMoneyToStaatskonto(offer.Price, "Autokauf - " + offer.Name + " - " + player.Id);

                        using var db = new RXContext();

                        
                        var dbVehicle = new DbVehicle
                        {
                            Id = await db.Vehicles.CountAsync() == 0 ? 1 : (await db.Vehicles.MaxAsync(v => v.Id) + 1),
                            ContainerId = 0,
                            Distance = 0,
                            Fuel = model.Fuel,
                            GarageId = 0,
                            Hash = model.Hash,
                            ModelId = model.Id,
                            OwnerId = player.Id,
                            Plate = "",
                            Position = vehicleShopSpawn.Position.FromPos(),
                            R = r,
                            G = g,
                            B = b,
                            Rotation = "0,0,0",
                            Stored = false,
                            Tuning = "{}"
                        };

                       
                        await db.Vehicles.AddAsync(dbVehicle);

                        await db.SaveChangesAsync();

                        NAPI.Task.Run(() =>
                        {
                            var vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(offer.Model), vehicleShopSpawn.Position, vehicleShopSpawn.Heading, 0, 0);

                            vehicle.Id = dbVehicle.Id;
                            vehicle.ModelData = model;
                            vehicle.Fuel = dbVehicle.Fuel;
                            vehicle.CustomPrimaryColor = new Color(r, g, b);
                            vehicle.CustomSecondaryColor = new Color(r, g, b);
                            vehicle.OwnerId = dbVehicle.OwnerId;
                        //vehicle.TeamId = dbVehicle.TeamId;
                        vehicle.ContainerId = dbVehicle.ContainerId;
                            vehicle.Plate = dbVehicle.Plate;
                            vehicle.Distance = dbVehicle.Distance;

                            vehicle.SetLocked(true);
                            vehicle.SetEngineStatus(false);
                        });

                        await player.SendNotify("Du hast das Fahrzeug erfolgreich erworben! Es steht auf dem Parkplatz des Autohauses.", 3500, "orange", shop.Name);

                    }, "Autokauf - " + offer.Name);
                }
                else
                {
                    PaymentModule.CreateFrakPayment(player, offer.Price, async player =>
                    {
                        await RX.GiveMoneyToStaatskonto(offer.Price, "Autokauf - " + offer.Name + " - " + player.Id);

                        using var db = new RXContext();

                        int livery = 0;

                        if (player.Team.Id == 20) { livery = 1; }

                        var dbVehicle = new DbTeamVehicle
                        {
                            Id = await db.TeamVehicles.CountAsync() == 0 ? 1 : (await db.TeamVehicles.MaxAsync(v => v.Id) + 1),
                            Hash = offer.Model,
                            Stored = true,
                            TeamId = player.TeamId,
                            R = r,
                            G = g,
                            B = b,
                            Livery = livery
                        };

                        await db.TeamVehicles.AddAsync(dbVehicle);

                        await db.SaveChangesAsync();

                        await player.SendNotify("Du hast das Fahrzeug erfolgreich erworben! Es wird in wenigen Minuten geliefert.", 3500, "orange", shop.Name);

                    }, "Autokauf - " + offer.Name, false, true);
                }
            }
        }
    }
}
