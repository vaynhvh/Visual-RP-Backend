using Backend.Models;
using Backend.Models.Appearance;
using Backend.Modules.Bank;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.ClothingShops
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class ClothingShopModule : RXModule
    {
        public ClothingShopModule() : base("ClothingShop", new RXWindow("ClothShop")) { }

        public static List<DbClothingShop> ClothingShops = new List<DbClothingShop>();
        public static List<DbCloth> Clothes = new List<DbCloth>();

        //[HandleExceptions]
        public override async void LoadAsync()
        {
            using var db = new RXContext();

            ClothingShops = new List<DbClothingShop>
            {
                new DbClothingShop(1, "Davis", new Vector3(76.97766, -1391.165, 28.276144)),
                new DbClothingShop(2, "Alta", new Vector3(124, -221, 54)),
                new DbClothingShop(3, "Burton", new Vector3(-163, -303, 39)),
                new DbClothingShop(4, "Rockfort", new Vector3(-710, -153, 36)),
                new DbClothingShop(5, "Vespucci", new Vector3(-821, -1074, 10)),
                new DbClothingShop(6, "Paleto Bay", new Vector3(3, 6512, 31)),
                new DbClothingShop(7, "Grapeseed", new Vector3(1693, 4821, 41)),
                new DbClothingShop(8, "Harmony", new Vector3(617, 2760, 41)),
                new DbClothingShop(9, "Great Chaparral", new Vector3(-1100, 2711, 18)),
                new DbClothingShop(10, "Chumash", new Vector3(-3172, 1047, 20)),
                new DbClothingShop(11, "Würfelpark", new Vector3(425, -807, 28)),
                new DbClothingShop(12, "Del Perro", new Vector3(-1194, -772, 16)),
                new DbClothingShop(13, "Vespucci 2", new Vector3(-1337.8447, -1278.7556, 4.8648067)),
                new DbClothingShop(14, "Hüte", new Vector3(-1483.8308, -946.82355, 10.214577)),
                new DbClothingShop(15, "Juwelier", new Vector3(-622.23505, -230.90811, 38.057068)),
            };

            List<uint> shops = ClothingShops.Where(x => x.Id != 13 && x.Name != "Hüte" && x.Name != "Juwelier").ToList().ConvertAll(x => x.Id);

            // Clothes
            {
                TransferDBContextValues(await db.MaleClothes.Where(x => x.ComponentId == 3).ToListAsync(), x =>
                {
                    x.ClothingShopList = shops;
                    x.Male = true;

                    Clothes.Add(x);

                    Clothes.Add(new DbMaleCloth
                    {
                        Id = x.Id,
                        Name = x.Name,
                        TextureId = x.TextureId,
                        DrawableId = x.DrawableId,
                        Male = false,
                        Prop = x.Prop,
                        Price = x.Price,
                        ComponentId = x.ComponentId,
                        ClothingShopList = x.ClothingShopList,
                        ClothingShops = x.ClothingShops
                    });
                });

                Clothes.Add(new DbMaleProp
                {
                    Id = 1,
                    Price = 0,
                    ClothingShopList = shops,
                    ComponentId = 8,
                    DrawableId = 15,
                    TextureId = 0,
                    Name = "Normal",
                    Prop = false,
                    Male = false
                });

                Clothes.Add(new DbMaleProp
                {
                    Id = 1,
                    Price = 0,
                    ClothingShopList = shops,
                    ComponentId = 8,
                    DrawableId = 15,
                    TextureId = 0,
                    Name = "Normal",
                    Prop = false,
                    Male = true
                });

                for (int i = 0; i < 12; i++)
                {
                    int chunkBy = await db.FemaleClothes.Where(x => x.ComponentId == i && x.ComponentId != 3).CountAsync() / (ClothingShops.Count - 1) - 5;
                    if (chunkBy > 230) chunkBy = 230;

                    List<List<DbFemaleCloth>> femaleClothes = (await db.FemaleClothes.Where(x => x.ComponentId == i && x.ComponentId != 3 && x.ComponentId != 7).ToListAsync()).ChunkBy(chunkBy);

                    uint count = 1;

                    TransferDBContextValues(femaleClothes, async x =>
                    {

                        await x.forEach(obj =>
                        {
                            obj.ClothingShopList = new List<uint> { count };
                            obj.Male = false;

                            Clothes.Add(obj);
                        });

                        count++;
                    });
                }

                for (int i = 0; i < 12; i++)
                {
                    int chunkBy = await db.MaleClothes.Where(x => x.ComponentId == i && x.ComponentId != 3).CountAsync() / (ClothingShops.Count() - 1) - 5;
                    if (chunkBy > 230) chunkBy = 230;

                    List<List<DbMaleCloth>> maleClothes = (await db.MaleClothes.Where(x => x.ComponentId == i && x.ComponentId != 3 && x.ComponentId != 7).ToListAsync()).ChunkBy(chunkBy);

                    uint count = 1;

                    TransferDBContextValues(maleClothes, async x =>
                    {

                        await x.forEach(obj =>
                        {
                            obj.ClothingShopList = new List<uint> { count };
                            obj.Male = true;

                            Clothes.Add(obj);
                        });

                        count++;
                    });
                }
            }

            //Hüte
            {
                List<uint> hatShops = ClothingShops.Where(x => x.Name == "Hüte").ToList().ConvertAll(x => x.Id);

                Clothes.Add(new DbMaleProp
                {
                    Id = 1,
                    Price = 0,
                    ClothingShopList = hatShops,
                    ComponentId = 0,
                    DrawableId = -1,
                    TextureId = 0,
                    Name = "Nichts",
                    Prop = true
                });

                Clothes.Add(new DbMaleProp
                {
                    Id = 1,
                    Price = 0,
                    ClothingShopList = hatShops,
                    ComponentId = 0,
                    DrawableId = -1,
                    TextureId = 0,
                    Name = "Nichts",
                    Prop = true,
                    Male = false
                });

                await (await db.FemaleProps.Where(x => x.ComponentId == 0).ToListAsync()).ChunkBy(230)[0].forEach(x =>
                {
                    x.ClothingShopList = hatShops;
                    x.Prop = true;
                    x.Male = false;

                    Clothes.Add(x);
                });

                await (await db.MaleProps.Where(x => x.ComponentId == 0).ToListAsync()).ChunkBy(230)[0].forEach(x =>
                {
                    x.ClothingShopList = hatShops;
                    x.Prop = true;
                    x.Male = true;

                    Clothes.Add(x);
                });
            }

            // Juwelier
            {
                List<uint> juwelierShops = ClothingShops.Where(x => x.Name == "Juwelier").ToList().ConvertAll(x => x.Id);

                //Armbänder
                {
                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 7,
                        DrawableId = -1,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = true
                    });

                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 7,
                        DrawableId = -1,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = true,
                        Male = false
                    });

                    await (await db.FemaleProps.Where(x => x.ComponentId == 7).ToListAsync()).ChunkBy(230)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Prop = true;
                        x.Male = false;

                        Clothes.Add(x);
                    });

                    await (await db.MaleProps.Where(x => x.ComponentId == 7).ToListAsync()).ChunkBy(230)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Prop = true;
                        x.Male = true;

                        Clothes.Add(x);
                    });
                }

                //Uhren
                {
                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 6,
                        DrawableId = -1,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = true
                    });

                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 6,
                        DrawableId = -1,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = true,
                        Male = false
                    });

                    await (await db.FemaleProps.Where(x => x.ComponentId == 6).ToListAsync()).ChunkBy(230)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Prop = true;
                        x.Male = false;

                        Clothes.Add(x);
                    });

                    await (await db.MaleProps.Where(x => x.ComponentId == 6).ToListAsync()).ChunkBy(230)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Prop = true;
                        x.Male = true;

                        Clothes.Add(x);
                    });
                }

                //Brillen
                {
                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 1,
                        DrawableId = -1,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = true
                    });

                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 1,
                        DrawableId = -1,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = true,
                        Male = false
                    });

                    await (await db.FemaleProps.Where(x => x.ComponentId == 1).ToListAsync()).ChunkBy(230)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Prop = true;
                        x.Male = false;

                        Clothes.Add(x);
                    });

                    await (await db.MaleProps.Where(x => x.ComponentId == 1).ToListAsync()).ChunkBy(230)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Prop = true;
                        x.Male = true;

                        Clothes.Add(x);
                    });
                }

                //Ohrringe
                {
                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 2,
                        DrawableId = -1,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = true
                    });

                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 2,
                        DrawableId = -1,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = true,
                        Male = false
                    });

                    await (await db.FemaleProps.Where(x => x.ComponentId == 2).ToListAsync()).ChunkBy(70)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Prop = true;
                        x.Male = false;

                        Clothes.Add(x);
                    });

                    await (await db.MaleProps.Where(x => x.ComponentId == 2).ToListAsync()).ChunkBy(70)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Prop = true;
                        x.Male = true;

                        Clothes.Add(x);
                    });
                }

                //Ketten - Accessories
                {
                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 7,
                        DrawableId = 0,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = false,
                        Male = true
                    });

                    Clothes.Add(new DbMaleProp
                    {
                        Id = 1,
                        Price = 0,
                        ClothingShopList = juwelierShops,
                        ComponentId = 7,
                        DrawableId = 0,
                        TextureId = 0,
                        Name = "Nichts",
                        Prop = false,
                        Male = false
                    });

                    await (await db.FemaleClothes.Where(x => x.ComponentId == 7).ToListAsync()).ChunkBy(100)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Male = false;
                        x.Prop = false;

                        Clothes.Add(x);
                    });

                    await (await db.MaleClothes.Where(x => x.ComponentId == 7).ToListAsync()).ChunkBy(100)[0].forEach(x =>
                    {
                        x.ClothingShopList = juwelierShops;
                        x.Male = true;
                        x.Prop = false;

                        Clothes.Add(x);
                    });
                }
            }

            //Masken
            {
                Clothes.Add(new DbMask
                {
                    Id = 1,
                    Price = 0,
                    ClothingShopList = new List<uint> { 13 },
                    ComponentId = 1,
                    DrawableId = 0,
                    TextureId = 0,
                    Name = "Nichts"
                });

                TransferDBContextValues((await db.Masks.ToListAsync()).ChunkBy(200)[0], x =>
                {
                    x.ClothingShopList = new List<uint> { 13 };

                    Clothes.Add(x);
                });
            }

            //Shops
            await ClothingShops.forEach(async clothing =>
            {
                var mcb = await NAPI.Entity.CreateMCB(clothing.Position, new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.VerticalCylinder, true, clothing.Id == 13 ? 362 : clothing.Name == "Juwelier" ? 617 : 73, 1, clothing.Id == 13 ? "Maskenladen" : clothing.Name == "Juwelier" ? "Juwelier" : "Kleidungsladen");

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um " + (clothing.Id == 13 ? "Masken" : clothing.Name == "Hüte" ? "Hüte" : clothing.Name == "Juwelier" ? "Schmuck" : "Kleidung") + " zu kaufen!",
                    Color = "yellow",
                    Duration = 3500,
                    Title = clothing.Id == 13 ? "Maskenladen" : clothing.Name == "Hüte" ? "Hüteladen" : clothing.Name == "Juwelier" ? "Juwelier" : "Kleidungsladen " + clothing.Name
                };

                mcb.ColShape.Action = async player => await OpenShop(player, clothing.Id);
            });
        }

        //[HandleExceptions]
        public async Task OpenShop(RXPlayer player, uint shopId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            var clothingShop = ClothingShops.FirstOrDefault(x => x.Id == shopId);
            if (clothingShop == null) return;

            List<int> componentIds = new List<int>();
            List<int> propIds = new List<int>();

            var shop = new RXClothingShop
            {
                Id = (int)clothingShop.Id,
                Name = clothingShop.Name,
                Slots = new List<RXClothingSlot>()
            };

            await Clothes.Where(x => x.ClothingShopList.Contains(shopId) && (x.Male == player.IsMale || (x.ComponentId == 1 && x.Prop == false))).forEach(cloth =>
            {
                if (!componentIds.Contains(cloth.ComponentId) && !cloth.Prop)
                    componentIds.Add(cloth.ComponentId);
                else if (!propIds.Contains(cloth.ComponentId) && cloth.Prop)
                    propIds.Add(cloth.ComponentId);
            });

            var sortedComponents = new List<int>();
            var sortedProps = new List<int>();

            if (componentIds.Contains(1)) sortedComponents.Add(1);
            if (componentIds.Contains(11)) sortedComponents.Add(11);
            if (componentIds.Contains(8)) sortedComponents.Add(8);
            if (componentIds.Contains(7)) sortedComponents.Add(7);
            if (componentIds.Contains(4)) sortedComponents.Add(4);
            if (componentIds.Contains(6)) sortedComponents.Add(6);

            if (propIds.Contains(0)) sortedProps.Add(0);
            if (propIds.Contains(1)) sortedProps.Add(1);
            if (propIds.Contains(2)) sortedProps.Add(2);
            if (propIds.Contains(6)) sortedProps.Add(6);
            if (propIds.Contains(7)) sortedProps.Add(7);

            await sortedComponents.forEach(component => shop.Slots.Add(new RXClothingSlot { Id = component.ToString(), Name = player.InAduty ? component + " " + GetComponentName(component) : GetComponentName(component) }));
            await sortedProps.forEach(component => shop.Slots.Add(new RXClothingSlot { Id = "p-" + component.ToString(), Name = player.InAduty ? component + " " + GetAccessoryName(component) : GetAccessoryName(component) }));

            await this.Window.OpenWindow(player, shop);

            await NAPI.Task.RunAsync(() => player.SetData("clothingShopId", shopId));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task GetClothesByTypeId(RXPlayer player, int shopid, string clothid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await NAPI.Task.RunReturnAsync(() => !player.HasData("clothingShopId")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("clothingShopId") == 0)) return;

            bool prop = clothid.Contains("p-");

            int slot = int.Parse(clothid.Replace("p-", ""));

            uint clothingShopId = await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("clothingShopId"));

            var clothingShop = ClothingShops.FirstOrDefault(x => x.Id == clothingShopId);
            if (clothingShop == null) return;

            await Task.Delay(300);

            if (!await player.GetExistsAsync()) return;

            List<DbCloth> list = Clothes.FindAll(x => x.ClothingShopList.Contains(clothingShopId) && x.Prop == prop && x.ComponentId == slot && (x.Male == player.IsMale || (x.ComponentId == 1 && x.Prop == false)));

            await player.TriggerEventAsync("SendClothesToClient", NAPI.Util.ToJson(list));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task clothingShopUndress(RXPlayer player, string slotStr)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await NAPI.Task.RunReturnAsync(() => !player.HasData("clothingShopId")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("clothingShopId") == 0)) return;

            bool prop = slotStr.Contains("p-");

            int slot = int.Parse(slotStr.Replace("p-", ""));

            uint clothingShopId = await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("clothingShopId"));

            var clothingShop = ClothingShops.FirstOrDefault(x => x.Id == clothingShopId);
            if (clothingShop == null) return;

            using var db = new RXContext();

            var dbCharacter = await db.Characters.FirstOrDefaultAsync(x => x.Id == player.Id);
            if (dbCharacter == null) return;

            Dictionary<int, RXClothesProp> clothesParts = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Clothes);
            Dictionary<int, RXClothesProp> clothesProps = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Accessories);

            if (!prop)
            {
                if (!clothesParts.ContainsKey(slot))
                {
                    await player.SetClothesAsync(slot, 0, 0);
                }
                else
                {
                    var part = clothesParts[slot];
                    if (part == null) return;

                    await player.SetClothesAsync(slot, part.drawable, part.texture);
                }
            }
            else
            {
                if (!clothesProps.ContainsKey(slot))
                {
                    await player.SetAccessoriesAsync(slot, -1, 0);
                }
                else
                {
                    var part = clothesProps[slot];
                    if (part == null) return;

                    await player.SetAccessoriesAsync(slot, part.drawable, part.texture);
                }
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task clothingShopDress(RXPlayer player, int component, int drawable, int texture, bool prop)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await NAPI.Task.RunReturnAsync(() => !player.HasData("clothingShopId")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("clothingShopId") == 0)) return;

            uint clothingShopId = await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("clothingShopId"));

            var clothingShop = ClothingShops.FirstOrDefault(x => x.Id == clothingShopId);
            if (clothingShop == null) return;

            if (!prop)
                await player.SetClothesAsync(component, drawable, texture);
            else
                await player.SetAccessoriesAsync(component, drawable, texture);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task CloseCloth(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await NAPI.Task.RunReturnAsync(() => !player.HasData("clothingShopId")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("clothingShopId") == 0)) return;

            await player.LoadCharacter();
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task BuyClothes(RXPlayer player, int shopid, string cart)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await NAPI.Task.RunReturnAsync(() => !player.HasData("clothingShopId")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("clothingShopId") == 0)) return;

            uint clothingShopId = await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("clothingShopId"));

            var clothingShop = ClothingShops.FirstOrDefault(x => x.Id == clothingShopId);
            if (clothingShop == null) return;

            List<DbMaleCloth> cartList = JsonConvert.DeserializeObject<List<DbMaleCloth>>(cart);

            int totalAmount = 0;

            await cartList.forEach(x => totalAmount += x.Price);

            PaymentModule.CreatePaymentWithCancelOption(player, totalAmount, async player =>
            {
                using var db = new RXContext();

                var dbCharacter = await db.Characters.FirstOrDefaultAsync(x => x.Id == player.Id);
                if (dbCharacter == null) return;

                Dictionary<int, RXClothesProp> clothesParts = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Clothes);
                Dictionary<int, RXClothesProp> clothesProps = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Accessories);

                await RX.GiveMoneyToStaatskonto(totalAmount, "Kleidungsladen - Einkauf - " + await player.GetNameAsync());

                await player.SendNotify("Du hast einen Einkauf im Wert von " + totalAmount.FormatMoneyNumber() + " getätigt.", 3500, "green", "Kleidungsladen " + clothingShop.Name);

                foreach (var x in cartList)
                {
                    if (db.WardrobeItems.FirstOrDefault(item => item.PlayerId == player.Id && item.Name == x.Name && item.ComponentId == x.ComponentId) == null)
                    {
                        db.WardrobeItems.Add(new DbWardrobeItem
                        {
                            PlayerId = player.Id,
                            Name = x.Name,
                            ComponentId = x.ComponentId,
                            DrawableId = x.DrawableId,
                            TextureId = x.TextureId,
                            IsProp = x.Prop
                        });
                    }

                    if (x.Prop)
                    {
                        clothesProps[x.ComponentId] = new RXClothesProp
                        {
                            drawable = x.DrawableId,
                            texture = x.TextureId,
                            active = true,
                            clothid = (int)x.Id,

                        };
                    }
                    else
                    {
                        clothesParts[x.ComponentId] = new RXClothesProp
                        {
                            drawable = x.DrawableId,
                            texture = x.TextureId,
                            active = true,
                            clothid = (int)x.Id,
                        };
                    }
                }

                dbCharacter.Clothes = JsonConvert.SerializeObject(clothesParts);
                dbCharacter.Accessories = JsonConvert.SerializeObject(clothesProps);

                await db.SaveChangesAsync();

                await player.LoadCharacter(dbCharacter);
            }, async player => await player.LoadCharacter(), "Kleidungsladen - Einkauf");

            await player.TriggerEventAsync("ResetCam");
            await this.Window.CloseWindow(player);
        }

        //[HandleExceptions]
        public static string GetComponentName(int component)
        {
            string componentName = "Nicht gefunden";

            switch (component)
            {
                case 1:
                    componentName = "Masken";
                    break;
                case 2:
                    componentName = "Haare";
                    break;
                case 3:
                    componentName = "Körper";
                    break;
                case 4:
                    componentName = "Hosen";
                    break;
                case 6:
                    componentName = "Schuhe";
                    break;
                case 7:
                    componentName = "Ketten";
                    break;
                case 8:
                    componentName = "Unterteile";
                    break;
                case 10:
                    componentName = "Körperbemalungen";
                    break;
                case 11:
                    componentName = "Oberteile";
                    break;
            }

            return componentName;
        }

        //[HandleExceptions]
        public static string GetAccessoryName(int component)
        {
            string componentName = "Nicht gefunden";

            switch (component)
            {
                case 0:
                    componentName = "Hüte";
                    break;
                case 1:
                    componentName = "Brillen";
                    break;
                case 2:
                    componentName = "Ohrringe";
                    break;
                case 6:
                    componentName = "Uhren";
                    break;
                case 7:
                    componentName = "Armbänder";
                    break;
            }

            return componentName;
        }
    }
}
