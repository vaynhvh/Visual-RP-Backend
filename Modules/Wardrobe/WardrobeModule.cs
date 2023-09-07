using Backend.Models;
using Backend.Models.Appearance;
using Backend.Modules.ClothingShops;
using Backend.Modules.Native;
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

namespace Backend.Modules.Wardrobe
{
    class WardrobeModule : RXModule
    {
        public WardrobeModule() : base("Wardrobe", new RXWindow("Wardrobe")) { }

        public static List<DbWardrobeItem> Body = new List<DbWardrobeItem>
        {
            new DbWardrobeItem
            {
                Id = 1,
                ComponentId = 3,
                DrawableId = 0,
                Name = "Normaler Körper"
            },
            new DbWardrobeItem
            {
                Id = 2,
                ComponentId = 3,
                DrawableId = 1,
                Name = "Körper 1"
            },
            new DbWardrobeItem
            {
                Id = 3,
                ComponentId = 3,
                DrawableId = 2,
                Name = "Körper 2"
            },
            new DbWardrobeItem
            {
                Id = 4,
                ComponentId = 3,
                DrawableId = 4,
                Name = "Körper 4"
            },
            new DbWardrobeItem
            {
                Id = 5,
                ComponentId = 3,
                DrawableId = 5,
                Name = "Körper 5"
            },
            new DbWardrobeItem
            {
                Id = 6,
                ComponentId = 3,
                DrawableId = 6,
                Name = "Körper 6"
            },
            new DbWardrobeItem
            {
                Id = 7,
                ComponentId = 3,
                DrawableId = 8,
                Name = "Körper 8"
            },
            new DbWardrobeItem
            {
                Id = 8,
                ComponentId = 3,
                DrawableId = 11,
                Name = "Körper 11"
            },
            new DbWardrobeItem
            {
                Id = 9,
                ComponentId = 3,
                DrawableId = 12,
                Name = "Körper 12"
            },
            new DbWardrobeItem
            {
                Id = 10,
                ComponentId = 3,
                DrawableId = 14,
                Name = "Körper 14"
            },
            new DbWardrobeItem
            {
                Id = 11,
                ComponentId = 3,
                DrawableId = 15,
                Name = "Körper 15"
            },
        };

        //[HandleExceptions]
        public static async Task OpenWardrobe(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.CanInteractAntiFloodNoMSG(1)) return;

            RXWindow wardrobeWindow = new RXWindow("ClothShop");

            using var db = new RXContext();

            var dbCharacter = await db.Characters.FirstOrDefaultAsync(x => x.Id == player.Id);
            if (dbCharacter == null) return;


