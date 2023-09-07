using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Bank;
using Backend.Modules.Casino;
using Backend.Modules.Inventory;
using Backend.Modules.Wardrobe;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using DSharpPlus.Entities;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Mozilla;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Modules.House
{

    public class HouseInterior
    {
        public uint id { get; set; }
        public float pos_x { get; set; }
        public float pos_y { get; set; }
        public float pos_z { get; set; }
        public float heading { get; set; }
        public float inv_x { get; set; }
        public float inv_y { get; set; }
        public float inv_z { get; set; }
        public float wardrobe_x { get; set; }
        public float wardrobe_y { get; set; }
        public float wardrobe_z { get; set; }

    }
    public class BlueprintBasementData
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "u")]
        public uint CanUse { get; set; }

        [JsonProperty(PropertyName = "di")]
        public uint BlueprintId { get; set; }

    }
    public class ItemBasementData
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public uint Amount { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        public uint item_id { get; set; }

    }

    public class WerkbankData
    {
        [JsonProperty(PropertyName = "b")]
        public List<BlueprintBasementData> Blueprints { get; set; }
        [JsonProperty(PropertyName = "i")]
        public List<ItemBasementData> Items { get; set; }

        [JsonProperty(PropertyName = "l")]
        public uint Level { get; set; }
    } 

    public class HouseMenu
    {
        [JsonProperty(PropertyName = "i")]
        public uint id { get; set; }
        [JsonProperty(PropertyName = "hb")]
        public uint basement { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string klingelschild { get; set; }


        [JsonProperty(PropertyName = "bl")]
        public uint basementLevel { get; set; }

        [JsonProperty(PropertyName = "ms")]
        public uint server { get; set; }

        [JsonProperty(PropertyName = "dhw")]
        public uint houswert { get; set; }
        [JsonProperty(PropertyName = "mw")]
        public uint workstation { get; set; }

        [JsonProperty(PropertyName = "l")]
        public bool locked { get; set; }

        public uint rent { get; set; }
        [JsonProperty(PropertyName = "m")]
        public uint money { get; set; }


        [JsonProperty(PropertyName = "in")]
        public bool isInside { get; set; }

        [JsonProperty(PropertyName = "cr")]
        public bool canRent { get; set; }

        [JsonProperty(PropertyName = "o")]
        public uint canControl { get; set; }
    }

        class HouseModule : RXModule
        {
        public HouseModule() : base("House", new RXWindow("House")) { }

        public static List<DbHouse> dbHouses = new List<DbHouse>();
        public static List<HouseInterior> HouseInteriors = new List<HouseInterior>();
        public static Vector3 ServerPos = new Vector3(1133.7701, -3199.113, -40.9);
        public static Vector3 WorkstationPos = new Vector3(1135.0079, -3194.2236, -40.396923);

        public override async void LoadAsync()
        {
            await Task.Delay(4000);

            HouseInteriors = new List<HouseInterior>
            {
                new HouseInterior
                {
                    id = 1,
                    pos_x = -773.898f,
                    pos_y =342.152f,
                    pos_z = 196.686f,
                    heading = 0f,
                    inv_x = -765.59f,
                    inv_y = 331.24f,
                    inv_z = 196.08f,
                    wardrobe_x = -763.1977f,
                    wardrobe_y = 328.81595f,
                    wardrobe_z = 199.48871f

                },
                new HouseInterior
                {
                    id = 2,
                    pos_x = -774.138f,
                    pos_y = 342.032f,
                    pos_z = 196.686f,
                    heading = 0f
                },
                new HouseInterior
                {
                    id = 3,
                    pos_x = -786.956f,
                    pos_y = 315.623f,
                    pos_z = 187.914f,
                    heading = 0f
                },
                new HouseInterior
                {
                    id = 4,
                    pos_x = -773.955f, 
                    pos_y = 341.989f,
                    pos_z = 196.686f,
                    heading = 0f
                },
                new HouseInterior
                {
                    id = 5,
                    pos_x = -774.022f, 
                    pos_y = 342.172f,
                    pos_z =196.686f,
                    heading = 0f
                }
            };

            using var db = new RXContext();

            foreach (DbHouse house in await db.Houses.ToListAsync())
            {
                house.RentList = NAPI.Util.FromJson<List<uint>>(house.rents);
                dbHouses.Add(house);

                var mcb = await NAPI.Entity.CreateMCB(new Vector3(house.posX, house.posY, house.posZ), new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.VerticalCylinder, false); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um das Hausmenü zu öffnen!",
                    Color = "lightblue",
                    Duration = 3500,
                    Title = "Haus (" + house.id + ")",
                };

                mcb.ColShape.OnEntityEnterColShape += (s, player) =>
                {
                    player.SetData<uint>("HouseId", house.id);
                };

                mcb.ColShape.OnEntityExitColShape += (s, player) =>
                {
                    player.ResetData("HouseId");
                };

                mcb.ColShape.Action = async player => await OpenHouseMenu(player, house);

                var interior = HouseInteriors.Find(x => x.id == house.interiorid);

                if (interior == null) return;
                var mcb3 = await NAPI.Entity.CreateMCB(ServerPos, new Color(0, 0, 0), house.id, 1.4f); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                mcb3.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um auf deinen Server zuzugreifen!",
                    Color = "yellow",
                    Duration = 3500,
                    Title = "Server",
                };

                mcb3.ColShape.OnEntityEnterColShape += (s, player) =>
                {
                    player.SetData<uint>("ServerId", house.id);
                };

                mcb3.ColShape.OnEntityExitColShape += (s, player) =>
                {
                    player.ResetData("ServerId");
                };

                mcb3.ColShape.Action = async player => await OpenBasementComputer(player, house);

                var mcb4 = await NAPI.Entity.CreateMCB(WorkstationPos, new Color(0, 0, 0), house.id, 1.4f); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                mcb4.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um auf deine Werkbank zuzugreifen!",
                    Color = "green",
                    Duration = 3500,
                    Title = "Werkbank",
                };

                mcb4.ColShape.Action = async player => await OpenBasementWorkstation(player, house);

                await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("tr_prop_tr_corp_servercln_01a"), ServerPos, new Vector3(0, 0, -178.01), (byte)255, house.id));


                var mcb2 = await NAPI.Entity.CreateMCB(new Vector3(interior.wardrobe_x, interior.wardrobe_y, interior.wardrobe_z), new Color(0, 0, 0), house.id, 1.4f); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                mcb2.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um dich umzuziehen!",
                    Color = "black",
                    Duration = 3500,
                    Title = "Kleiderschrank",
                };

                mcb2.ColShape.Action = async player => await WardrobeModule.OpenWardrobe(player);

                if (house.ownerID != 0)
                {

                    var container = await db.Containers.FirstOrDefaultAsync(x => x.Id == house.container_id);
                    if (container == null)
                    {
                        var dbhouse = await db.Houses.FirstOrDefaultAsync(x => x.id == house.id);

                        container = new DbContainer
                        {
                            Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                            Name = "Schrank",
                            MaxSlots = 60,
                            MaxWeight = 300000,
                        };

                        dbhouse.container_id = container.Id;

                        await db.Containers.AddAsync(container);
                    }

                    await db.SaveChangesAsync();

                    await NAPI.Task.RunAsync(() =>
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(new Vector3(interior.inv_x, interior.inv_y, interior.inv_z), 3f, 3f, house.id);

                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = container.Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Schrank";
                        colShape.ContainerRestrictedPlayer = house.ownerID;
                    });
                }
            }
        }

        public override async Task OnFifteenMinutes()
        {
            using var db = new RXContext();

            var serverList = await db.HouseServers.ToListAsync();

            foreach (var server in serverList)
            {
                if (!server.isActive) continue;

                var house = await db.Houses.FirstOrDefaultAsync(x => x.id == server.houseid);

                if (house == null) continue;

                var player = await PlayerController.FindPlayerById(house.ownerID);

                if (player == null) continue;

                if (server.RAM == 0 || server.CPU == 0 || server.GraphicCard == 0 || server.Netzteil == 0) continue;

                uint cpu = server.CPU;
                uint gpu = server.GraphicCard;

                uint calcValue = 0;

                if (gpu == 1 && cpu == 2)
                {
                    calcValue += (uint)new Random().Next(4);
                } else if (gpu == 2 && cpu == 1)
                {
                    calcValue += (uint)new Random().Next(4);
                } else if (gpu == 2 && cpu == 2)
                {
                    calcValue += (uint)new Random().Next(9);
                } else if (gpu == 3 && cpu == 1)
                {
                    calcValue += (uint)new Random().Next(8);
                }
                else if (gpu == 3 && cpu == 2)
                {
                    calcValue += (uint)new Random().Next(12);
                }

                server.CryptoValue += calcValue;

                db.HouseServers.Update(server);

                await db.HouseServerLogs.AddAsync(new DbHouseServerLogs() { playerid = server.id, serverid = server.id, value = calcValue, date = DateTime.Now.ToLocalTime().ToString("dd\\/MM\\/yyyy h\\:mm") });

                await db.SaveChangesAsync();


            }

        }

        
        
        public async Task OpenBasementWorkstation(RXPlayer player, DbHouse house)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;
            List<BlueprintBasementData> blueprints = new List<BlueprintBasementData>();

            List<ItemBasementData> items = new List<ItemBasementData>();

            int id = 1;
            foreach (RXItem item in player.Container.Slots)
            {
                var final = new ItemBasementData() { Id = (uint)id, item_id = item.ItemModelId, Amount = (uint)item.Amount, Name = ItemModelModule.ItemModels.Find(x => x.Id == item.ItemModelId).Name };
                id++;
            items.Add(final);
        }

            blueprints.Add(new BlueprintBasementData() {  Id = 2, BlueprintId = 2, CanUse = 10});

        var data = new WerkbankData() { Level = house.werkbank, Blueprints = blueprints, Items = items};

            RXWindow window = new RXWindow("BasementWorkstation");


            await window.OpenWindow(player, data);
        }

        public async Task OpenBasementComputer(RXPlayer player, DbHouse house)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            using var db = new RXContext();

            var server = await db.HouseServers.FirstOrDefaultAsync(x => x.houseid == house.id);

            if (server == null)
            {
                server = new DbHouseServer
                {
                    id = await db.HouseServers.CountAsync() == 0 ? 1 : (await db.HouseServers.MaxAsync(con => con.id) + 1),
                    houseid = house.id,
                    isActive = false,
                    GraphicCard = 0,
                    CPU = 0,
                    Netzteil = 0,
                    RAM = 0,
                    CryptoValue = 0
                };

                await db.HouseServers.AddAsync(server);
                await db.SaveChangesAsync();
            }

            RXWindow window = new RXWindow("BasementServer");


            await window.OpenWindow(player, server);
        }

        public async Task OpenHouseMenu(RXPlayer player, DbHouse house)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (house.ownerID == 0)
            {
                object confirmationBoxObject = new
                {
                    t = "Willst du das Haus (" + house.id + ") für " + house.price + "$ kaufen?",
                    ft = "Kaufen",
                    st = "Schließen",
                    fe = "buyhouse",
                    se = "Close",
                    d = house.id,
                };

                var confirmation = new RXWindow("Confirm");

                await confirmation.OpenWindow(player, confirmationBoxObject);

                return;
            }

            uint controlLevel = 0;

            if (house.ownerID == player.Id)
            {
                controlLevel = 2;
            } else if (house.rents.Contains(player.Id.ToString()))
            {
                controlLevel = 1;
            }


            var obj = new HouseMenu { id = house.id, basement = house.keller, basementLevel = house.kellerlevel, canControl = controlLevel, canRent = true, houswert = house.price, klingelschild = house.note, isInside = false, server = house.server, workstation = house.werkbank, rent = house.rentprice, locked = house.Locked, money = house.money};
       

            await this.Window.OpenWindow(player, obj);

        }

        [RemoteEvent]
        public async Task LeaveHouse(RXPlayer player, uint houseid)
        {
            if (player == null) return;

            using var db = new RXContext();

            DbHouse house = dbHouses.Find(x => x.id == houseid);

            if (house == null)
            {
                await player.SendNotify("Dieses Haus wurde nicht gefunden!");
                return;
            }

            if (house.ownerID == 0)
            {
                await player.SendNotify("Dieses Haus gehört niemanden!");
                return;
            }

            var interior = HouseInteriors.Find(x => x.id == house.interiorid);

            if (interior == null)
            {
                await player.SendNotify("Das Interior dieses Haus konnte nicht geladen werden!");
                return;
            }

            if (house.Locked)
            {
                await player.SendNotify("Die Tür ist abgeschlossen!", 3500, "red");
            }
            else
            {
                  await player.SendNotify("Du hast das Haus verlassen!", 3500, "lightblue", "Haus (" + house.id + ")");

                     await player.SetDimensionAsync(0);
                  await player.SetPositionAsync(new Vector3(house.posX, house.posY, house.posZ));
                    player.ResetData("IsInHouse");
            }

        }


        [RemoteEvent]
        public async Task UpdateDoorBellSign(RXPlayer player, uint houseid, string note)
        {
            if (player == null) return;

            using var db = new RXContext();

            var house = dbHouses.Find(x => x.id == houseid);

            if (house == null)
            {
                await player.SendNotify("Dieses Haus wurde nicht gefunden!");
                return;
            }

            if (house.ownerID == 0)
            {
                await player.SendNotify("Dieses Haus gehört niemanden!");
                return;
            }

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dazu hast du keine Berechtigung!");
                return;
            }
            await player.SendNotify("Du hast den Klingelschild Text geändert!", 3500, "lightblue", "Haus (" + house.id + ")");
            house.note = note;
            await db.SaveChangesAsync();
        }

        [RemoteEvent]
        public async Task ChangeDefaultRentCost(RXPlayer player, uint houseid, uint price)
        {
            if (player == null) return;

            using var db = new RXContext();

            var house = dbHouses.Find(x => x.id == houseid);

            if (house == null)
            {
                await player.SendNotify("Dieses Haus wurde nicht gefunden!");
                return;
            }

            if (house.ownerID == 0)
            {
                await player.SendNotify("Dieses Haus gehört niemanden!");
                return;
            }

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dazu hast du keine Berechtigung!");
                return;
            }
            await player.SendNotify("Du hast den Mietpreis geändert!", 3500, "lightblue", "Haus (" + house.id + ")");
            house.rentprice = price;
            await db.SaveChangesAsync();
        }

        [RemoteEvent]
        public async Task RqCryptoMiningLogs(RXPlayer player, uint serverid)
        {
            if (player == null) return;
            using var db = new RXContext();
            var server = await db.HouseServerLogs.Where(x => x.serverid == serverid).ToListAsync();

            if (server == null) return;

            await player.TriggerEventAsync("RsCryptoMiningLogs", NAPI.Util.ToJson(server));


        }

        [RemoteEvent]
        public async Task TakeCryptoFromServer(RXPlayer player, uint serverid)
        {
            if (player == null) return;
            using var db = new RXContext();
            var server = await db.HouseServers.FirstOrDefaultAsync(x => x.id == serverid);

            if (server == null) return;
            var house = dbHouses.Find(x => x.id == server.houseid);

            if (house == null)
            {
                await player.SendNotify("Dieses Haus wurde nicht gefunden!");
                return;
            }

            if (house.ownerID == 0)
            {
                await player.SendNotify("Dieses Haus gehört niemanden!");
                return;
            }

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dazu hast du keine Berechtigung!");
                return;
            }


            if (server.CryptoValue < 1)
            {
                await player.SendNotify("Du hast zu wenig VisCoins auf dieser Wallet!");
                return;
            }

            player.WalletValue += server.CryptoValue;

            await player.SendNotify("Die Wallet des Computers sendet " + server.CryptoValue + " VisCoins an folgende Adresse + " + player.WalletAdress + "!");
            await db.HouseServerLogs.AddAsync(new DbHouseServerLogs() { playerid = server.id, serverid = server.id, value = server.CryptoValue, date = DateTime.Now.ToLocalTime().ToString("dd\\/MM\\/yyyy h\\:mm") });

            server.CryptoValue = 0;

            db.HouseServers.Update(server);

            await db.SaveChangesAsync();


        }


        [RemoteEvent]
        public async Task ToggleCryptoServer(RXPlayer player, uint serverid)
        {
            if (player == null) return;

            using var db = new RXContext();
            var server = await db.HouseServers.FirstOrDefaultAsync(x => x.id == serverid);

            var house = dbHouses.Find(x => x.id == server.houseid);

            if (house == null)
            {
                await player.SendNotify("Dieses Haus wurde nicht gefunden!");
                return;
            }

            if (house.ownerID == 0)
            {
                await player.SendNotify("Dieses Haus gehört niemanden!");
                return;
            }

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dazu hast du keine Berechtigung!");
                return;
            }


            if (server.isActive)
            {
                await player.SendNotify("Farming wurde gestoppt!", 3500, "yellow", "Server (" + server.id + ")");
                server.isActive = false;
            } else
            {
                await player.SendNotify("Farming wurde gestartet!", 3500, "yellow", "Server (" + server.id + ")");
                server.isActive = true;
            }

            db.HouseServers.Update(server);
            await db.SaveChangesAsync();

        }


        [RemoteEvent]
        public async Task TakeHouseMoney(RXPlayer player, uint houseid)
        {
            if (player == null) return;

            using var db = new RXContext();

            var house = dbHouses.Find(x => x.id == houseid);

            if (house == null)
            {
                await player.SendNotify("Dieses Haus wurde nicht gefunden!");
                return;
            }

            if (house.ownerID == 0)
            {
                await player.SendNotify("Dieses Haus gehört niemanden!");
                return;
            }

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dazu hast du keine Berechtigung!");
                return;
            }

            if (house.money < 1)
            {
                await player.SendNotify("Es ist nicht genug Geld auf dem Hauskonto!");
                return;
            }

            await player.SendNotify("Du hast " + house.money + "$ von dem Hauskonto abgehoben!", 3500, "lightblue", "Haus (" + house.id + ")");
            house.money = 0;
            await db.SaveChangesAsync();
        }

        //1138.0089 -3199.0076 -39.665684 Heading: 0,95152724

        [RemoteEvent]
        public async Task EnterBasement(RXPlayer player, uint houseid)
        {
            if (player == null) return;

            using var db = new RXContext();

            var house = dbHouses.Find(x => x.id == houseid);

            if (house == null)
            {
                await player.SendNotify("Dieses Haus wurde nicht gefunden!");
                return;
            }

            if (house.ownerID == 0)
            {
                await player.SendNotify("Dieses Haus gehört niemanden!");
                return;
            }

            var interior = HouseInteriors.Find(x => x.id == house.interiorid);

            if (interior == null)
            {
                await player.SendNotify("Das Interior dieses Haus konnte nicht geladen werden!");
                return;
            }

            if (house.Locked)
            {
                await player.SendNotify("Die Tür ist abgeschlossen!", 3500, "red");
            }
            else
            {
                await player.SendNotify("Du hast den Keller des Hauses betreten!", 3500, "lightblue", "Haus (" + house.id + ")");

                await player.SetDimensionAsync(house.id);
                await player.SetPositionAsync(new Vector3(1138.0089, -3199.0076, -39.665684));
                player.SetData("IsInHouse", house.id);
            }

        }


        [RemoteEvent]
        public async Task EnterHouse(RXPlayer player, uint houseid)
        {
            if (player == null) return;

            using var db = new RXContext();

            var house = dbHouses.Find(x => x.id == houseid);

            if (house == null)
            {
                await player.SendNotify("Dieses Haus wurde nicht gefunden!");
                return;
            }

            if (house.ownerID == 0)
            {
                await player.SendNotify("Dieses Haus gehört niemanden!");
                return;
            }

            var interior = HouseInteriors.Find(x => x.id == house.interiorid);

            if (interior == null)
            {
                await player.SendNotify("Das Interior dieses Haus konnte nicht geladen werden!");
                return;
            }

            if (house.Locked)
            {
                await player.SendNotify("Die Tür ist abgeschlossen!", 3500, "red");
            }
            else
            {
                await player.SendNotify("Du hast das Haus betreten!", 3500, "lightblue", "Haus (" + house.id + ")");

                await player.SetDimensionAsync(house.id);
                await player.SetPositionAsync(new Vector3(interior.pos_x, interior.pos_y, interior.pos_z));
                player.SetData("IsInHouse", house.id);
            }

        }

        public static async Task<DbHouse> getHouse(RXPlayer player)
        {
            using var db = new RXContext();
            foreach (DbHouse house in db.Houses)
            {
                Vector3 vec = new Vector3(house.posX, house.posY, house.posZ);
                Vector3 ppos = await player.GetPositionAsync();
                if (vec.DistanceTo(ppos) < 2.5) {
                    return house;
                }
            }
            return null;
        }

        public override async Task PressedE(RXPlayer player)
        {
            uint playerdim = await player.GetDimensionAsync();



            if (player.HasData("IsInHouse") && playerdim != 0)
            {
                Vector3 playerpos = await player.GetPositionAsync();

                using var db = new RXContext();

                var house = dbHouses.Find(x => x.id == player.GetData<uint>("IsInHouse"));

                if (house == null) return;
                

                var interior = HouseInteriors.Find(x => x.id == house.interiorid);

                if (interior == null)
                {
                    await player.SendNotify("Das Interior dieses Haus konnte nicht geladen werden!");
                    return;
                }

                Vector3 leavepos = new Vector3(interior.pos_x, interior.pos_y, interior.pos_z);
                Vector3 leavepos2 = new Vector3(1138.0089, -3199.0076, -39.665684);

                if (leavepos.DistanceTo(playerpos) < 2.5 || leavepos2.DistanceTo(playerpos) < 2.5)
                {

                    uint controlLevel = 0;

                    if (house.ownerID == player.Id)
                    {
                        controlLevel = 2;
                    }
                    else if (house.rents.Contains(player.Id.ToString()))
                    {
                        controlLevel = 1;
                    }


                    var obj = new HouseMenu { id = house.id, basement = house.keller, basementLevel = house.kellerlevel, canControl = controlLevel, canRent = true, houswert = house.price, klingelschild = house.note, isInside = true, server = house.server, workstation = house.werkbank, rent = house.rentprice, locked = house.Locked, money = house.money };


                    await this.Window.OpenWindow(player, obj);
                }


            }
        }

        [RemoteEvent]
        public async Task UpdateHouseLock(RXPlayer player, uint houseid, bool state)
        {
            using var db = new RXContext();

            var house = dbHouses.Find(x => x.id == houseid);

            if (house == null) return;

            if (house.ownerID != player.Id)
            {
                if (!house.RentList.Contains(player.Id))
                    return;
            }


            if (!state)
            {
                await player.SendNotify("Haustür aufgeschlossen!", 3500, "green");
                house.Locked = false;
            }
            else
            {
                await player.SendNotify("Haustür zugeschlossen!", 3500, "red");
                house.Locked = true;
            }
            await db.SaveChangesAsync();
           
            
        }

        [RemoteEvent]
        public async Task buyhouse(RXPlayer player, uint houseid)
        {
            if (player == null) return;

            if (player.HouseId != 0)
            {
                await player.SendNotify("Du besitzt bereits ein Haus!");
                return;
            }

            using var db = new RXContext();

            var house = dbHouses.Find(x => x.id == houseid);
            if (house == null) return;

            if (house.ownerID != 0)
            {
                await player.SendNotify("Dieses Haus gehört bereits jemanden!");
                return;
            }

            PaymentModule.CreatePayment(player, (int)house.price, async player =>
            {
                await RX.GiveMoneyToStaatskonto((int)house.price, "Hauskauf - " + house.id + " - " + await player.GetNameAsync());

                await player.SendNotify("Du hast erfolgreich das Haus mit der Nummer " + house.id + " für " + house.price + "$ gekauft!", 3500, "lightblue", "Haus");
                house.rents = "[]";
                house.RentList = new List<uint>();
                house.ownerID = player.Id;
                house.inv_cash = 0;
                house.bl_amount = 0;
                player.HouseId = house.id;

                using var db = new RXContext();
                var dbHouse = await db.Houses.FirstOrDefaultAsync(x => x.id == house.id);
                if (dbHouse == null) return;
                dbHouse.rents = "[]";
                dbHouse.RentList = new List<uint>();
                dbHouse.ownerID = player.Id;
                dbHouse.inv_cash = 0;
                dbHouse.bl_amount = 0;

                await db.SaveChangesAsync();

            }, "Hauskauf");
        }

        [RemoteEvent]
        public async Task AbandonHouse(RXPlayer player, uint houseId)
        {
            if (player == null) return;

            if (player.HouseId != houseId || player.HouseId == 0 || houseId == 0) return;

            using var db = new RXContext();

            var dbHouse = await db.Houses.FirstOrDefaultAsync(x => x.id == houseId);
            if (dbHouse == null || dbHouse.ownerID == 0 || dbHouse.ownerID != player.Id) return;

            var house = dbHouses.Find(x => x.id == houseId);
            if (house == null) return;

            dbHouse.ownerID = 0;
            dbHouse.rents = "[]";
            dbHouse.RentList = new List<uint>();
            dbHouse.Locked = true;
            dbHouse.rentprice = 300;

            house.ownerID = 0;
            house.rents = "[]";
            house.RentList = new List<uint>();
            house.Locked = true;
            house.rentprice = 300;

            var dbPlayer = await db.Players.FirstOrDefaultAsync(p => p.Id == player.Id);
            if (dbPlayer == null) return;
            player.HouseId = 0;
            dbPlayer.HouseId = 0;

            await db.SaveChangesAsync();
            await RX.TakeMoneyFromStaatskonto((int)dbHouse.price / 2, $"Hausverkauf - {dbHouse.id} - {await player.GetNameAsync()}");
            await player.GiveMoney((int)dbHouse.price / 2);

            await player.SendNotify($"Du hast dein Haus für {dbHouse.price / 2}$ verkauft!");
        }
    }
}
