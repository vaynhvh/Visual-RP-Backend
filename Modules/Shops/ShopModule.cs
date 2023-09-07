using Backend.Models;
using Backend.Modules.Bank;
using Backend.Modules.BlackMarket;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
using Backend.Modules.Scenarios;
using Backend.MySql;
using Backend.MySql.Models;
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

namespace Backend.Modules.Shops
{
    public class ShopItemResponse
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public int Amount { get; set; }

        [JsonProperty(PropertyName = "p")]
        public int Price { get; set; }


        [JsonProperty(PropertyName = "image")]
        public string Ingame { get; set; }


        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }
        public ShopItemResponse() { }
    }

    public class RXShop
    {

        [JsonProperty("i")]
        public uint Id { get; set; }
        [JsonIgnore]
        public Vector3 Position { get; set; }
        [JsonIgnore]
        public bool Weapon { get; set; } = false;
        [JsonProperty("data")]
        public List<RXShopProduct> Products { get; set; } = new List<RXShopProduct>();

        [JsonProperty("n")]
        public string CustomName { get; set; } = "";

        [JsonProperty("amount")]
        public int Amount { get; set; } = 0;

        [JsonIgnore]
        public bool HasBlip { get; set; } = true;

        [JsonIgnore]
        public int CustomBlip { get; set; } = 0;
        [JsonIgnore]
        public int CustomColor { get; set; } = 0;

        public RXShop() { }

        public RXShop(uint id, Vector3 position, bool weapon = false)
        {
            Id = id;
            Position = position;
            Weapon = weapon;
        }

        public RXShop(Vector3 position, bool weapon = false)
        {
            Id = ShopModule.Shops.Count > 0 ? ShopModule.Shops.Max(x => x.Id) + 1 : 1;
            Position = position;
            Weapon = weapon;
        }

        public RXShop(Vector3 position, string customname = "", int customblip = 0, int customcolor = 0, bool weapon = false)
        {
            Id = ShopModule.Shops.Count > 0 ? ShopModule.Shops.Max(x => x.Id) + 1 : 1;
            Position = position;
            Weapon = weapon;
            CustomName = customname;
            CustomBlip = customblip;
            CustomColor = customcolor;
        }

        public RXShop(uint id, Vector3 position, string customname = "", int customblip = 0, int customcolor = 0, bool weapon = false, bool hasblip = true)
        {
            Id = id;
            Position = position;
            Weapon = weapon;
            CustomName = customname;
            CustomBlip = customblip;
            CustomColor = customcolor;
            HasBlip = hasblip;
        }
    }

    public class RXProduct
    {
        [JsonProperty("i")]
        public uint Id { get; set; }
        [JsonIgnore]
        public List<uint> ShopIds { get; set; } = new List<uint>();
        [JsonProperty("p")]
        public int Price { get; set; } = 0;
        [JsonIgnore]
        public RXItemModel ItemModel { get; set; }
        [JsonProperty("n")]
        public string Name { get; set; }



        public RXProduct(uint id, int price, RXItemModel model, List<uint> shops, string name)
        {
            Id = id;
            Price = price;
            ItemModel = model;
            ShopIds = shops;
            Name = name;
        }

        public RXProduct(uint id, int price, RXItemModel model)
        {
            Id = id;
            Price = price;
            ItemModel = model;
        }
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class ShopModule : RXModule
    {
        public ShopModule() : base("Shops", new RXWindow("Shop")) { }

        public static List<RXShop> Shops = new List<RXShop>();

        public static Dictionary<int, Rob> Robberies = new Dictionary<int, Rob>();


        //[HandleExceptions]
        public RXItemModel GetItemModel(uint id) => ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == id);

        //[HandleExceptions]
        public RXItemModel GetItemModel(string name) => ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == name);

        //[HandleExceptions]
        public override async void LoadAsync()
        {
            using var db = new RXContext();

            RequireModule("ItemModel");

            Shops = new List<RXShop>
            {
                //Weaponshops
                new RXShop(1, new Vector3(21.006, -1106.372, 29.797), true),
                new RXShop(2, new Vector3(-663.686, -938.757, 21.8292), true),
                new RXShop(3, new Vector3(811.254, -2157.673, 29.619), true),
                new RXShop(4, new Vector3(843.329, -1033.942, 28.195), true),
                new RXShop(5, new Vector3(-1305.258, -393.208, 36.696), true),
                new RXShop(6, new Vector3(252.862, -49.209, 69.941), true),
                new RXShop(7, new Vector3(2568.893, 294.226, 108.735), true),
                new RXShop(8, new Vector3(1692.728, 3759.273, 34.705), true),
                new RXShop(9, new Vector3(-331.176, 6083.048, 31.455), true),
                new RXShop(10, new Vector3(-3172.487, 1086.728, 20.839), true),
                new RXShop(11, new Vector3(-1118.809, 2697.982, 18.55415), true),

                //Shops
                new RXShop(12, new Vector3(25.7567, -1346.8448, 28.3970), false), // 12
                new RXShop(13, new Vector3(374.68, 327.81, 102.7), false), // 13
                new RXShop(14, new Vector3(-708, -912.85, 18.2), false), // 14
                new RXShop(15, new Vector3(-550.91, -192.17, 37.22), "Regierungs Shop", 280, 33), // 15
                new RXShop(16, new Vector3(-1222.49, -906.26, 11.3), false), // 16
                new RXShop(17, new Vector3(547.21, 2671.64, 41), false), // 17
                new RXShop(18, new Vector3(1960.56, 3742.51, 31.1), false), // 18
                new RXShop(19, new Vector3(1730, 6416, 34), false), // 19
                new RXShop(20, new Vector3(2555.57, 382.7, 107.7), false), // 20
                new RXShop(21, new Vector3(2677.46, 3281.81, 54.24), false), // 21
                new RXShop(22, new Vector3(-47.73, -1756.17, 28.5), false), // 22
                new RXShop(23, new Vector3(-3241.99, 1001.7, 11.83), false), // 23
                new RXShop(24, new Vector3(-3039, 586, 6.91), false), // 24
                new RXShop(25, new Vector3(-1821.1149, 791.9859, 138.12766), false), // 25

                new RXShop(27, new Vector3(2748.785, 3472.3552, 55.67765), "Baumarkt", 566, 33), // 2
                new RXShop(28, new Vector3(185.55228f, -931.6463f, 30.686806f), "Rubbellose", 7000, 0, false, false), // 28
                new RXShop(29, new Vector3(185.26794f, -942.69183f, 30.09193f), "Donutstand", 7000, 0, false, false), // 28
                new RXShop(30, new Vector3(188.21121f, -986.1788f, 30.09191f), "MP Hotdog", 7000, 0, false, false), // 28
                new RXShop(31, new Vector3(198.12215f, -977.2212f, 30.091911f), "MP Burger", 7000, 0, false, false), // 28

            };

            List<RXProduct> products = new List<RXProduct>
            {
               new RXProduct(2, 700, GetItemModel("Verbandskasten"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "Verbandskasten"),
               new RXProduct(3, 750, GetItemModel("Werkzeugkasten"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "Werkzeugkasten"),
               new RXProduct(4, 150, GetItemModel("Smartphone"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "Smartphone"),
               new RXProduct(55, 800, GetItemModel("Funkgerät"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "Funkgerät"),
               new RXProduct(78, 20000, GetItemModel("NVIDIA GTX 1070"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "NVIDIA GTX 1070"),
               new RXProduct(79, 20000, GetItemModel("NVIDIA RTX 2080"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "NVIDIA RTX 2080"),
               new RXProduct(80, 20000, GetItemModel("NVIDIA RTX 2080 TI"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "NVIDIA RTX 2080 TI"),
               new RXProduct(81, 20000, GetItemModel("AMD Ryzen 5 1600X"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "AMD Ryzen 5 1600X"),
               new RXProduct(82, 20000, GetItemModel("AMD Ryzen 7 2700X"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "AMD Ryzen 7 2700X"),
               new RXProduct(83, 20000, GetItemModel("16GB G.Skill Aegis DDR4-2666"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "16GB G.Skill Aegis DDR4-2666"),
               new RXProduct(84, 20000, GetItemModel("Netzteil"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "Netzteil"),
               new RXProduct(24, 2500, GetItemModel("Rubbellos"), new List<uint> { 28 }, "Rubbellos"),
               new RXProduct(90, 10000, GetItemModel("Luxus Rubbellos"), new List<uint> { 28 }, "Luxus Rubbellos"),
               new RXProduct(187, 150, GetItemModel("Donut"), new List<uint> { 29 }, "Donut"),
               new RXProduct(188, 100, GetItemModel("Milkshake"), new List<uint> { 29, 30, 31 }, "Milkshake"),
               new RXProduct(189, 200, GetItemModel("Hotdog"), new List<uint> { 30 }, "Hotdog"),
               new RXProduct(190, 100, GetItemModel("Eistee Zitrone"), new List<uint> { 30,31 }, "Eistee Zitrone"),
               new RXProduct(191, 100, GetItemModel("Eistee Pfirsich"), new List<uint> { 30, 31 }, "Eistee Pfirsich"),
               new RXProduct(192, 200, GetItemModel("Cheeseburger"), new List<uint> { 31 }, "Cheeseburger"),
               new RXProduct(193, 200, GetItemModel("Double Cheeseburger"), new List<uint> { 31 }, "Double Cheeseburger"),
               new RXProduct(194, 100, GetItemModel("Eistee Zitrone"), new List<uint> { 30,31 }, "Eistee Zitrone"),
               new RXProduct(195, 100, GetItemModel("Coca-Cola"), new List<uint> { 30,31 }, "Coca-Cola"),
               new RXProduct(196, 100, GetItemModel("Fanta"), new List<uint> { 30,31 }, "Fanta"),
               new RXProduct(32, 5500, GetItemModel("Zigaretten"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, "Zigaretten"),
            /*    new RXProduct(32, 5500, GetItemModel("Zigaretten"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }),
                //new RXProduct(5, 30, GetItemModel("Seil"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }),
                new RXProduct(6, 3000, GetItemModel("Laptop"), new List<uint> { 26 }),
                new RXProduct(7, 1200, GetItemModel("Paper"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }),
                new RXProduct(8, 125, GetItemModel("Filter"), new List<uint> { 12, 13, 14, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }),*/
                new RXProduct(9, 250, GetItemModel("Seil"), new List<uint> { 27 }, "Seil"),
                new RXProduct(10, 2500, GetItemModel("Kocher"), new List<uint> { 27 }, "Kocher"),
                new RXProduct(11, 5000, GetItemModel("Fangkaefig"), new List<uint> { 27 }, "Fangkaefig"),
                new RXProduct(12, 1000, GetItemModel("Schaufel"), new List<uint> { 27 }, "Schaufel"),
                new RXProduct(13, 2000, GetItemModel("Sichel"), new List<uint> { 27 }, "Sichel"),
                new RXProduct(14, 1250, GetItemModel("Schere"), new List<uint> { 27 }, "Schere"),
                new RXProduct(15, 1250, GetItemModel("Spitzhacke"), new List<uint> { 27 }, "Spitzhacke"),
                new RXProduct(76, 2500, GetItemModel("Käfig"), new List<uint> { 27 }, "Käfig"),
                new RXProduct(78, 3500, GetItemModel("Eimer"), new List<uint> { 27 }, "Eimer"),

            };

            List<RXProduct> weaponProducts = new List<RXProduct>
            {
                new RXProduct(1, 500, GetItemModel("Pistol"))
            };

            await Shops.forEach(async shop =>
            {
                var mcb = await NAPI.Entity.CreateMCB(shop.Position, new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.VerticalCylinder, shop.HasBlip, shop.Weapon ? 110 : shop.CustomName == "" ? 52 : shop.CustomBlip, (byte)(shop.Weapon ? 0 : shop.CustomName == "" ? 33 : shop.CustomColor), shop.Weapon ? "Ammunation" : shop.CustomName == "" ? "Shop" : shop.CustomName);

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um " + (shop.Weapon ? "Waffen zu kaufen" : "Gegenstände zu kaufen") + "!",
                    Color = "green",
                    Duration = 3500,
                    Title = shop.Weapon ? "Ammunation" : shop.CustomName == "" ? "Shop" : shop.CustomName
                };

                mcb.ColShape.Action = async player => await OpenShop(player, shop.Id);

                if (shop.Weapon)
                {
                    shop.Products = weaponProducts.ConvertAll(x => new RXShopProduct
                    {
                        Id = x.Id,
                        Image = x.ItemModel.ImagePath,
                        Name = x.ItemModel.Name,
                        Price = x.Price
                    });
                }
                else
                {
                    shop.Products = products.Where(x => x.ShopIds.Contains(shop.Id)).ToList().ConvertAll(x => new RXShopProduct
                    {
                        Id = x.Id,
                        Image = x.ItemModel.ImagePath,
                        Name = x.ItemModel.Name,
                        Price = x.Price
                    });
                }
            });

            Shops.Add(new RXShop // Gärtner
            {
                Id = 9991,
                Products = new List<RXShopProduct> { new RXShopProduct { Id = 999, Image = "samengut.png", Name = "Orangensamen", Price = 10 }, new RXShopProduct { Id = 998, Image = "Blumentopf.png", Name = "Blumentopf", Price = 20 }, new RXShopProduct { Id = 997, Image = "Duenger.png", Name = "Dünger", Price = 12 }, new RXShopProduct { Id = 996, Image = "Giesskanne.png", Name = "Giesskanne", Price = 12 } }
            });

            Shops.Add(new RXShop // Schwarzmarkt
            {
                Id = 9992,
                Products = new List<RXShopProduct> { new RXShopProduct { Id = 999, Image = "grinded-weed.png", Name = "Weedsamen", Price = 25 } }
            });
        }

        //[HandleExceptions]
        public static async Task OpenShop(RXPlayer player, uint shopId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            var shop = Shops.FirstOrDefault(x => x.Id == shopId);
            if (shop == null) return;

            var clientShop = new Models.RXShop
            {
                Id = shopId,
                Title = shop.Weapon ? "Ammunation" : "Shop",
                Items = shop.Products,
                Money = (uint)player.Cash,
                isRob = true,
            };

            await new RXWindow("Shop").OpenWindow(player, clientShop);

            await NAPI.Task.RunAsync(() => player.SetData("shopId", shopId));
        }

        public static async Task<RXShop> GetNearShop(RXPlayer player, float range = 2.5f)
        {
            if (player == null) return null;
            var ppos = await player.GetPositionAsync();
            foreach (var shop in Shops)
            {
                if (shop == null) continue;
                if (shop.Position.DistanceTo(ppos) < range)
                {
                    return shop;
                }
                
            }

            return null;
        }

        [RemoteEvent]
        public async Task robShop(RXPlayer player, uint shopid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;
            var shop = Shops.FirstOrDefault(x => x.Id == shopid);
            if (shop == null) return;

            if (RobberyModule.Robberies.ContainsKey((int)shopid))
            {
                await player.SendNotify("Dieser Shop wird bereits ausgeraubt.");
                return;
            }

            RobberyModule.Add((int)shop.Id, player, 2, RobType.Shop, new Random().Next(2, 10), new Random().Next(25, 35));

            await player.SendNotify("Du beginnst die Kasse aufzuschweißen.");
            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");

            player.Freezed = true;
            await player.SendProgressbar(60000);

            await Task.Delay(60000);
            if (player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;
            await player.StopAnimationAsync();
            player.Freezed = false;
            Random rnd = new Random();
            int fortnite = rnd.Next(8000, 10000);
            await player.GiveMoney(fortnite);
            await player.SendNotify($"Du hast erfolgreich den Laden ausgeraubt. Du hast {fortnite}$ erbeutet!");
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task shopBuy(RXPlayer player, string json)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || await NAPI.Task.RunReturnAsync(() => !player.HasData("shopId")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("shopId") == 0)) return;

            if (!json.IsValidJson<List<ShopItemResponse>>()) return;

            List<ShopItemResponse> items = JsonConvert.DeserializeObject<List<ShopItemResponse>>(json);
            if (items == null || items.Count < 1) return;

            uint shopId = player.GetData<uint>("shopId");

            var shop = Shops.FirstOrDefault(x => x.Id == shopId);
            if (shop == null) return;

            int totalAmount = 0;

            Dictionary<RXItemModel, int> itemsToAdd = new Dictionary<RXItemModel, int>();

            int weight = 0;
            int requiredSlots = 0;

            await items.forEach(async shopItemResponse =>
            {
                if (shopItemResponse.Amount < 1) return;

                var originalItem = shop.Products.FirstOrDefault(shopItem => shopItem.Id == shopItemResponse.Id);
                if (originalItem == null) return;

                var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == originalItem.Name);
                if (model == null) return;

                if (BlackMarketModule.MarketStorage.ContainsKey(originalItem.Name))
                {
                    if (BlackMarketModule.MarketStorage[originalItem.Name] < shopItemResponse.Amount)
                    {
                        await player.SendNotify("Der Shop hat das Produkt " + originalItem.Name + " nicht mehr in der Menge verfügbar. (Verfügbare Menge: " + BlackMarketModule.MarketStorage[originalItem.Name] + ")", 8000, "red", shop.Weapon ? "Ammunation" : "Shop");
                        return;
                    }
                    else
                    {
                        BlackMarketModule.MarketStorage[originalItem.Name] -= shopItemResponse.Amount;
                    }
                }

                itemsToAdd.Add(model, shopItemResponse.Amount);
                totalAmount = totalAmount + (originalItem.Price * shopItemResponse.Amount);

                weight += shopItemResponse.Amount * model.Weight;

                var similarStack = player.Container.GetSlotOfSimilairSingleItemsToStack(model.Id);
                var stackRequiredSlots = 99;

                if (similarStack == -1)
                { //Es gibt bisher keinen Stack mit diesem Itemtyp
                    stackRequiredSlots = shopItemResponse.Amount / model.MaximumStackSize < 1 ? 1 : (int)Math.Ceiling((decimal)shopItemResponse.Amount / (decimal)model.MaximumStackSize);
                }
                else
                { //Es wurde ein Stack mit dem Itemtyp gefunden
                    var spaceLeftOnSlot = model.MaximumStackSize - player.Container.GetAmountOfItemsOnSlot(similarStack);
                    stackRequiredSlots = shopItemResponse.Amount <= spaceLeftOnSlot ? 0 : (int)Math.Ceiling((decimal)(shopItemResponse.Amount - spaceLeftOnSlot) / (decimal)model.MaximumStackSize);
                }

                requiredSlots += model.MaximumStackSize > 1 ? stackRequiredSlots : 1;
            });

            if (itemsToAdd.Count < 1) return;

            if (player.Container.GetInventoryUsedSpace() + weight > player.Container.MaxWeight)
            {
                await player.SendNotify("Du hast keinen Platz im Inventar!", 3500, "red", shop.Weapon ? "Ammunation" : "Shop");
                return;
            }

            if (player.Container.GetUsedSlots() + requiredSlots > player.Container.MaxSlots)
            {
                await player.SendNotify("Dein Inventar hat zu wenige Slots!", 3500, "red", shop.Weapon ? "Ammunation" : "Shop");
                return;
            }

            PaymentModule.CreatePayment(player, totalAmount, async player =>
            {
                await RX.GiveMoneyToStaatskonto(totalAmount, "Shop - Einkauf - " + await player.GetNameAsync());

                await itemsToAdd.forEach(item => player.Container.AddItem(item.Key, item.Value));
                await player.SendNotify("Du hast einen Einkauf im Wert von " + totalAmount.FormatMoneyNumber() + " getätigt.", 3500, "green", shop.Weapon ? "Ammunation" : "Shop");

            }, "Shop - Einkauf", shopId == 9992 ? true : false);
        }
    }
}
