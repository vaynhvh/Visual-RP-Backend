using Backend.Models;
using Backend.Modules.Gas;
using Backend.Modules.Native;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Storage
{

    public class StorageRoomObject
    {

        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "o")]
        public uint IsOwner { get; set; }

        [JsonProperty(PropertyName = "l")]
        public bool IsLocked { get; set; }

        [JsonProperty(PropertyName = "cl")]
        public bool ChestLock { get; set; }

        [JsonProperty(PropertyName = "ll")]
        public uint LagerLevel { get; set; }

        [JsonProperty(PropertyName = "s")]
        public bool Outside { get; set; }
    }
    class StorageModule : RXModule
    {
        public StorageModule() : base("Storage", new RXWindow("Storageroom")) { }

        public static List<DbStorage> Storages = new List<DbStorage>();

        public static int MaxStorageRooms = 5;

        public static Vector3 SmallWareHouse = new Vector3(1087.3651, -3099.4084, -38.99992);
        public static Vector3 Kiste1 = new Vector3(1088.8152, - 3096.7385, - 38.999954);
        public static Vector3 Kiste2 = new Vector3(1091.3251, - 3096.9067, - 38.999954);
        public static Vector3 Kiste3 = new Vector3(1095.08, - 3096.6863, - 38.999954);
        public static Vector3 Kiste4 = new Vector3(1097.7529, - 3096.65, - 38.999954);
        public static Vector3 Kiste5 = new Vector3(1101.3629, - 3096.5864, - 38.999954);
        public static Vector3 Kiste6 = new Vector3(1103.8866, - 3096.6873, - 38.999954);

        public static string image = "https://cdn.discordapp.com/attachments/1059566474328559686/1061737525967851630/storage.jpg";

        public override async void LoadAsync()
        {
            await Task.Delay(4000);

            using var db = new RXContext();
            Storages = await db.Storages.ToListAsync();

            foreach (var storage in Storages)
            {
                var mcb = await NAPI.Entity.CreateMCB(storage.Position.ToPos(), new Color(255, 140, 0), 0u, 1.4f, 1.4f, false, MarkerType.VerticalCylinder, false); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                mcb.ColShape.Message = new RXMessage
                    {
                        Text = $"Drücke E um auf die Lagerhalle zuzugreifen!",
                        Color = "brown",
                        Duration = 3500,
                        Title = "Lagerhalle (" + storage.Id + ")",
                    };

                mcb.ColShape.Action = async player => await OpenStorageMenu(player, storage);

                    mcb.ColShape.SetData("Storage", storage);

     
                    var ausgang = await NAPI.Entity.CreateMCB(SmallWareHouse, new Color(255, 140, 0), storage.Id, 1.4f, 1.4f, false, MarkerType.VerticalCylinder, false); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                    ausgang.ColShape.Message = new RXMessage
                    {
                        Text = $"Drücke E um auf die Lagerhalle zuzugreifen!",
                        Color = "brown",
                        Duration = 3500,
                        Title = "Lagerhalle (" + storage.Id + ")",
                    };

                    ausgang.ColShape.Action = async player => await OpenStorageMenu(player, storage, true);


                if (storage.OwnerId != 0)
                {


                    {
                        var container = await db.Containers.FirstOrDefaultAsync(x => x.Id == storage.Container1Id);
                        if (container == null)
                        {
                            var dbstorage = await db.Storages.FirstOrDefaultAsync(x => x.Id == storage.Id);

                            container = new DbContainer
                            {
                                Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                                Name = "Lagerhalle (Kiste 1)",
                                MaxSlots = (int)storage.ContainerSlots,
                                MaxWeight = (int)storage.ContainerWeight,
                            };

                            dbstorage.Container1Id = container.Id;

                            await db.Containers.AddAsync(container);
                        }

                    }
                    await db.SaveChangesAsync();


                    {
                        var container = await db.Containers.FirstOrDefaultAsync(x => x.Id == storage.Container2Id);
                        if (container == null)
                        {
                            var dbstorage = await db.Storages.FirstOrDefaultAsync(x => x.Id == storage.Id);

                            container = new DbContainer
                            {
                                Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                                Name = "Lagerhalle (Kiste 2)",
                                MaxSlots = (int)storage.ContainerSlots,
                                MaxWeight = (int)storage.ContainerWeight,
                            };

                            dbstorage.Container2Id = container.Id;

                            await db.Containers.AddAsync(container);
                        }

                    }
                    await db.SaveChangesAsync();


                    {
                        var container = await db.Containers.FirstOrDefaultAsync(x => x.Id == storage.Container3Id);
                        if (container == null)
                        {
                            var dbstorage = await db.Storages.FirstOrDefaultAsync(x => x.Id == storage.Id);

                            container = new DbContainer
                            {
                                Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                                Name = "Lagerhalle (Kiste 3)",
                                MaxSlots = (int)storage.ContainerSlots,
                                MaxWeight = (int)storage.ContainerWeight,
                            };

                            dbstorage.Container3Id = container.Id;

                            await db.Containers.AddAsync(container);
                        }

                    }
                    await db.SaveChangesAsync();


                    {
                        var container = await db.Containers.FirstOrDefaultAsync(x => x.Id == storage.Container4Id);
                        if (container == null)
                        {
                            var dbstorage = await db.Storages.FirstOrDefaultAsync(x => x.Id == storage.Id);

                            container = new DbContainer
                            {
                                Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                                Name = "Lagerhalle (Kiste 4)",
                                MaxSlots = (int)storage.ContainerSlots,
                                MaxWeight = (int)storage.ContainerWeight,
                            };

                            dbstorage.Container4Id = container.Id;

                            await db.Containers.AddAsync(container);
                        }

                    }
                    await db.SaveChangesAsync();

                    {
                        var container = await db.Containers.FirstOrDefaultAsync(x => x.Id == storage.Container5Id);
                        if (container == null)
                        {
                            var dbstorage = await db.Storages.FirstOrDefaultAsync(x => x.Id == storage.Id);

                            container = new DbContainer
                            {
                                Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                                Name = "Lagerhalle (Kiste 5)",
                                MaxSlots = (int)storage.ContainerSlots,
                                MaxWeight = (int)storage.ContainerWeight,
                            };

                            dbstorage.Container5Id = container.Id;

                            await db.Containers.AddAsync(container);
                        }

                    }
                    await db.SaveChangesAsync();

                    {
                        var container = await db.Containers.FirstOrDefaultAsync(x => x.Id == storage.Container6Id);
                        if (container == null)
                        {
                            var dbstorage = await db.Storages.FirstOrDefaultAsync(x => x.Id == storage.Id);

                            container = new DbContainer
                            {
                                Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                                Name = "Lagerhalle (Kiste 6)",
                                MaxSlots = (int)storage.ContainerSlots,
                                MaxWeight = (int)storage.ContainerWeight,
                            };

                            dbstorage.Container6Id = container.Id;

                            await db.Containers.AddAsync(container);
                        }

                    }
                    await db.SaveChangesAsync();

                    



                }
            }
        }

        public async Task SpawnStorageObjects(DbStorage storage)
        {
            await NAPI.Task.RunAsync(() =>
            {

                if (storage.Kiste1 != null)
                {
                    NAPI.Task.Run(() => storage.Kiste1.Delete());
                }
                if (storage.Kiste2 != null)
                {
                    NAPI.Task.Run(() => storage.Kiste2.Delete());
                }
                if (storage.Kiste3 != null)
                {
                    NAPI.Task.Run(() => storage.Kiste3.Delete());
                }
                if (storage.Kiste4 != null)
                {
                    NAPI.Task.Run(() => storage.Kiste4.Delete());
                }
                if (storage.Kiste5 != null)
                {
                    NAPI.Task.Run(() => storage.Kiste5.Delete());
                }
                if (storage.Kiste6 != null)
                {
                    NAPI.Task.Run(() => storage.Kiste6.Delete());
                }


                if (storage.Ausbaustufe == 0)
                {
                    storage.Kiste1 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste1.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                }
                else if (storage.Ausbaustufe == 1)
                {
                    storage.Kiste1 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste1.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste2 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste2.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                }
                else if (storage.Ausbaustufe == 2)
                {
                    storage.Kiste1 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste1.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste2 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste2.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste3 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste3.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                }
                else if (storage.Ausbaustufe == 3)
                {
                    storage.Kiste1 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste1.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste2 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste2.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste3 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste3.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste4 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste4.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                }
                else if (storage.Ausbaustufe == 4)
                {
                    storage.Kiste1 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste1.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste2 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste2.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste3 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste3.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste4 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste4.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste5 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste5.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                }
                else if (storage.Ausbaustufe == 5)
                {
                    storage.Kiste1 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste1.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste2 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste2.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste3 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste3.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste4 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste4.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste5 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste5.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);
                    storage.Kiste6 = NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_box_wood01a"), Kiste6.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 180f), 255, storage.Id);

                }
                     if (storage.Ausbaustufe == 0)
                     {
                         var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste1, 1.2f, 1.2f, storage.Id);


                         colShape.IsContainerColShape = true;
                         colShape.ContainerId = storage.Container1Id;
                         colShape.ContainerOpen = true;
                         colShape.ContainerCustomName = "Lagerhalle";
                         colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                }
                else if (storage.Ausbaustufe == 1)
                     {

                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste1, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container1Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }

                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste2, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container2Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                }
                     else if(storage.Ausbaustufe == 2)
                     {

                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste1, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container1Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }

                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste2, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container2Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste3, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container3Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                } else if (storage.Ausbaustufe == 3)
                {

                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste1, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container1Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }

                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste2, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container2Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste3, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container3Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste4, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container4Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                } else if (storage.Ausbaustufe == 4)
                {
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste1, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container1Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }

                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste2, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container2Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste3, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container3Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste4, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container4Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste5, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container5Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                } else
                {
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste1, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container1Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }

                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste2, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container2Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste3, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container3Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste4, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container4Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste5, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container5Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId; colShape.ContainerType = 5;

                    }
                    {
                        var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(Kiste6, 1.2f, 1.2f, storage.Id);


                        colShape.IsContainerColShape = true;
                        colShape.ContainerId = storage.Container6Id;
                        colShape.ContainerOpen = true;
                        colShape.ContainerCustomName = "Lagerhalle";
                        colShape.ContainerRestrictedPlayer = storage.OwnerId;
                        colShape.ContainerType = 5;
                    }
                }

            });
        }

        public async Task OpenStorageMenu(RXPlayer player, DbStorage storage, bool inside = false)
        {

            if (player == null) return;

            if (storage.OwnerId == 0)
            {

                object confirmationBoxObject = new
                {
                    t = "Willst du die Lagerhalle (" + storage.Id + ") für " + storage.Price + "$ kaufen?",
                    ft = "Kaufen",
                    st = "Schließen",
                    fe = "BuyStorage",
                    se = "Close",
                    d = storage.Id,
                };

                var confirmation = new RXWindow("Confirm");

                await confirmation.OpenWindow(player, confirmationBoxObject);
            }
            else
            {

                uint ownerlevel = 0;

                if (storage.OwnerId == player.Id)
                {
                    ownerlevel = 2;
                }
                else if (player.Storages.Contains(storage.Id))
                {
                    ownerlevel = 1;
                }

                var storageRoom = new StorageRoomObject() { Id = storage.Id, ChestLock = storage.ChestLocked, IsLocked = storage.Locked, LagerLevel = (uint)storage.Ausbaustufe, IsOwner = ownerlevel, Outside = inside };

                await this.Window.OpenWindow(player, storageRoom);
            }
        }
        [RemoteEvent]
        public async Task EnterStorageroom(RXPlayer player, uint storageid)
        {
            if (player == null) return;
            var storage = Storages.Find(x => x.Id == storageid);

            if (storage == null) return;
            if (storage.Locked == true)
            {

                await player.SendNotify($"Die Tür ist abgeschlossen!", 3500, "red");
                return;
            }
            await SpawnStorageObjects(storage);

            await player.SetPositionAsync(SmallWareHouse);
            await player.SetDimensionAsync(storage.Id);


        }

        [RemoteEvent]
        public async Task LeaveStorageroom(RXPlayer player, uint storageid)
        {
            if (player == null) return;
            var storage = Storages.Find(x => x.Id == storageid);

            if (storage == null) return;
            if (storage.Locked == true)
            {

                await player.SendNotify($"Die Tür ist abgeschlossen!", 3500, "red");
                return;
            }

            await player.SetDimensionAsync(0);
            await player.SetPositionAsync(storage.Position.ToPos());
        }

        [RemoteEvent]
        public async Task UpdateStorageroomLock(RXPlayer player, uint storageid, bool state)
        {
            if (player == null) return;
            var storage = Storages.Find(x => x.Id == storageid);

            if (storage== null) return;
            if (storage.OwnerId != player.Id) return;

            if (state)
            {
                await player.SendNotify($"Tür abgeschlossen", 3500, "red");
                storage.Locked = true;
            } else
            {


                await player.SendNotify($"Tür aufgeschlossen", 3500, "green");
                storage.Locked = false;
            }
        }

        [RemoteEvent]
        public async Task UpdateStorageroomChestLock(RXPlayer player, uint storageid, bool state)
        {
            if (player == null) return;
            var storage = Storages.Find(x => x.Id == storageid);

            if (storage == null) return;
            if (storage.OwnerId != player.Id) return;

            if (state)
            {
                await player.SendNotify($"Kisten verriegelt", 3500, "red");
                storage.ChestLocked = true;
            }
            else
            {


                await player.SendNotify($"Kisten entriegelt", 3500, "green");
                storage.ChestLocked = false;
            }
        }

        [RemoteEvent]
        public async Task BuyStorage(RXPlayer player, uint storageid)
        {
            if (player == null) return;

            var storage = Storages.Find(x => x.Id == storageid);
            if (storage == null) return;

            if (storage.OwnerId == 0)
            {
                if (player.Storages.Count >= MaxStorageRooms)
                {
                    await player.SendNotify($"Du hast zu viele Lager! ({MaxStorageRooms})!");
                    return;
                }
                if (await player.BankAccount.TakeBankMoney(storage.Price))
                {
                    using var db = new RXContext();
                    var dbstorage = await db.Storages.FirstOrDefaultAsync(x => x.Id == storage.Id);

                    dbstorage.OwnerId = player.Id;

                    await db.SaveChangesAsync();

                    storage.OwnerId = player.Id;

                    player.Storages.Add(storage.Id);

             //       var colshape = NAPI.Pools.GetAllColShapes().Cast<RXColShape>().ToList().Where(x => x.Id == storage.Id);

             //       if (colshape != null)
                  //  {
              //          colshape.FirstOrDefault().Container.Slots.Clear();
                  //      colshape.FirstOrDefault().ContainerRestrictedPlayer = player.Id;
                //    }


                    await player.SendNotify("Lager für $" + storage.Price + " gekauft!");
                    return;
                } else
                {
                    await player.SendNotify("Du hast nicht genug Geld auf der Bank!");
                }
            }

        }

        [RemoteEvent]
        public async Task UpgradeStorageroom(RXPlayer player, uint storageid)
        {
            if (player == null) return;

            var storage = Storages.Find(x => x.Id == storageid);
            if (storage == null) return;

            if (storage.OwnerId == player.Id)
            {
                if (storage.Ausbaustufe == 5)
                {
                    await player.SendNotify($"Du kannst dieses Lager nicht mehr upgraden!");
                    return;
                }
                if (await player.BankAccount.TakeBankMoney(storage.Price))
                {
                    using var db = new RXContext();
                    var dbstorage = await db.Storages.FirstOrDefaultAsync(x => x.Id == storage.Id);

                    dbstorage.Ausbaustufe = dbstorage.Ausbaustufe + 1;

                    await db.SaveChangesAsync();

                    storage.Ausbaustufe = storage.Ausbaustufe + 1;
                    await SpawnStorageObjects(storage);


                    await player.SendNotify("Lager für $" + storage.Price + " erweitert!");
                    return;
                }
                else
                {
                    await player.SendNotify("Du hast nicht genug Geld auf der Bank!");
                }
            }

        }

        public override async Task OnColShape(RXColShape shape, RXPlayer player, bool state)
        {
            if (await NAPI.Task.RunReturnAsync(() => !shape.HasData("Storage")) || !player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;

            var storage = await NAPI.Task.RunReturnAsync(() => shape.GetData<DbStorage>("Storage"));
            if (storage == null) return;

            if (state)
            {

                await NAPI.Task.RunAsync(() => player.SetData("Storage", storage));

                if (storage.OwnerId == 0)
                {
                    player.SendInfocard("Lagerraum (" + storage.Id + ")", "brown", image, 12000, 2, new List<RXPlayer.InfoCardData>
                {
                    new RXPlayer.InfoCardData{ key = "Preis", value = storage.Price + "$" },
                    new RXPlayer.InfoCardData{ key = "Max. Slots", value = storage.ContainerSlots.ToString() },
                    new RXPlayer.InfoCardData{ key =  "Max. Gewicht", value = storage.ContainerWeight.ToString() },

                });
                } else
                {
                    player.SendInfocard("Lagerraum (" + storage.Id + ")", "green", image, 12000, 2, new List<RXPlayer.InfoCardData>
                {
                     new RXPlayer.InfoCardData{ key = "Ausbaustufe", value = storage.Ausbaustufe.ToString() },
                     new RXPlayer.InfoCardData{ key = "Max. Slots", value = storage.ContainerSlots.ToString() },
                     new RXPlayer.InfoCardData{ key = "Max. Gewicht", value = storage.ContainerWeight.ToString() },

                });
                }
            }
            else
            {
                await NAPI.Task.RunAsync(() => player.ResetData("Storage"));
            }
        }

        [RXCommand("createstorage", 96)]
        public async Task createstorage(RXPlayer player, string[] args)
        {
            using var db = new RXContext();
            var coord = await player.GetPositionAsync();

            var storage = new DbStorage { Ausbaustufe = 0, CocainLabor = false, Container1Id = 0, Container2Id = 0, Container3Id = 0, Container4Id = 0, Container5Id = 0, Container6Id = 0, Locked = true, MainFlagged = false, OwnerId = 0, Price = int.Parse(args[0]), Heading = await player.GetHeadingAsync(), Position = coord.FromPos(), ContainerSlots = uint.Parse(args[1]), ContainerWeight = uint.Parse(args[2]) };

            db.Storages.Add(storage);

            await db.SaveChangesAsync();

            NAPI.Task.Run(() =>
            {
                var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(storage.Position.ToPos(), 1.4f, 1.4f, 0);
                colShape.SetData("Storage", storage);
            });

            await player.SendNotify("Storage erstellt!");
        }

    }
}
