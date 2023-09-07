using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Inventory;
using Backend.Modules.Native;
using Backend.Modules.Phone.Apps;
using Backend.Modules.Shops;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
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

namespace Backend.Modules.Farming
{
    public enum PlantType
    {
        Orange = 0,
        Weed = 1
    }

    class SeedModule : RXModule
    {
        public SeedModule() : base("Seed") { }

        public static Vector3 SellStandPosition = new Vector3(1792.5328, 4594.4746, 37.682922);
        public static float SellStandHeading = -179.88539f;

        public static Vector3 OrangeStandPosition = new Vector3(-3026.7537, 369.26904, 14.642386);
        public static float OrangeStandHeading = -101.90585f;

        public static Vector3 OrangeProcessingPosition = new Vector3(2472.0574, 4110.675, 38.064697);
        public static float OrangeProcessingHeading = 71.00133f;

        public static Vector3 WeedSellingPosition = new Vector3(-1638.9268, -1064.4023, 13.152304);
        public static float WeedSellingHeading = 15.349948f;

        public static List<DbPlant> Plants = new List<DbPlant>();

        public static List<Vector3> Places = new List<Vector3>
        {
            new Vector3(2149.596, 5085.409, 45.289978),
            new Vector3(1930.9148, 4935.2637, 46.91632),
            new Vector3(2384.845, 4878.074, 41.14574),
            new Vector3(2579.5437, 4525.427, 36.147373)
        };

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            await NAPI.Task.RunAsync(() => new NPC((PedHash)NAPI.Util.GetHashKey("cs_old_man2"), SellStandPosition, SellStandHeading, 0u));
            await NAPI.Task.RunAsync(() => new NPC((PedHash)NAPI.Util.GetHashKey("s_m_m_migrant_01"), OrangeProcessingPosition, OrangeProcessingHeading, 0u));
            await NAPI.Task.RunAsync(() => new NPC((PedHash)NAPI.Util.GetHashKey("a_m_m_og_boss_01"), WeedSellingPosition, WeedSellingHeading, 0u));

            {
                var mcb = await NAPI.Entity.CreateMCB(SellStandPosition, new Color(255, 140, 0), 0u, 4.4f, 2.4f, false, MarkerType.VerticalCylinder, true, 285, 82, "Gärtner");

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um mit dem Gärtner zu sprechen.",
                    Color = "dgray",
                    Duration = 3500,
                    Title = "Gärtner"
                };

                mcb.ColShape.Action = async player => await ShopModule.OpenShop(player, 9991);
            }

            {
                var mcb = await NAPI.Entity.CreateMCB(OrangeProcessingPosition, new Color(255, 140, 0), 0u, 6.4f, 2.4f, false, MarkerType.VerticalCylinder, false, 285, 82, "Orangenverarbeiter");

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um Orangen zu verarbeiten.",
                    Color = "dgray",
                    Duration = 3500,
                    Title = "Orangenverarbeiter"
                };