            Dictionary<int, RXClothesProp> clothesParts = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Clothes).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            Dictionary<int, RXClothesProp> clothesProps = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Accessories).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            List<DbWardrobeItem> wardrobeItems = await db.WardrobeItems.Where(x => x.PlayerId == player.Id && x.Gender == dbCharacter.Gender).ToListAsync();

            wardrobeItems.AddRange(Body);

            if (player.TeamId != 0 && player.Team != null && player.Team.TeamClothes.Count > 0)
            {
                await player.Team.TeamClothes.forEach(x => wardrobeItems.Add(new DbWardrobeItem
                {
                    Id = wardrobeItems.Max(x => x.Id) + 1,
                    ComponentId = x.ComponentId,
                    DrawableId = x.DrawableId,
                    TextureId = x.TextureId,
                    Gender = x.Gender,
                    IsProp = x.IsProp,
                    Name = x.Name,
                    PlayerId = player.Id
                }));
            }

            Dictionary<string, dynamic> wearing = new Dictionary<string, dynamic>();

            foreach (var item in clothesParts)
            {
                var wardrobeItem = wardrobeItems.FirstOrDefault(x => x.ComponentId == item.Key && x.DrawableId == item.Value.drawable && x.TextureId == item.Value.texture && !x.IsProp);
                if (wardrobeItem == null) continue;

                wearing.Add(item.Key.ToString(), new { Name = wardrobeItem.Name });
            }

            foreach (var item in clothesProps)
            {
                var wardrobeItem = wardrobeItems.FirstOrDefault(x => x.ComponentId == item.Key && x.DrawableId == item.Value.drawable && x.TextureId == item.Value.texture && x.IsProp);
                if (wardrobeItem == null) continue;

                wearing.Add("p-" + item.Key, new { Name = wardrobeItem.Name });
            }

            List<int> componentIds = new List<int>();
            List<int> propIds = new List<int>();

            await wardrobeItems.forEach(cloth =>
            {
                if (!componentIds.Contains(cloth.ComponentId) && !cloth.IsProp)
                    componentIds.Add(cloth.ComponentId);
                else if (!propIds.Contains(cloth.ComponentId) && cloth.IsProp)
                    propIds.Add(cloth.ComponentId);
            });

            var sortedComponents = new List<int>();
            var sortedProps = new List<int>();

            if (componentIds.Contains(1)) sortedComponents.Add(1);
            if (componentIds.Contains(11)) sortedComponents.Add(11);
            if (componentIds.Contains(8)) sortedComponents.Add(8);
            if (componentIds.Contains(7)) sortedComponents.Add(7);
            if (componentIds.Contains(3)) sortedComponents.Add(3);
            if (componentIds.Contains(4)) sortedComponents.Add(4);
            if (componentIds.Contains(6)) sortedComponents.Add(6);

            if (propIds.Contains(0)) sortedProps.Add(0);
            if (propIds.Contains(1)) sortedProps.Add(1);
            if (propIds.Contains(2)) sortedProps.Add(2);
            if (propIds.Contains(6)) sortedProps.Add(6);
            if (propIds.Contains(7)) sortedProps.Add(7);

            List<RXClothingSlot> slots = new List<RXClothingSlot>();

            await sortedComponents.forEach(component => slots.Add(new RXClothingSlot { Id = component.ToString(), Name = ClothingShopModule.GetComponentName(component) }));
            await sortedProps.forEach(component => slots.Add(new RXClothingSlot { Id = "p-" + component.ToString(), Name = ClothingShopModule.GetAccessoryName(component) }));


            var shop = new RXClothingShop
            {
                Id = -1,
                Name = "Kleiderschrank",
                Slots = slots
            };

            await wardrobeWindow.OpenWindow(player, shop);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task wardrobeLoadClothes(RXPlayer player, string slotStr)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;

            RXWindow wardrobeWindow = new RXWindow("Wardrobe");

            bool prop = slotStr.Contains("p-");

            if (prop) slotStr.Replace("p-", "");

            if (int.TryParse(slotStr, out int slot))
            {
                using var db = new RXContext();

                var dbCharacter = await db.Characters.FirstOrDefaultAsync(x => x.Id == player.Id);
                if (dbCharacter == null) return;

                List<DbWardrobeItem> wardrobeItemsDb = await db.WardrobeItems.Where(x => x.PlayerId == player.Id && x.Gender == dbCharacter.Gender).ToListAsync();

                if (player.TeamId != 0 && player.Team != null && player.Team.TeamClothes.Count > 0)
                {
                    await player.Team.TeamClothes.forEach(x => wardrobeItemsDb.Add(new DbWardrobeItem
                    {
                        Id = wardrobeItemsDb.Max(x => x.Id) + 1,
                        ComponentId = x.ComponentId,
                        DrawableId = x.DrawableId,
                        TextureId = x.TextureId,
                        Gender = x.Gender,
                        IsProp = x.IsProp,
                        Name = x.Name,
                        PlayerId = player.Id
                    }));
                }

                List<DbWardrobeItem> wardrobeItems = wardrobeItemsDb.Where(x => x.PlayerId == player.Id && x.Gender == dbCharacter.Gender && x.ComponentId == slot && x.IsProp == prop).ToList();

                if (slot == 3 && !prop) wardrobeItems.AddRange(Body);

                var clothes = wardrobeItems.ConvertAll(x => new { Id = x.Id.ToString(), Name = x.Name, c = x.ComponentId, d = x.DrawableId, t = x.TextureId});

                await wardrobeWindow.TriggerEvent(player, "responseWardrobeClothes", JsonConvert.SerializeObject(clothes));
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task wardrobeDress(RXPlayer player, string slotStr, int componentId, int drawableId, int textureId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;

            RXWindow wardrobeWindow = new RXWindow("Wardrobe");

            bool prop = slotStr.Contains("p-");

            if (prop) slotStr.Replace("p-", "");

            if (int.TryParse(slotStr, out int slot))
            {
                using var db = new RXContext();

                var dbCharacter = await db.Characters.FirstOrDefaultAsync(x => x.Id == player.Id);
                if (dbCharacter == null) return;

                DbWardrobeItem item = null;

                if (!(slot == 3 && !prop))
                {
                    var wardrobeItems = await db.WardrobeItems.ToListAsync();

                    if (player.TeamId != 0 && player.Team != null && player.Team.TeamClothes.Count > 0)
                    {
                        await player.Team.TeamClothes.forEach(x => wardrobeItems.Add(new DbWardrobeItem
                        {
                            Id = wardrobeItems.Max(x => x.Id) + 1,
                            ComponentId = x.ComponentId,
                            DrawableId = x.DrawableId,
                            TextureId = x.TextureId,
                            Gender = x.Gender,
                            IsProp = x.IsProp,
                            Name = x.Name,
                            PlayerId = player.Id
                        }));
                    }

                    item = wardrobeItems.FirstOrDefault(x => x.PlayerId == player.Id && x.Gender == dbCharacter.Gender && x.ComponentId == slot && x.IsProp == prop && x.ComponentId == componentId && x.DrawableId == drawableId && x.TextureId == textureId);
                }
                else
                {
                    item = Body.FirstOrDefault(x => x.ComponentId == componentId && x.DrawableId == drawableId && x.TextureId == textureId);
                }

                if (item == null) return;

                if (prop)
                    await player.SetAccessoriesAsync(slot, item.DrawableId, item.TextureId);
                else
                    await player.SetClothesAsync(slot, item.DrawableId, item.TextureId);

                Dictionary<int, RXClothesProp> clothesParts = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Clothes).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                Dictionary<int, RXClothesProp> clothesProps = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Accessories).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);


                if (prop)
                {
                    clothesProps[slot] = new RXClothesProp
                    {
                        drawable = item.DrawableId,
                        texture = item.TextureId
                    };
                }
                else
                {
                    clothesParts[slot] = new RXClothesProp
                    {
                        drawable = item.DrawableId,
                        texture = item.TextureId
                    };
                }

                dbCharacter.Clothes = JsonConvert.SerializeObject(clothesParts);
                dbCharacter.Accessories = JsonConvert.SerializeObject(clothesProps);

                await db.SaveChangesAsync();
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task wardrobeAltkleider(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;

            player.CloseNativeMenu();

            using var db = new RXContext();

            List<DbWardrobeItem> wardrobeItems = await db.WardrobeItems.Where(x => x.PlayerId == player.Id && !x.Name.ToLower().Contains("nichts") && !x.Name.ToLower().Contains("kein")).ToListAsync();

            List<NativeItem> nativeItems = new List<NativeItem>
            {
                new NativeItem("Schließen", player => player.CloseNativeMenu())
            };

            await wardrobeItems.forEach(x =>
            {
                nativeItems.Add(new NativeItem(x.Name, player =>
                {
                    var nativeMenu = new NativeMenu("Altkleider", x.Name, new List<NativeItem>
                    {
                        new NativeItem("Schließen", async player => await wardrobeAltkleider(player)),
                        new NativeItem("Entsorgen", async player =>
                        {
                            using var context = new RXContext();

                            context.WardrobeItems.Remove(x);

                            context.SaveChanges();

                            player.CloseNativeMenu();

                            await player.SendNotify("Du hast das Kleidungsstück in einem Altkleidersack entsorgt!");
                        })
                    });

                    player.CloseNativeMenu();
                    player.ShowNativeMenu(nativeMenu);
                }));
            });

            player.ShowNativeMenu(new NativeMenu("Altkleider", "", nativeItems));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task saveOutfit(RXPlayer player, string outfitName)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;

            using var db = new RXContext();

            var dbCharacter = await db.Characters.FirstOrDefaultAsync(x => x.Id == player.Id);
            if (dbCharacter == null) return;

            await db.WardrobeOutfits.AddAsync(new DbWardrobeOutfit
            {
                PlayerId = player.Id,
                Accessories = dbCharacter.Accessories,
                Clothes = dbCharacter.Clothes,
                Gender = dbCharacter.Gender,
                Name = outfitName,
            });

            await db.SaveChangesAsync();

            await wardrobeOutfits(player);

            await player.SendNotify("Du hast dein Outfit erfolgreich gespeichert!");
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task wardrobeOutfits(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;

            player.CloseNativeMenu();

            using var db = new RXContext();

            var dbCharacter = await db.Characters.FirstOrDefaultAsync(x => x.Id == player.Id);
            if (dbCharacter == null) return;

            List<DbWardrobeOutfit> wardrobeOutfitList = await db.WardrobeOutfits.Where(x => x.PlayerId == player.Id && x.Gender == dbCharacter.Gender).ToListAsync();

            List<NativeItem> nativeItems = new List<NativeItem>
            {
                new NativeItem("Schließen", player => player.CloseNativeMenu()),
                new NativeItem("Aktuelles Outfit speichern", async player =>
                {
                    player.CloseNativeMenu();

                    object textInputBoxObject = new
                    {
                        textBoxObject = new
                        {
                            Title = "Outfit benennen",
                            Message = "Gebe bitte den Namen des Outfits ein.",
                            Callback = "saveOutfit"
                        }
                    };

                    await new RXWindow("TextInputBox").OpenWindow(player, textInputBoxObject);
                })
            };

            await wardrobeOutfitList.forEach(x =>
            {
                nativeItems.Add(new NativeItem(x.Name, player =>
                {
                    var nativeMenu = new NativeMenu("Outfits", x.Name, new List<NativeItem>
                    {
                        new NativeItem("Schließen", async player => await wardrobeOutfits(player)),
                        new NativeItem("Outfit anziehen", async player =>
                        {
                            using var context = new RXContext();

                            var dbCharacter = context.Characters.FirstOrDefault(x => x.Id == player.Id);
                            if (dbCharacter == null) return;

                            dbCharacter.Clothes = x.Clothes;
                            dbCharacter.Accessories = x.Accessories;

                            context.SaveChanges();

                            player.CloseNativeMenu();

                            await player.LoadCharacter(dbCharacter);

                            await player.SendNotify("Du hast dich erfolgreich umgezogen!");
                        }),
                        new NativeItem("Outfit entfernen", async player =>
                        {
                            using var context = new RXContext();

                            context.WardrobeOutfits.Remove(x);

                            context.SaveChanges();

                            player.CloseNativeMenu();

                            await player.SendNotify("Dein Outfit wurde aus deinem Kleiderschrank entfernt!");
                        })
                    });

                    player.CloseNativeMenu();
                    player.ShowNativeMenu(nativeMenu);
                }));
            });

            player.ShowNativeMenu(new NativeMenu("Outfits", "", nativeItems));
        }
    }
}
