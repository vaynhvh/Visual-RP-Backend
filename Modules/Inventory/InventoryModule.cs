using Backend.Controllers;
using Backend.Models;
using Backend.Models.Appearance;
using Backend.Modules.ClothingShops;
using Backend.Modules.Discord;
using Backend.Modules.Faction;
using Backend.Modules.Laptop.Apps;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Backend.Models.RXContainer;
using static System.Reflection.Metadata.BlobBuilder;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Inventory
{

    public class DroppedWeaponObject
    {
        public uint Id { get; set; }
        public uint ItemModelId { get; set; }
        public Vector3 Position { get; set; }
        public GTANetworkAPI.Object Object { get; set; }


    }
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class InventoryModule : RXModule
    {
        public InventoryModule() : base("Inventory", new RXWindow("Inventory")) { }

        public static Dictionary<int, DroppedWeaponObject> DroppedWeapons = new Dictionary<int, DroppedWeaponObject>();
        public static List<int> WeaponAmAufheben = new List<int>();

        //[HandleExceptions]
        [RemoteEvent]
        public static async Task requestInventory(RXPlayer player, bool giveitem = false)
        {
            if (!player.IsLoggedIn || player.inPaintball || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !await player.CanInteractAntiFloodNoMSG(0.5)) return;

            var window = new RXWindow("Inventory");

            uint externInventoryId = 0;

            List<RXClientContainer> containerList = new List<RXClientContainer>();

            var pcontainer = player.Container.ConvertForClient(player.Container.Id, "Rucksack", player.Cash, player.Blackmoney);

            containerList.Add(pcontainer);

            if (player.TeamId != 0)
            {
                if (player.Team.Type == TeamType.Mafia && player.TeamId != 0 && player.GangwarContainerId != 0 || player.Team.Type == TeamType.Gang && player.TeamId != 0 && player.GangwarContainerId != 0)
                {
                    Vector3 playerpos = await player.GetPositionAsync();

                    if (playerpos.DistanceTo(player.Team.GangwarEnter) < 2.5)
                    {
                        containerList.Add(player.GangwarContainer.ConvertForClient(player.GangwarContainer.Id, "Gangwar"));

                        externInventoryId = player.GangwarContainer.Id;
                    }
                }
            }

            var closestVehicle = await NAPI.Task.RunReturnAsync(() => VehicleController.GetClosestVehicle(player.Position, 3));
            if (closestVehicle != null && closestVehicle.TrunkOpen && !(closestVehicle.ModelData == null && closestVehicle.TeamId == 0) && !await NAPI.Task.RunReturnAsync(() => closestVehicle.Locked))
            {
                if (closestVehicle.ContainerId == 0)
                {
                    using var db = new RXContext();

                    if (closestVehicle.TeamId == 0)
                    {
                        var dbVehicle = await db.Vehicles.FirstOrDefaultAsync(x => x.Id == closestVehicle.Id);
                        if (dbVehicle != null)
                        {
                            var dbContainer = new DbContainer
                            {
                                Id = await db.Containers.CountAsync() == 1 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                                Name = "Kofferraum",
                                MaxSlots = closestVehicle.ModelData.InventorySize,
                                MaxWeight = closestVehicle.ModelData.InventoryWeight,
                            };

                            dbVehicle.ContainerId = dbContainer.Id;
                            closestVehicle.ContainerId = dbContainer.Id;

                            await db.Containers.AddAsync(dbContainer);
                            await db.SaveChangesAsync();
                            await ContainerModule.RefreshContainersAsync();

                        }
                    }
                    else
                    {
                        var dbVehicle = await db.TeamVehicles.FirstOrDefaultAsync(x => x.Id == closestVehicle.Id);
                        if (dbVehicle != null)
                        {
                            var dbContainer = new DbContainer
                            {
                                Id = await db.Containers.CountAsync() == 1 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                                Name = "Kofferraum",
                                MaxSlots = 15,
                                MaxWeight = 50000,
                                
                            };
                            dbVehicle.ContainerId = dbContainer.Id;
                            closestVehicle.ContainerId = dbContainer.Id;

                            await db.Containers.AddAsync(dbContainer);
                            await db.SaveChangesAsync();

                            await ContainerModule.RefreshContainersAsync();
                        }
                    }
                }

                var c = closestVehicle.Container.ConvertForClient(closestVehicle.Container.Id, "Kofferraum", 0, 0, 2);

                containerList.Add(c);
                externInventoryId = closestVehicle.Container.Id;
            }

            if (containerList.Count == 1)
            {
                
                List<RXColShape> colShapes = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllColShapes().Cast<RXColShape>().Where(colShape => colShape.IsPointWithin(player.Position)).ToList());

                foreach (RXColShape colShape in colShapes)
                {
              
                    if (colShape != null && colShape.IsContainerColShape && colShape.ContainerOpen && (colShape.ContainerRestrictedWorkstation == 0 || colShape.ContainerRestrictedWorkstation == player.WorkstationId && colShape.ContainerRestrictedPlayer == player.Id) && (colShape.ContainerRestrictedPlayer == 0 || colShape.ContainerRestrictedPlayer == player.Id) && (colShape.ContainerRestrictedTeam == 0 || colShape.ContainerRestrictedTeam == player.TeamId && player.TeamMemberData.Inventory) && colShape.Container != null)
                    {
                        var c = colShape.Container.ConvertForClient(colShape.Container.Id, colShape.ContainerCustomName == "" ? colShape.Container.Name : colShape.ContainerCustomName, 0, 0, colShape.ContainerType);

                        containerList.Add(c);

                        externInventoryId = colShape.Container.Id;
                    }
                }
            }
       
            if (giveitem) {
                await window.OpenWindow(player, new { s = false, i = containerList, w = false, p = true }, false);
            }
            else
            {
                await window.OpenWindow(player, new { s = false, i = containerList, w = false, }, false);
            }

        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task fillInventorySlots(RXPlayer player, int id)
        {
            if (!player.IsTaskAllowed || !await player.CanInteractAntiFloodNoMSG(2)) return;

            await this.Window.CloseWindow(player);

            player.IsTaskAllowed = false;

            List<int> touchedSlots = new List<int>();
            Dictionary<uint, int> items = new Dictionary<uint, int>();

            var container = player.Container;

            foreach (RXItem item in container.Slots.Where(i => i != null && i.ItemModelId != 0 && i.Model.MaximumStackSize > 1).ToList())
            {
                if (!items.ContainsKey(item.ItemModelId))
                {
                    items.Add(item.ItemModelId, item.Amount);
                }
                else
                {
                    items[item.ItemModelId] += item.Amount;
                }
                if (!touchedSlots.Contains(item.Slot)) touchedSlots.Add(item.Slot);

                container.Slots.Remove(item);
            }

            using var db = new RXContext();

            foreach (int l_Slot in touchedSlots)
            {
                DbItem item = await db.Items.FirstOrDefaultAsync(s => s.Slot == l_Slot && s.InventoryId == container.Id);
                if (item == null) return;

                db.Items.Remove(item);
            }

            await db.SaveChangesAsync();

            foreach (KeyValuePair<uint, int> item in items)
            {
                RXItemModel itemModel = ItemModelModule.ItemModels.FirstOrDefault(s => s.Id == item.Key);
                if (itemModel == null) return;

                container.AddItem(itemModel, item.Value);
            }

            await ContainerModule.RefreshContainersAsync();

            player.IsTaskAllowed = true;
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task moveItemInInventory(RXPlayer player, int sourceSlot, int destinationSlot, int amount, int id, int tid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !await player.CanInteractAntiFlood(1) || id != tid) return;

            RXContainerObj container = ContainerModule.Containers.FirstOrDefault(c => c.Id == id);
            if (container == null) return;

            RXItemModel model = container.GetModelOnSlot(sourceSlot);
            RXItem item = container.Slots.FirstOrDefault(s => s.Slot == sourceSlot);

            if (amount <= 0 || model == null) return;

            if (amount > item.Amount) return;

            await ContainerModule.MoveItemToAnotherContainer(container, container, sourceSlot, destinationSlot, item.Amount);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async void MoveItem(RXPlayer player, int sourceSlot, int destinationSlot, int id, int tid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled) return;
            if (!await MoveItemToInventory(player, sourceSlot, destinationSlot, id, tid))
            {
                await this.Window.CloseWindow(player);
            }
        }

        //[HandleExceptions]
        public async Task<bool> MoveItemToInventory(RXPlayer player, int sourceSlot, int destinationSlot, int id, int tid)
        {
            try
            {
                if (!player.IsLoggedIn || !await player.CanInteractAntiFlood(1) || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled) return false;


                RXContainerObj externContainer = ContainerModule.Containers.FirstOrDefault(x => x.Id == tid);
                if (externContainer == null) return false;
                RXContainerObj playerContainer = ContainerModule.Containers.FirstOrDefault(x => x.Id == id);
                if (playerContainer == null) return false;


                if (!externContainer.CanInventoryItemAdded(playerContainer.GetModelOnSlot(sourceSlot), playerContainer.GetItemOnSlot(sourceSlot).Amount))
                {
                    await player.SendNotify("Das Inventar reicht nicht aus!", 3500, "red");
                    return false;
                }
                DiscordModule.Logs.Add(new DiscordLog("Inventory", (await player.GetNameAsync()) + " " + " legt das Item " + playerContainer.GetModelOnSlot(sourceSlot).Name + " " + playerContainer.GetItemOnSlot(sourceSlot).Amount + "x in das Inventar " + playerContainer.Name + ". INV (" + playerContainer.Id + ") -> OTHER (" + externContainer.Id + ")", DiscordModule.InventoryWebhook));

                await ContainerModule.MoveItemToAnotherContainer(playerContainer, externContainer, sourceSlot, destinationSlot, playerContainer.GetItemOnSlot(sourceSlot).Amount);

                await player.PlayInventoryInteractAnimation();



            }
            catch (Exception e)
            {
                RXLogger.Print("MoveItemToInventory: " + e.Message);
            }


            return true;
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task DropItem(RXPlayer player, int slot, int amount)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if (slot < 0 || slot > 47) return;

            RXContainerObj container = player.Container;
            if (container == null) return;

            var item = container.GetItemOnSlot(slot).Model;

            if (item != null && item.Name.Contains("Beamten") && !string.IsNullOrEmpty(item.WeaponHash))
            {
                await player.SendNotify("Diese Funktion ist nicht für Beamten Waffen verfügbar!");
                return;
            }

            if (!string.IsNullOrEmpty(item.ItemModel))
            {
                Vector3 playerpos = await player.GetPositionAsync();
                await NAPI.Task.RunAsync(() =>
                {
                    var obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(item.ItemModel), playerpos.Subtract(new Vector3(0, 0, 0.97)), new Vector3(90, 0, 0));
                    uint Id = (uint)DroppedWeapons.Count + 1;
                    DroppedWeapons.Add((int)Id, new DroppedWeaponObject { Id = Id, ItemModelId = item.Id, Object = obj, Position = obj.Position });
                });
            }
            DiscordModule.Logs.Add(new DiscordLog("Inventory", (await player.GetNameAsync()) + " " + " droppt das Item " + item.Name + " von dem Slot " + slot + ".", DiscordModule.InventoryWebhook));

            container.RemoveFromSlot(slot, amount);

            await player.PlayInventoryInteractAnimation();
        }

        [RemoteEvent]
        public async Task DropItemOutOfWindow(RXPlayer player, int inventoryId, int slot, int amount)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if (slot < 0 || slot > 47) return;

            RXContainerObj container = ContainerModule.Containers.FirstOrDefault(x => x.Id == inventoryId);
            if (container == null) return;

            var item = container.GetItemOnSlot(slot).Model;

            if (item != null && item.Name.Contains("Beamten") && !string.IsNullOrEmpty(item.WeaponHash))
            {
                await player.SendNotify("Diese Funktion ist nicht für Beamten Waffen verfügbar!");
                return;
            }

            if (!string.IsNullOrEmpty(item.ItemModel))
            {
                Vector3 playerpos = await player.GetPositionAsync();
                await NAPI.Task.RunAsync(() =>
                {
                    var obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(item.ItemModel), playerpos.Subtract(new Vector3(0, 0, 0.97)), new Vector3(90, 0, 0));
                    uint Id = (uint)DroppedWeapons.Count + 1;
                    DroppedWeapons.Add((int)Id, new DroppedWeaponObject { Id = Id, ItemModelId = item.Id, Object = obj, Position = obj.Position });
                });
            }
            DiscordModule.Logs.Add(new DiscordLog("Inventory", (await player.GetNameAsync()) + " " + " droppt das Item " + item.Name + " von dem Slot " + slot + ".", DiscordModule.InventoryWebhook));

            container.RemoveFromSlot(slot, amount);

            await player.PlayInventoryInteractAnimation();
        }

        public async Task<DroppedWeaponObject> GetDroppedWeapon(RXPlayer player)
        {
            if (player == null) return null;

            Vector3 playerpos = await player.GetPositionAsync();
            foreach (var weapon in DroppedWeapons)
            {
                if (!WeaponAmAufheben.Contains((int)weapon.Value.Id))
                {
                    if (weapon.Value.Position.DistanceTo(playerpos) < 2.5)
                    {
                        return weapon.Value;
                    }
                }
            }
            return null;

        }

        public override async Task PressedE(RXPlayer player)
        {

            var item = await GetDroppedWeapon(player);

            if (item == null) return;

            if (!WeaponAmAufheben.Contains((int)item.Id))
            {

                WeaponAmAufheben.Add((int)item.Id);
                DiscordModule.Logs.Add(new DiscordLog("Inventory", (await player.GetNameAsync()) + " " + " hebt die Waffe " + ItemModelModule.ItemModels.Find(x => x.Id == item.ItemModelId).Name + " auf.", DiscordModule.InventoryWebhook));


                await player.PlayInventoryInteractAnimation();

                await NAPI.Task.RunAsync(() =>
                {
                    item.Object.Delete();

                    player.Container.AddItem(item.ItemModelId, 1);
                    WeaponAmAufheben.Remove((int)item.Id);
                    DroppedWeapons.Remove((int)item.Id);
                });
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task GiveItem(RXPlayer player, int slot, int amount)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if (await player.GetIsInVehicleAsync()) return;

            if (slot < 0 || slot > 47) return;

            RXPlayer destinationPlayer = await PlayerController.FindPlayerById(player.GetData<uint>("giveitem"));

            await NAPI.Task.RunAsync(() => player.ResetData("giveitem"));

            if (destinationPlayer == null)
            {
                await this.Window.CloseWindow(player);
                return;
            }

            if ((await destinationPlayer.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 3.0f)
            {
                await this.Window.CloseWindow(player);
                return;
            }

            RXItem item = player.Container.GetItemOnSlot(slot);

            if (item.Model != null && item.Model.Name.Contains("Beamten") && !string.IsNullOrEmpty(item.Model.WeaponHash))
            {
                await player.SendNotify("Diese Funktion ist nicht für Beamten Waffen verfügbar!");
                return;
            }

            RXItemModel model = item.Model;

            if (item.Amount < amount)
            {
                await player.SendNotify("Du hast so viel davon nicht!");
                return;
            }

            if (!destinationPlayer.Container.CanInventoryItemAdded(model, amount))
            {
                await player.SendNotify("Der Spieler hat keinen Platz im Rucksack!");

                await this.Window.CloseWindow(player);
                return;
            }

            player.Container.RemoveFromSlot(slot, amount);
            destinationPlayer.Container.AddItem(model, amount);

            await player.SendNotify($"Du hast {amount}x {model.Name} gegeben!");
            await destinationPlayer.SendNotify($"Du hast {amount}x {model.Name} bekommen!");
            DiscordModule.Logs.Add(new DiscordLog("Inventory", (await player.GetNameAsync()) + " " + " übergibt das Item " + model.Name + " " + amount + "x an " + await destinationPlayer.GetNameAsync()  + ".", DiscordModule.InventoryWebhook));

            await ContainerModule.RefreshContainersAsync();

            await player.PlayInventoryInteractAnimation();
        }

        [RemoteEvent]
        public async Task requestPlayerClothes(RXPlayer dbPlayer)
        {
           

                if (dbPlayer == null)
                    return;

                using var db = new RXContext();

                DbCharacter dbCharacter = await db.Characters.FirstOrDefaultAsync(c => c.Id == dbPlayer.Id);
                if (dbCharacter == null) return;

                Customization customization = JsonConvert.DeserializeObject<Customization>(dbCharacter.Customization);
                if (customization == null) return;

                List<InventoryPlayerClothesCategory> playerInvCats = new List<InventoryPlayerClothesCategory>();

                Dictionary<int, RXClothesProp> clothesParts = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Clothes);
                Dictionary<int, RXClothesProp> clothesProps = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Accessories);

                // Clothes
                foreach (var slot in clothesParts)
                {
                    InventoryPlayerClothesCategory slotInvCat = new InventoryPlayerClothesCategory()
                    {
                        Slot = slot.Key,
                        Name = ClothingShopModule.GetComponentName(slot.Key),
                        Items = new List<InventoryPlayerClothesItem>()
                    };

                    if (slot.Key == 3) // Körper
                    {
                        List<DbWardrobeItem> wardrobeItems = await db.WardrobeItems.Where(x => x.PlayerId == dbPlayer.Id).ToListAsync();

                        foreach (var cloth in wardrobeItems)
                        {
                            if (cloth == null || cloth.ComponentId != 3 || cloth.Gender != dbCharacter.Gender) continue;

                            if (slotInvCat.Items.Where(i => i.Id == cloth.Id).Count() > 0) continue;
                            slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = (uint)cloth.ComponentId, Name = cloth.Name });
                        }
                    }
                    else if (slot.Key == 1)
                    {
                    foreach (var kvp in clothesParts.ToList().Where(c => c.Key == slot.Key))
                    {
                        var cloth = ClothingShopModule.Clothes.Find(x => x.Id == kvp.Value.clothid && !x.Prop && x.ComponentId == 1 && x.Male == dbCharacter.Gender);
                        if (cloth == null) continue;

                        if (slotInvCat.Items.Where(i => i.Id == cloth.Id).Count() > 0) continue;
                        slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = (uint)cloth.ComponentId, Name = cloth.Name });
                    }
                } else { 

                        // Add Clothes from actual wearing
                        foreach (var kvp in clothesParts.ToList().Where(c => c.Key == slot.Key))
                        {
                            var cloth = ClothingShopModule.Clothes.Find(x => x.Id == kvp.Value.clothid && !x.Prop && x.Male == dbCharacter.Gender);
                            if (cloth == null) continue;

                            if (slotInvCat.Items.Where(i => i.Id == cloth.Id).Count() > 0) continue;
                            slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = (uint)cloth.ComponentId, Name = cloth.Name });
                        }

                    }
                    playerInvCats.Add(slotInvCat);
                }

            foreach (var slot in clothesProps)
            {
                InventoryPlayerClothesCategory slotInvCat = new InventoryPlayerClothesCategory()
                {
                    Slot = slot.Key,
                    Name = ClothingShopModule.GetAccessoryName(slot.Key),
                    Items = new List<InventoryPlayerClothesItem>()
                };


                // Add Props from actual wearing
                foreach (var kvp in clothesProps.ToList().Where(c => c.Key == slot.Key))
                {
                    var cloth = ClothingShopModule.Clothes.Find(x => x.Id == kvp.Value.clothid && x.Prop && x.Male == dbCharacter.Gender);
                    if (cloth == null) continue;

                    if (slotInvCat.Items.Where(i => i.Id == cloth.Id).Count() > 0) continue;
                    slotInvCat.Items.Add(new InventoryPlayerClothesItem() { Id = (uint)cloth.ComponentId, Name = cloth.Name, Prop = true });
                }

                playerInvCats.Add(slotInvCat);
            }


            await dbPlayer.TriggerEventAsync("componentServerEvent", "Inventory", "responseInventoryClothes", NAPI.Util.ToJson(playerInvCats));
      
        }

        //[HandleExceptions]
        [RemoteEvent]
        public static async Task useInventoryItem(RXPlayer player, int slot)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.Injured || player.DeathData.IsDead || player.IsInvDisabled) return;

            bool usedSuccessfully = false;

            if (slot < 0 || slot > 47) return;
            //if (!player.CanInteract()) return;

            RXItemModel model = player.Container.GetModelOnSlot(slot);
            RXItem item = player.Container.GetItemOnSlot(slot);

            if (model == null || item == null) return;

            DiscordModule.Logs.Add(new DiscordLog("Inventory", (await player.GetNameAsync()) + " " + " benutzt das Item " + model.Name + " auf dem Slot " + slot + ".", DiscordModule.InventoryWebhook));

            if (model.WeaponHash != null && model.WeaponHash.Length > 0 && model.WeaponHash.ToLower() != "weapon_unarmed")
            {
                if (model.RemoveOnUse) player.Container.RemoveItemSlotFirst(model, slot);

                if (model.Name.Contains("Beamten") && player.InDuty && player.Team.IsState())
                {
                    await player.AddWeaponToLoadout((WeaponHash)NAPI.Util.GetHashKey(model.WeaponHash), true, 0, true);
                }
                else
                {
                    if (!player.Team.IsState()) return;

                    await player.AddWeaponToLoadout((WeaponHash)NAPI.Util.GetHashKey(model.WeaponHash), true, 0);
                }

                return;
            }

            usedSuccessfully = await ItemScripts.RunScript(player, slot, model.Script);

            if (model.RemoveOnUse && usedSuccessfully) player.Container.RemoveItemSlotFirst(model, slot);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public static async Task InvPunkt(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled) return;

            RXItemModel model = player.Container.GetModelOnSlot(1);

            if (model == null) return;

            bool usedSuccessfully = false;

            var amount = player.Container.GetItemAmount(model);
            if (amount < 1) return;

            usedSuccessfully = await ItemScripts.RunScript(player, 1, model.Script);

            if (model.RemoveOnUse && usedSuccessfully) player.Container.RemoveItem(model);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public static async Task InvComma(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled) return;

            RXItemModel model = player.Container.GetModelOnSlot(0);

            if (model == null) return;

            bool usedSuccessfully = false;

            var amount = player.Container.GetItemAmount(model);
            if (amount < 1) return;

            usedSuccessfully = await ItemScripts.RunScript(player, 0, model.Script);

            if (model.RemoveOnUse && usedSuccessfully) player.Container.RemoveItem(model);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task packArmor(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !player.IsTaskAllowed) return;

            if (await player.GetArmorAsync() >= 90)
            {
                uint id = 0;

                if (player.Team.IsState() && player.InDuty)
                {
                    id = 111;
                } else
                {
                    id = 1;
                }

                var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == id);
                if (model == null) return;

                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(5000);

                player.IsTaskAllowed = false;

                await player.PlayAnimationAsync(33, "anim@heists@money_grab@duffel", "enter", 1);
                await Task.Delay(5000);

                lock (player) if (!RX.PlayerExists(player)) return;

                await player.SetArmorAsync(0);

                player.IsTaskAllowed = true;

                await player.SendNotify("Du hast deine Weste erfolgreich in deinem Rucksack verstaut!");
                await player.StopAnimationAsync();
                await player.disableAllPlayerActions(false);

                await player.SetClothesAsync(9, 0, 0);
                DiscordModule.Logs.Add(new DiscordLog("Inventory", (await player.GetNameAsync()) + " " + " packt " + model.Name + ".", DiscordModule.InventoryWebhook));

                player.Container.AddItem(model);
            }
            else
            {
                if (await player.GetArmorAsync() > 0)
                {
                    await player.SendNotify("Deine Weste darf nicht beschädigt sein!");
                }
            }
        }

        public static int GetWeaponPackItemIDByWeaponHash(WeaponHash weapon, bool BWeapon)
        {
            switch (weapon)
            {
                case WeaponHash.Specialcarbine:
                    return BWeapon ? 119 : 86; // erster wert ist itemid von der beamten waffe der zweite von der normalen so spart man sich code :).
                case WeaponHash.Advancedrifle:
                    return BWeapon ? 112 : 3; // erster wert ist itemid von der beamten waffe der zweite von der normalen so spart man sich code :).
                case WeaponHash.Bullpuprifle:
                    return BWeapon ? 131 : 88; // erster wert ist itemid von der beamten waffe der zweite von der normalen so spart man sich code :).
                case WeaponHash.Carbinerifle:
                    return BWeapon ? 117 : 87; // erster wert ist itemid von der beamten waffe der zweite von der normalen so spart man sich code :).
                case WeaponHash.Carbinerifle_mk2:
                    return BWeapon ? 118 : 133; // erster wert ist itemid von der beamten waffe der zweite von der normalen so spart man sich code :).
                case WeaponHash.Specialcarbine_mk2:
                    return BWeapon ? 120 : 132; // erster wert ist itemid von der beamten waffe der zweite von der normalen so spart man sich code :).
                case WeaponHash.Smg:
                    return BWeapon ? 116 : 0; // erster wert ist itemid von der beamten waffe der zweite von der normalen so spart man sich code :).
            }

            return 0;
        }

        public static int GetWeaponPackAmmoItemIDByWeaponHash(WeaponHash weapon)
        {
            switch (weapon)
            {
                case WeaponHash.Specialcarbine:
                case WeaponHash.Advancedrifle:
                case WeaponHash.Bullpuprifle:
                case WeaponHash.Carbinerifle:
                case WeaponHash.Carbinerifle_mk2:
                case WeaponHash.Specialcarbine_mk2:
                case WeaponHash.Smg:
                    return 130;
            }

            return 0;
        }

        public static int GetWeaponPackClipSizeByWeaponHash(WeaponHash weapon)
        {
            switch (weapon)
            {
                case WeaponHash.Specialcarbine:
                case WeaponHash.Advancedrifle:
                case WeaponHash.Bullpuprifle:
                case WeaponHash.Carbinerifle:
                case WeaponHash.Carbinerifle_mk2:
                case WeaponHash.Specialcarbine_mk2:
                case WeaponHash.Smg:
                    return 30;
            }

            return 0;
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task packGun(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !player.IsTaskAllowed) return;

            if (player.Dimension != 0 || player.CurrentWeapon == WeaponHash.Unarmed)
            {
                await player.SendNotify("Du kannst aktuell keine Waffen verstauen!");
                return;
            }

            WeaponHash currentWeapon = player.CurrentWeapon;
            WeaponLoadoutItem playerWeaponsWeapon = player.Weapons.Find(x => x.WeaponHash == currentWeapon.ToString());
            int currentWeaponAmmo = player.GetWeaponAmmo(currentWeapon);

            if (NAPI.Util.GetHashKey(currentWeapon.ToString()) == 0) return;
            if (playerWeaponsWeapon.BWeapon && !player.InDuty) return;

            RXItemModel itemModelWeapon = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == GetWeaponPackItemIDByWeaponHash(currentWeapon, playerWeaponsWeapon.BWeapon));
            if (itemModelWeapon == null) return;

            RXItemModel itemModelAmmo = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == GetWeaponPackAmmoItemIDByWeaponHash(currentWeapon));
            if (itemModelAmmo == null) return;

            int weaponClipSize = GetWeaponPackClipSizeByWeaponHash(currentWeapon);
            if (weaponClipSize == 0) return;
            int AmmoToAdd = currentWeaponAmmo / weaponClipSize;

            if (!player.Container.CanInventoryItemAdded(itemModelWeapon, 1))
            {
                await player.SendNotify("Du hast keinen Platz in deinen Taschen!");
                return;
            }

            if (AmmoToAdd > 0 && !player.Container.CanInventoryItemAdded(itemModelAmmo, AmmoToAdd))
            {
                await player.SendNotify("Du hast keinen Platz in deinen Taschen!");
                return;
            }

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(5000);

            player.IsTaskAllowed = false;

            player.StopAnimation();
            player.PlayAnimation("anim@heists@money_grab@duffel", "enter", 1);

            await Task.Delay(2500);

            player.StopAnimation();
            player.PlayAnimation("anim@heists@money_grab@duffel", "enter", 2);

            await Task.Delay(2500);

            lock (player) if (!RX.PlayerExists(player)) return;

            player.Container.AddItem(itemModelWeapon, 1);
            player.Container.AddItem(itemModelAmmo, AmmoToAdd);
            player.IsTaskAllowed = true;

            if (AmmoToAdd > 0)
            {
                await player.SendNotify($"Du hast deine {itemModelWeapon.Name} abgelegt und {AmmoToAdd} Magazine dazubekommen!", 5000, "green");
            }
            else
            {
                await player.SendNotify($"Du hast deine {itemModelWeapon.Name} abgelegt!", 5000, "green");
            }

            player.StopAnimation();
            await player.disableAllPlayerActions(false);

            player.SetWeaponAmmo(currentWeapon, 0);
            player.RemoveWeapon(currentWeapon);

            await player.RemoveWeaponFromLoadout((WeaponHash)NAPI.Util.GetHashKey(itemModelWeapon.WeaponHash));

            DiscordModule.Logs.Add(new DiscordLog("Inventory", $"{player.Name} hat seine {itemModelWeapon.Name} abgelegt und {AmmoToAdd} Magazine dazubekommen!", DiscordModule.InventoryWebhook));
        }

        [RemoteEvent]
        public async Task packblackmoney(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !player.IsTaskAllowed || await player.GetIsInVehicleAsync()) return;

            if (player.Blackmoney < 1)
            {
                await player.SendNotify("Du hast kein Schwarzgeld dabei!");
                return;
            }

            int blAmount = player.Blackmoney;
            int stackSize = ItemModelModule.ItemModels.Find(x => x.Id == 33).MaximumStackSize;

            player.Freezed = true;
            player.SetData<int>("packBlackMoney", blAmount);
            await player.disableAllPlayerActions(true);

            while (blAmount > 0)
            {
                if (!player.Container.CanInventoryItemAdded(ItemModelModule.ItemModels.Find(x => x.Id == 33), 1))
                {
                    await player.SendNotify($"Daf+r hast du keinen Platz!");
                    break;
                }

                int amountForCurrentItr = blAmount < stackSize ? blAmount : stackSize;

                await player.SendProgressbar(5000);
                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01");
                player.Blackmoney -= amountForCurrentItr;
                await Task.Delay(5000);

                player.Container.AddItem(33, amountForCurrentItr);
                await player.SendNotify($"Du hast deine {amountForCurrentItr}$ Schwarzgeld verpackt.");
                blAmount -= amountForCurrentItr;
                await Task.Delay(1000);
            }
            player.Freezed = false;
            player.ResetData("packBlackMoney");
            await player.disableAllPlayerActions(false);
            await player.StopAnimationAsync();
            DiscordModule.Logs.Add(new DiscordLog("Inventory", (await player.GetNameAsync()) + " " + " verpackt " + blAmount + "$ Schwarzgeld.", DiscordModule.InventoryWebhook));

        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task takeOutVest(RXPlayer player) => await player.SetClothesAsync(9, 0, 0);
    }


    public class InventoryPlayerClothesCategory
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Slot")]
        public int Slot { get; set; }

        [JsonProperty(PropertyName = "Items")]
        public List<InventoryPlayerClothesItem> Items { get; set; }
    }

    public class InventoryPlayerClothesItem
    {
        [JsonProperty(PropertyName = "Id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Prop")]
        public bool Prop { get; set; }

        public InventoryPlayerClothesItem()
        {
            Prop = false;
        }
    }
}