                mcb.ColShape.Action = async player => await ProcessOranges(player);
            }

         

            {
                var mcb = await NAPI.Entity.CreateMCB(WeedSellingPosition, new Color(255, 140, 0), 0u, 6.4f, 2.4f, false, MarkerType.VerticalCylinder, false, 52, 81, "Weedverkäufer WIRD ENTFERNT");

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um mit dem Dealer zu sprechen.",
                    Color = "dgray",
                    Duration = 3500,
                    Title = "Dealer",
                    OnlyBadFaction = true
                };

                mcb.ColShape.Action = async player => await OpenWeedShop(player);
            }

            Places.ForEach(async place =>
            {
                var mcb = await NAPI.Entity.CreateMCB3(place, new Color(255, 140, 0), 0u, 150f, 100f, false, MarkerType.VerticalCylinder);

                mcb.ColShape.IsInteractionColShape = false;
                mcb.ColShape.PlantPlace = true;
            });
            
            db.Plants.RemoveRange(db.Plants.ToList().Where(plant => (DateTime.Now - plant.PlantTime).Days >= 3));

            db.SaveChanges();

            db.Plants.ToList().ForEach(async plant =>
            {
                if (plant.PlantType == PlantType.Orange)
                {
                    var obj = await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_plant_palm_01b"), plant.Position.ToPos(), new Vector3(), 255, 0));
                    var colshape = await NAPI.Task.RunReturnAsync(() => (RXColShape)NAPI.ColShape.CreateCylinderColShape(plant.Position.ToPos(), 2.4f, 4f, 0));

                    colshape.PlantObj = obj;
                    colshape.PlantData = plant;
                }
                else if (plant.PlantType == PlantType.Weed)
                {
                    int fortschritt = (int)Math.Round((DateTime.Now - plant.PlantTime).TotalHours / GetReadyHours(plant.PlantType) * 100);

                    if (!plant.Watered && fortschritt > 50) fortschritt = 50;

                    string model = "bkr_prop_weed_01_small_01c";

                    if (fortschritt < 25) model = "bkr_prop_weed_01_small_01c";
                    else if (fortschritt < 50) model = "bkr_prop_weed_01_small_01a";
                    else if (fortschritt < 75) model = "bkr_prop_weed_med_01a";
                    else if (fortschritt > 75) model = "bkr_prop_weed_lrg_01b";

                    var obj = await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey(model), plant.Position.ToPos(), new Vector3(), 255, 0));
                    var colshape = await NAPI.Task.RunReturnAsync(() => (RXColShape)NAPI.ColShape.CreateCylinderColShape(plant.Position.ToPos(), 2.4f, 4f, 0));

                    colshape.PlantObj = obj;
                    colshape.PlantData = plant;
                }
            });
        }

        public async Task ProcessOranges(RXPlayer player)
        {
            if (!player.CanInteract() || !await player.CanInteractAntiFloodNoMSG(2)) return;

            if (await NAPI.Task.RunReturnAsync(() => player.HasData("ProcessingOrange") && player.GetData<bool>("ProcessingOrange")))
            {
                player.IsTaskAllowed = true;

                await player.disableAllPlayerActions(false);
                await NAPI.Task.RunAsync(() => player.SetData("ProcessingOrange", false));
                await player.SendNotify("Du hast mit dem Verarbeiten aufgehört!");

                await player.StopAnimationAsync();
            }
            else
            {
                player.IsTaskAllowed = false;

                await player.disableAllPlayerActions(true);
                await NAPI.Task.RunAsync(() => player.SetData("ProcessingOrange", true));
                await player.SendNotify("Du hast mit dem Verarbeiten angefangen!");

                await player.StopAnimationAsync();
            }
        }

        public async Task OpenFruitShop(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            RXItemModel model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Orange");
            if (model == null || player.Container == null) return;

            RXItemModel model2 = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Orangensaft");
            if (model2 == null || player.Container == null) return;

            NativeMenu nativeMenu = new NativeMenu("Früchtemarkt", "", new List<NativeItem>
                    {
                        new NativeItem("Schließen", player => player.CloseNativeMenu()),
                        new NativeItem("3x Orangen verkaufen (5$)", async player =>
                        {
                            var amount = player.Container.GetItemAmount(model);
                            if (amount < 3)
                            {
                                await player.SendNotify("Du hast nicht genug Orangen bei dir!");
                                player.CloseNativeMenu();

                                return;
                            }

                            player.Container.RemoveItem(model, 3);

                            await player.GiveMoney(5);
                            await RX.TakeMoneyFromStaatskonto(5);

                            await player.SendNotify("Du hast 3x Orangen für 5$ verkauft!");
                        }),
                        new NativeItem("2x Orangensaft verkaufen (14$)", async player =>
                        {
                            var amount = player.Container.GetItemAmount(model2);
                            if (amount < 2)
                            {
                                await player.SendNotify("Du hast nicht genug Orangensaft bei dir!");
                                player.CloseNativeMenu();

                                return;
                            }

                            player.Container.RemoveItem(model2, 2);

                            await player.GiveMoney(14);
                            await RX.TakeMoneyFromStaatskonto(14);

                            await player.SendNotify("Du hast 2x Orangensaft für 14$ verkauft!");
                        }),
                    });

            if (player.Container.GetItemAmount(model) >= 3)
            {
                nativeMenu.Items.Add(new NativeItem("Alle Orangen verkaufen (" + (player.Container.GetItemAmount(model) / 3 * 5) + "$)", async player =>
                {
                    var price = player.Container.GetItemAmount(model) / 3 * 5;

                    var amount = player.Container.GetItemAmount(model);
                    if (amount < 3)
                    {
                        await player.SendNotify("Du hast nicht genug Orangen bei dir! Mind. 3");
                        player.CloseNativeMenu();

                        return;
                    }

                    player.CloseNativeMenu();

                    player.Container.RemoveItem(model, amount);

                    await player.GiveMoney(price);
                    await RX.TakeMoneyFromStaatskonto(price);

                    await player.SendNotify("Du hast " + amount + "x Orangen für " + price + "$ verkauft!");
                }));
            }

            if (player.Container.GetItemAmount(model2) >= 2)
            {
                nativeMenu.Items.Add(new NativeItem("Alle Orangensäfte verkaufen (" + (player.Container.GetItemAmount(model2) / 2 * 14) + "$)", async player =>
                {
                    var price = player.Container.GetItemAmount(model2) / 2 * 14;

                    var amount = player.Container.GetItemAmount(model2);
                    if (amount < 2)
                    {
                        await player.SendNotify("Du hast nicht genug Orangensaft bei dir! Mind. 2");
                        player.CloseNativeMenu();

                        return;
                    }

                    player.CloseNativeMenu();

                    player.Container.RemoveItem(model2, amount);

                    await player.GiveMoney(price);
                    await RX.TakeMoneyFromStaatskonto(price);

                    await player.SendNotify("Du hast " + amount + "x Orangensaft für " + price + "$ verkauft!");
                }));
            }

            player.ShowNativeMenu(nativeMenu);
        }

        public async Task OpenWeedShop(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            if (player.TeamId == 0 || player.Team == null)
            {
                await player.SendNotify("Hör zu kleiner.. Dafür bist du noch nicht gewachsen.");

                return;
            }

            RXItemModel model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Weedtüte");
            if (model == null || player.Container == null) return;

            RXItemModel model2 = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Weed");
            if (model2 == null || player.Container == null) return;

            NativeMenu nativeMenu = new NativeMenu("Dealer", "", new List<NativeItem>
                    {
                        new NativeItem("Schließen", player => player.CloseNativeMenu()),
                        new NativeItem("1x Weedtüte verkaufen (215$)", async player =>
                        {
                            var amount = player.Container.GetItemAmount(model);
                            if (amount < 1)
                            {
                                await player.SendNotify("Du hast nicht genug Weedtüten bei dir!");
                                player.CloseNativeMenu();

                                return;
                            }

                            player.Container.RemoveItem(model, 1);

                            await player.GiveMoney(215);

                            await player.SendNotify("Du hast 1x Weedtüte für 215$ verkauft!");
                        })
                    });

            if (player.Container.GetItemAmount(model) >= 1)
            {
                nativeMenu.Items.Add(new NativeItem("Alle Weedtüten verkaufen (" + (player.Container.GetItemAmount(model) * 215) + "$)", async player =>
                {
                    var price = player.Container.GetItemAmount(model) * 215;

                    var amount = player.Container.GetItemAmount(model);
                    if (amount < 1)
                    {
                        await player.SendNotify("Du hast nicht genug Weedtüten bei dir! Mind. 1");
                        player.CloseNativeMenu();

                        return;
                    }

                    player.Container.RemoveItem(model, amount);

                    player.CloseNativeMenu();

                    await player.GiveMoney(price);

                    await player.SendNotify("Du hast " + amount + "x Weedtüten für " + price + "$ verkauft!");
                }));
            }

            player.ShowNativeMenu(nativeMenu);
        }

        /*
        public override async Task OnFiveSecond()
        {
            var list = await NAPI.Task.RunReturnAsync(() => PlayerController.GetValidPlayers().ToList().Where(x => x.HasData("ProcessingOrange") && x.GetData<bool>("ProcessingOrange")));

            RXItemModel model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Orange");
            if (model == null) return;

            RXItemModel model2 = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Orangensaft");
            if (model2 == null) return;

            await list.forEachAlternativeAsync(async player =>
            {
                await player.PlayAnimationAsync(33, "anim@mp_snowball", "pickup_snowball");
                await player.disableAllPlayerActions(true);

                NAPI.Task.Run(async () =>
                {
                    if (player.HasData("ProcessingOrange") && player.GetData<bool>("ProcessingOrange"))
                    {

                        if (player.Container != null && player.Container.GetItemAmount(model) > 2)
                        {
                            player.Container.RemoveItem(model, 3);
                            player.Container.AddItem(model2, 1);

                            await player.StopAnimationAsync();
                        }
                        else
                        {
                            player.SetData("ProcessingOrange", false);
                            player.IsTaskAllowed = true;

                            await player.disableAllPlayerActions(false);
                            await player.SendNotify("Du hast das Verarbeiten erfolgreich abgeschlossen!");

                            await player.StopAnimationAsync();
                        }
                    }
                }, 4000);
            });

            List<RXColShape> colShapes = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllColShapes().Cast<RXColShape>().ToList());
            if (colShapes == null || colShapes.Count < 1) return;

            await colShapes.forEach(async plant =>
            {
                if (plant.PlantData != null && plant.PlantData.Id > 0 && plant.PlantObj != null && plant.PlantData.PlantType == PlantType.Weed)
                {
                    int fortschritt = (int)Math.Round((DateTime.Now - plant.PlantData.PlantTime).TotalHours / GetReadyHours(plant.PlantData.PlantType) * 100);

                    if (!plant.PlantData.Watered && fortschritt > 50) fortschritt = 50;

                    string model = "bkr_prop_weed_01_small_01c";

                    if (fortschritt < 25) model = "bkr_prop_weed_01_small_01c";
                    else if (fortschritt < 50) model = "bkr_prop_weed_01_small_01a";
                    else if (fortschritt < 75) model = "bkr_prop_weed_med_01a";
                    else if (fortschritt > 75) model = "bkr_prop_weed_lrg_01b";

                    if (await NAPI.Task.RunReturnAsync(() => plant.PlantObj.Model != NAPI.Util.GetHashKey(model)))
                    {
                        await NAPI.Task.RunAsync(() => plant.PlantObj.Delete());

                        var obj = await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey(model), plant.PlantData.Position.ToPos(), new Vector3(), 255, 0));

                        plant.PlantObj = obj;
                    }
                }
            });
        }
        */
        public int GetReadyHours(PlantType plantType)
        {
            switch (plantType)
            {
                case PlantType.Orange:
                    return 1;

                case PlantType.Weed:
                    return 6;
            }

            return 0;
        }

        //[HandleExceptions]
        public override async Task PressedE(RXPlayer player)
        {
            List<RXColShape> colShapes = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllColShapes().Cast<RXColShape>().ToList());
            if (colShapes == null || colShapes.Count < 1) return;

            RXColShape colShape = await NAPI.Task.RunReturnAsync(() => colShapes.FirstOrDefault(colShape => colShape.PlantData.Id != 0 && colShape.IsPointWithin(player.Position)));
            if (colShape == null) return;

            if (await NAPI.Task.RunReturnAsync(() => colShape.Dimension) != await player.GetDimensionAsync() || !player.IsTaskAllowed) return;

            int fortschritt = (int)Math.Round((DateTime.Now - colShape.PlantData.PlantTime).TotalHours / GetReadyHours(colShape.PlantData.PlantType) * 100);

            if (colShape.PlantData.PlantType == PlantType.Orange)
            {
                if (fortschritt < 100 || colShape.PlantData.OwnerId != player.Id) return;

                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(20000);

                player.IsTaskAllowed = false;

                await player.PlayAnimationAsync(33, "amb@world_human_gardener_plant@male@idle_a", "idle_a", 8);
                await Task.Delay(20000);

                lock (player) if (!RX.PlayerExists(player)) return;

                player.IsTaskAllowed = true;

                var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == colShape.PlantData.PlantType.ToString());
                if (model == null) return;

                int amount = new Random().Next(10, 16);

                if (player.IsHigh()) amount = new Random().Next(15, 30);

                if (player.Container.CanInventoryItemAdded(model, amount))
                {
                    player.Container.AddItem(model, amount);

                    await player.SendNotify("Du hast die Pflanze erfolgreich geerntet.");

                    await NAPI.Task.RunAsync(() =>
                    {
                        using var db = new RXContext();

                        db.Plants.Remove(colShape.PlantData);

                        db.SaveChanges();

                        if (colShape.PlantObj != null) colShape.PlantObj.Delete();

                        colShape.Delete();
                    });
                }
                else
                {
                    await player.SendNotify("Es befindet sich nicht genügend Platz in deinem Inventar!");
                }

                await player.StopAnimationAsync();
                await player.disableAllPlayerActions(false);
            }
            else if (colShape.PlantData.PlantType == PlantType.Weed)
            {
                if (colShape.PlantData.OwnerId != player.Id) return;

                if (fortschritt >= 50 && !colShape.PlantData.Watered)
                {
                    if (player.Container.GetItemAmount("Giesskanne") > 0)
                    {
                        await player.disableAllPlayerActions(true);
                        await player.SendProgressbar(5000);

                        player.IsTaskAllowed = false;

                        await player.PlayAnimationAsync(33, "amb@world_human_gardener_plant@male@idle_a", "idle_a", 8);
                        await Task.Delay(5000);

                        lock (player) if (!RX.PlayerExists(player)) return;

                        player.IsTaskAllowed = true;

                        await player.SendNotify("Du hast deine Pflanze erfolgreich gewässert.");

                        await player.StopAnimationAsync();
                        await player.disableAllPlayerActions(false);

                        var wateringCan = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Giesskanne");
                        if (wateringCan == null) return;

                        using var db = new RXContext();

                        var plant = await db.Plants.FirstOrDefaultAsync(x => x.Id == colShape.PlantData.Id);
                        if (plant == null) return;

                        colShape.PlantData.Watered = true;
                        colShape.PlantData.PlantTime = DateTime.Now.Subtract(new TimeSpan(GetReadyHours(colShape.PlantData.PlantType) / 2, 0, 0));

                        plant.Watered = true;
                        plant.PlantTime = DateTime.Now.Subtract(new TimeSpan(GetReadyHours(colShape.PlantData.PlantType) / 2, 0, 0));

                        await db.SaveChangesAsync();

                        player.Container.RemoveItem(wateringCan);
                    }
                    else return;
                }
                else if (fortschritt >= 100 && colShape.PlantData.Watered)
                {
                    await player.disableAllPlayerActions(true);
                    await player.SendProgressbar(20000);

                    player.IsTaskAllowed = false;

                    await player.PlayAnimationAsync(33, "amb@world_human_gardener_plant@male@idle_a", "idle_a", 8);
                    await Task.Delay(20000);

                    lock (player) if (!RX.PlayerExists(player)) return;

                    player.IsTaskAllowed = true;

                    var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == colShape.PlantData.PlantType.ToString());
                    if (model == null) return;

                    var modelWeed = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Weedsamen");
                    if (modelWeed == null) return;

                    int amount = new Random().Next(6, 14);
                    int seeds = new Random().Next(1, 50);

                    if (player.Container.CanInventoryItemAdded(model, amount))
                    {
                        player.Container.AddItem(model, amount);

                        await player.SendNotify("Du hast die Pflanze erfolgreich geerntet.");

                        if (player.Container.CanInventoryItemAdded(modelWeed, 1) && seeds == 20) player.Container.AddItem(modelWeed);

                        await NAPI.Task.RunAsync(() =>
                        {
                            using var db = new RXContext();

                            db.Plants.Remove(colShape.PlantData);

                            db.SaveChanges();

                            if (colShape.PlantObj != null) colShape.PlantObj.Delete();

                            colShape.Delete();
                        });
                    }
                    else
                    {
                        await player.SendNotify("Es befindet sich nicht genügend Platz in deinem Inventar!");
                    }

                    await player.StopAnimationAsync();
                    await player.disableAllPlayerActions(false);
                }
            }
        }

        //[HandleExceptions]
        public override async Task OnColShape(RXColShape shape, RXPlayer player, bool state)
        {
            if (state && shape.PlantData != null && shape.PlantData.Id > 0 && shape.PlantData.OwnerId == player.Id)
            {
                if (shape.PlantData.PlantType == PlantType.Orange)
                {
                    int fortschritt = (int)Math.Round((DateTime.Now - shape.PlantData.PlantTime).TotalHours / GetReadyHours(shape.PlantData.PlantType) * 100);

                    var msg = "";

                    msg += "Pflanzentyp: " + shape.PlantData.PlantType.ToString();
                    msg += "Gepflanzt: " + MessengerApp.GetUpdatedTimeFormated(shape.PlantData.PlantTime, true);
                    msg += "Fortschritt: " + (fortschritt > 100 ? 100 : fortschritt) + "%";

                    if (shape.PlantData.OwnerId == player.Id && fortschritt >= 100)
                        msg = "Diese Pflanze ist nun ausgewachsen. Du kannst diese jetzt ernten.";

                    await player.SendNotify(msg, 6000, "green", "Pflanze");
                }
                else if (shape.PlantData.PlantType == PlantType.Weed)
                {
                    int fortschritt = (int)Math.Round((DateTime.Now - shape.PlantData.PlantTime).TotalHours / GetReadyHours(shape.PlantData.PlantType) * 100);

                    var msg = "";

                    msg += "Pflanzentyp: " + shape.PlantData.PlantType.ToString();
                    msg += "" + (shape.PlantData.Watered ? "Gewässert" : "Gepflanzt") + ": " + MessengerApp.GetUpdatedTimeFormated(shape.PlantData.PlantTime, true);
                    msg += "Fortschritt: " + (fortschritt > 100 ? 100 : fortschritt) + "%";

                    if (shape.PlantData.OwnerId == player.Id && fortschritt >= 100 && shape.PlantData.Watered)
                        msg = "Diese Pflanze ist nun ausgewachsen. Du kannst diese jetzt ernten.";

                    if (fortschritt >= 50 && !shape.PlantData.Watered)
                        msg = "Diese Pflanze muss gewässert werden.";

                    await player.SendNotify(msg, 6000, "green", "Pflanze");
                }
            }
        }

        public static async Task CreatePlant(RXPlayer player, PlantType type, bool inDatabase = true)
        {
            if (type == PlantType.Orange)
            {
                var obj = await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("prop_plant_palm_01b"), player.Position.Subtract(new Vector3(0, 0, 1)), new Vector3(), 255, player.Dimension));

                if (inDatabase)
                {
                    using var db = new RXContext();

                    var plant = new DbPlant
                    {
                        Id = await db.Plants.CountAsync() == 0 ? 1 : (await db.Plants.MaxAsync(con => con.Id) + 1),
                        OwnerId = player.Id,
                        PlantTime = DateTime.Now,
                        Type = (int)type,
                        Position = (await player.GetPositionAsync()).Subtract(new Vector3(0, 0, 1)).FromPos()
                    };

                    await db.Plants.AddAsync(plant);

                    await db.SaveChangesAsync();

                    var colshape = await NAPI.Task.RunReturnAsync(() => (RXColShape)NAPI.ColShape.CreateCylinderColShape(plant.Position.ToPos(), 2.4f, 4f, 0));

                    colshape.PlantObj = obj;
                    colshape.PlantData = plant;
                }
            }
            else if (type == PlantType.Weed)
            {
                var obj = await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("bkr_prop_weed_01_small_01c"), player.Position.Subtract(new Vector3(0, 0, 1)), new Vector3(), 255, player.Dimension));

                if (inDatabase)
                {
                    using var db = new RXContext();

                    var plant = new DbPlant
                    {
                        Id = await db.Plants.CountAsync() == 0 ? 1 : (await db.Plants.MaxAsync(con => con.Id) + 1),
                        OwnerId = player.Id,
                        PlantTime = DateTime.Now,
                        Type = (int)type,
                        Position = (await player.GetPositionAsync()).Subtract(new Vector3(0, 0, 1)).FromPos()
                    };

                    await db.Plants.AddAsync(plant);

                    await db.SaveChangesAsync();

                    var colshape = await NAPI.Task.RunReturnAsync(() => (RXColShape)NAPI.ColShape.CreateCylinderColShape(plant.Position.ToPos(), 2.4f, 4f, 0));

                    colshape.PlantObj = obj;
                    colshape.PlantData = plant;
                }
            }
        }
    }
}
