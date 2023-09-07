using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Bank;
using Backend.Modules.BlackMarket;
using Backend.Modules.Gangwar;
using Backend.Modules.Garage;
using Backend.Modules.Inventory;
using Backend.Modules.Native;
using Backend.Modules.Shops;
using Backend.Modules.Vehicle;
using Backend.Modules.Wardrobe;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Faction
{
    public enum TeamType
    {
        LSPD = 1,
        Medic = 2,
        Gang = 3,
        Mafia = 4,
        DMV = 5,
        DPOS = 6,
    }

    public class FrakMenuObject
    {
        [JsonProperty(PropertyName = "m")]
        public uint MagsCount { get; set; }

        [JsonProperty(PropertyName = "k")]
        public uint UseableMagsCount { get; set; }

        [JsonProperty(PropertyName = "p")]
        public uint MagPrice { get; set; }

        [JsonProperty(PropertyName = "a")]
        public uint DrugCounts { get; set; }

        [JsonProperty(PropertyName = "c")]
        public uint MaxDrugCounts { get; set; }

        [JsonProperty(PropertyName = "g")]
        public string GangwarName { get; set; }

        [JsonProperty(PropertyName = "d")]
        public uint MultiDrop { get; set; }

        [JsonProperty(PropertyName = "q")]
        public float DrugQuality { get; set; }

        [JsonProperty(PropertyName = "s")]
        public uint DrugPrice { get; set; }

        [JsonProperty(PropertyName = "l")]
        public bool FrakLeader { get; set; }

    }

    public class RXTeamCloth
    {
        public string Name { get; set; }
        public int ComponentId { get; set; }
        public int DrawableId { get; set; }
        public int TextureId { get; set; }
        public bool IsProp { get; set; }
        public bool Gender { get; set; }

        public RXTeamCloth(string name, int componentId, int drawableId, int textureId, bool prop, bool gender = true)
        {
            Name = name;
            ComponentId = componentId;
            DrawableId = drawableId;
            TextureId = textureId;
            IsProp = prop;
            Gender = gender;
        }
    }

    public class RXTeamEquipItem
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public int Amount { get; set; }

        public RXTeamEquipItem(string name, int price, int amount)
        {
            this.Name = name;
            this.Price = price;
            this.Amount = amount;
        }
    }

    class TeamModule : RXModule
    {
        public TeamModule() : base("Team", new RXWindow("Garage")) { }

        public string GetVehicleName(string model) => model;

        public static bool IsPlane(string model)
        {
            switch(model)
            {
                default: return false;
            }
        }

        public static Vector3 EntryStorage = new Vector3(116.346855, -1991.6023, 18.30044);
        public static Vector3 StorageInventory = new Vector3(107.690155, -2013.2461, 19.037737);
        public static Vector3 StorageInventory2 = new Vector3(114.46046, -2000.8934, 12.600731);
        public static Vector3 EquipPoint = new Vector3(114.54975, -1996.539, 12.600729);

        public static List<RXTeam> Teams = new List<RXTeam>();
        public static List<DbTeamArmory> TeamArmory = new List<DbTeamArmory>();

        public override async void LoadAsync()
        {
            await Task.Delay(3000);


            using var db = new RXContext();

            foreach (var team in await db.Teams.ToListAsync())
            {
                Teams.Add(new RXTeam() { Id = team.Id, BankPosition = team.BankPosition.ToPos(), ToggleDuty = team.ToggleDuty.ToPos(), ToggleDutyHeading = team.ToggleDutyHeading, ToggleDutyNPC = team.ToggleDutyNPC, Armory = team.Armory.ToPos(), ArmoryHeading = team.ArmoryHeading, ArmoryNPC = team.ArmoryNPC, Name = team.Name, NahKampfWeapon = (WeaponHash)NAPI.Util.GetHashKey(team.NahkampfWeapon), ShortName = team.ShortName, BlipColor = team.BlipColor, BlipType = team.BlipType, CanRegisterVehicles = team.CanRegisterVehicles, ColorId = team.ColorId, Dimension = team.Dimension, Equip = new List<RXTeamEquipItem>(), GangwarEnter = team.GangwarEnter.ToPos(), Garage = team.Garage.ToPos(), GarageSpawns = new List<RXGarageSpawn>(), HasDuty = team.HasDuty, HelicopterSpawn = new RXGarageSpawn(1, 0f, new Vector3(0, 0,0)), Hex = team.Hex, Image = team.Image, Logo = team.Logo, MedicPlayer = team.MedicPlayer, MOTD = team.MOTD, NPC = team.NPC, NPCHeading = team.NPCHeading, RGB = new Color(team.R, team.G, team.B), Salary = new Dictionary<int, int>(), Spawn = team.Spawn.ToPos(), Storage = team.Storage.ToPos(), TeamClothes = new List<RXTeamCloth>(), Type = (TeamType)team.Type, Wardrobe = team.Wardrobe.ToPos() });
            }

            TeamArmory = await db.TeamArmory.ToListAsync();

            Teams.ForEach(async team =>
            {

                if (team.NPC != null && team.Garage != null && team.NPCHeading != null)
                {
                    new NPC((PedHash)NAPI.Util.GetHashKey(team.NPC), team.Garage, team.NPCHeading, 0u);
                    // team.TeamClothes.forEachAlternative(cloth => cloth.Name = team.ShortName + " " + cloth.Name);
                    using var db = new RXContext();

                    var garagepoints = await db.TeamGaragePoints.Where(x => x.teamid == team.Id).ToListAsync();

                    foreach (var garagep in garagepoints)
                    {
                        team.GarageSpawns.Add(new RXGarageSpawn(garagep.id, garagep.heading, garagep.position.ToPos()));
                    }

                    var container = await db.Containers.FirstOrDefaultAsync(x => x.Name == team.ShortName);
                    if (container == null)
                    {
                        container = new DbContainer
                        {
                            Name = team.ShortName,
                            MaxSlots = 30,
                            MaxWeight = 500000,
                        };

                        await db.Containers.AddAsync(container);
                    }

                    {
                        //Fraktionslager inventory
                        await NAPI.Task.RunAsync(() =>
                        {
                            var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(team.Storage, 3f, 3f, 0u);

                            colShape.IsContainerColShape = true;
                            colShape.ContainerId = container.Id;
                            colShape.ContainerOpen = true;
                            colShape.ContainerCustomName = "Fraktionslager";
                            colShape.ContainerRestrictedTeam = team.Id;
                            colShape.ContainerType = 13;
                        });
                    }



                    DbBankAccount dbBankAccount = await db.BankAccounts.FirstOrDefaultAsync(con => con.Name == team.ShortName);
                    if (dbBankAccount == null)
                    {
                        dbBankAccount = new DbBankAccount
                        {
                            Name = team.ShortName,
                            Balance = 0,
                            History = new List<DbBankHistory>()
                        };

                        await db.AddAsync(dbBankAccount);
                    }

                    await db.SaveChangesAsync();
                }
                if (team.ToggleDuty != null && team.IsLowestState() && team.ToggleDutyNPC != "s_f_y_airhostess_01")
                {
                    new NPC((PedHash)NAPI.Util.GetHashKey(team.ToggleDutyNPC), team.ToggleDuty, team.ToggleDutyHeading, 0u);

                    var mcb = await NAPI.Entity.CreateMCB(team.ToggleDuty, new Color(255, 140, 0), 0u, 2.4f);

                    mcb.ColShape.Message = new RXMessage
                    {
                        Text = "Benutze E um deinen Dienst zu verwalten!",
                        Color = team.RGB.ConvertHTML(),
                        Duration = 3500,
                        Title = team.Name,
                        RestrictedToTeam = team.Id
                    };

                    mcb.ColShape.Action = async player => await OpenToggleDuty(player, team.Id);

                }



                if (team.Armory != null && team.IsLowestState() && team.ArmoryNPC != "s_f_y_airhostess_01")
                {
                    new NPC((PedHash)NAPI.Util.GetHashKey(team.ArmoryNPC), team.Armory, team.ArmoryHeading, 0u);

                    var mcb = await NAPI.Entity.CreateMCB(team.Armory, new Color(255, 140, 0), 0u, 2.4f);

                    mcb.ColShape.Message = new RXMessage
                    {
                        Text = "Benutze E um die Armory zu öffnen!",
                        Color = team.RGB.ConvertHTML(),
                        Duration = 3500,
                        Title = team.Name,
                        RestrictedToTeam = team.Id
                    };

                    mcb.ColShape.Action = async player => await OpenTeamArmory(player, team.Id);

                }


                if (team.IsLowestState())
                {
                    var mcb = await NAPI.Entity.CreateMCB(team.BankPosition, new Color(255, 140, 0), 0u, 2.4f);

                    mcb.ColShape.Message = new RXMessage
                    {
                        Text = "Benutze E um die Bank zu öffnen!",
                        Color = team.RGB.ConvertHTML(),
                        Duration = 3500,
                        Title = team.Name,
                        RestrictedToTeam = team.Id
                    };

                    mcb.ColShape.Action = async player => await TeamBank(player);
                    
                }


                {
                    if (team.Garage != null)
                    {
                        //Garage
                        var mcb = await NAPI.Entity.CreateMCB(team.Garage, new Color(255, 140, 0), 0u, 2.4f);

                        mcb.ColShape.Message = new RXMessage
                        {
                            Text = "Benutze E um die Garage zu öffnen!",
                            Color = team.RGB.ConvertHTML(),
                            Duration = 3500,
                            Title = team.Name,
                            RestrictedToTeam = team.Id
                        };

                        mcb.ColShape.Action = async player => await OpenTeamGarage(player, team.Id);

                        var mcbe = await NAPI.Entity.CreateMCB(team.Garage.Add(new Vector3(0,0,1)), new Color(0, 238, 255, 180), 187000, 1.4f, 1.4f, true, (MarkerType)36);

                        mcbe.ColShape.Message = new RXMessage
                        {
                            Text = "Benutze E um die Trainings-Garage zu öffnen!",
                            Color = team.RGB.ConvertHTML(),
                            Duration = 3500,
                            Title = team.Name,
                            RestrictedToTeam = team.Id
                        };

                        mcbe.ColShape.Action = async player => await OpenTeamGarage(player, team.Id);
                    }
                }
            
                {
                    if (team.Storage != null)
                    {
                        //Fraktionslager
                        var mcb = await NAPI.Entity.CreateMCB(team.Storage.Add(new Vector3(0, 0, 1)), team.RGB, 0u, 1.4f);//, 2.4f, 0.7f, true, MarkerType.ThickChevronUp);

                        mcb.ColShape.Message = new RXMessage
                        {
                            Text = "Benutze E um das Fraktionsmenü zu öffnen!",
                            Color = team.RGB.ConvertHTML(),
                            Duration = 3500,
                            Title = team.Name,
                            RestrictedToTeam = team.Id
                        };

                        mcb.ColShape.Action = async player => await EnterStorage(player, team.Id);
                    }
                }


                if (team.Wardrobe != null)
                {

                    {
                        //Wardrobe
                        var mcb = await NAPI.Entity.CreateMCB(team.Wardrobe, team.RGB, 0u, 1.4f); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                        mcb.ColShape.Message = new RXMessage
                        {
                            Text = "Benutze E um dich umzuziehen!",
                            Color = team.RGB.ConvertHTML(),
                            Duration = 3500,
                            Title = team.Name,
                            RestrictedToTeam = team.Id
                        };

                        mcb.ColShape.Action = async player => await WardrobeModule.OpenWardrobe(player);
                    }
                }
            });

      
        }

        public async Task OpenToggleDuty(RXPlayer player, uint teamid)
        {
            if (!player.CanInteract() || !await player.CanInteractAntiFloodNoMSG(2)) return;

            if (player.Team == null || player.Team.Id != teamid || !player.Team.IsLowestState()) return;

            if (!player.Team.HasDuty) return;

            if (player.InDuty)
            {
                object confirmationBoxObject = new
                {
                    t = "Willst du deinen Dienst Verlassen?",
                    ft = "Ja",
                    st = "Nein",
                    fe = "togglePlayerDuty",
                    se = "Close",
                    d = 0,
                };

                var confirmation = new RXWindow("Confirm");

                await confirmation.OpenWindow(player, confirmationBoxObject);

            }
            else
            {
                object confirmationBoxObject = new
                {
                    t = "Willst du deinen Dienst antreten?",
                    ft = "Ja",
                    st = "Nein",
                    fe = "togglePlayerDuty",
                    se = "Close",
                    d = 1,
                };

                var confirmation = new RXWindow("Confirm");

                await confirmation.OpenWindow(player, confirmationBoxObject);
            }
        }


        [RemoteEvent]
        public async Task togglePlayerDuty(RXPlayer player, uint dienst)
        {
            if (player.Team == null || !player.Team.IsLowestState()) return;

            if (!player.Team.HasDuty) return;

            player.InDuty = dienst == 1 ? true : false;
            await player.SendNotify("Du hast deinen Dienst " + (dienst == 1 ? "betreten" : "verlassen"));

            if (player.Team.IsState())
            {
                if (dienst == 1)
                {
                    await player.RemoveAllWeaponsAsync();
                    await player.AddWeaponToLoadout(WeaponHash.Stungun, true, 9999, true);
                    await player.AddWeaponToLoadout(WeaponHash.Pistol_mk2, true, 250, true);
                    await player.AddWeaponToLoadout(WeaponHash.Nightstick, true, 9999, true);
                    await player.AddWeaponToLoadout(WeaponHash.Flashlight, true, 9999, true);
                    await player.AddWeaponToLoadout(WeaponHash.Flare, true, 20, true);

                    if (player.Team.Id == 1)
                    {
                        if (player.IsMale)
                        {
                            await player.SetClothesAsync(8, 58, 0);
                            await player.SetClothesAsync(11, 55, 0);
                            await player.SetClothesAsync(3, 41, 0);
                            await player.SetClothesAsync(4, 25, 0);
                            await player.SetClothesAsync(6, 25, 0);
                        } else
                        {

                        }
                    }
                }
                else
                {
                    player.Weapons.Clear();
                    await player.RemoveAllWeaponsAsync();
                    await player.LoadCharacter();
                }
            }
            if (player.Team.Id == 3)
            {
                if (dienst == 1)
                {
                    await player.RemoveAllWeaponsAsync();
                    await player.AddWeaponToLoadout(WeaponHash.Stungun, true, 9999, true);
                    await player.AddWeaponToLoadout(WeaponHash.Flashlight, true, 9999, true);
                    await player.AddWeaponToLoadout(WeaponHash.Flare, true, 20, true);

                    if (player.IsMale)
                    {
                        await player.SetClothesAsync(8, 129, 0);
                        await player.SetClothesAsync(11, 349, 6);
                        await player.SetClothesAsync(3, 92, 0);
                        await player.SetClothesAsync(4, 24, 5);
                        await player.SetClothesAsync(6, 77, 0);
                        await player.SetClothesAsync(7, 127, 0);
                        await player.SetClothesAsync(10, 58, 0);
                    }
                    else
                    {
                        await player.SetClothesAsync(8, 159, 0);
                        await player.SetClothesAsync(11, 367, 6);
                        await player.SetClothesAsync(3, 101, 0);
                        await player.SetClothesAsync(4, 23, 0);
                        await player.SetClothesAsync(6, 81, 0);
                        await player.SetClothesAsync(7, 97, 0);
                        await player.SetClothesAsync(10, 66, 0);
                    }

                }
                else
                {
                    player.Weapons.Clear();
                    await player.RemoveAllWeaponsAsync();
                    await player.LoadCharacter();
                }
            }
            await player.TriggerEventAsync(PlayerDatas.DutyEvent, player.InDuty);

        }



        public async Task OpenTeamArmory(RXPlayer player, uint teamid)
        {
            try
            {
                if (!player.CanInteract() || !await player.CanInteractAntiFloodNoMSG(2)) return;

                if (player.Team == null || player.Team.Id != teamid || !player.Team.IsLowestState()) return;

                var shopitems = new List<RXShopProduct>();

                foreach (var item in TeamArmory.Where(x => x.TeamId == player.Team.Id))
                {
                    shopitems.Add(new RXShopProduct() { Id = item.ItemId, Image = ItemModelModule.ItemModels.Find(x => x.Id == item.ItemId).ImagePath, Name = ItemModelModule.ItemModels.Find(x => x.Id == item.ItemId).Name, Price = (int)item.Price });
                }


                var clientShop = new Models.RXShop
                {
                    Id = 999,
                    Title = "Waffenkammer " + player.Team.Name,
                    Items = shopitems,
                    Money = (uint)player.Team.BankAccount.Balance,
                    isRob = false,
                    isFrakshop = true,
                };

                await new RXWindow("Shop").OpenWindow(player, clientShop);

            } catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }

        }


        //[HandleExceptions]
        [RemoteEvent]
        public async Task FrakshopBuy(RXPlayer player, string json)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (!json.IsValidJson<List<ShopItemResponse>>()) return;

            List<ShopItemResponse> items = JsonConvert.DeserializeObject<List<ShopItemResponse>>(json);
            if (items == null || items.Count < 1) return;

            int totalAmount = 0;

            Dictionary<RXItemModel, int> itemsToAdd = new Dictionary<RXItemModel, int>();

            int weight = 0;
            int requiredSlots = 0;

            await items.forEach(async shopItemResponse =>
            {
                if (shopItemResponse.Amount < 1)
                {
                    RXLogger.Print("shopItemResponse.Amount < 1");

                    return;
                }

                var originalItem = TeamArmory.FirstOrDefault(shopItem => shopItem.ItemId == shopItemResponse.Id && shopItem.TeamId == player.Team.Id);
                if (originalItem == null)
                {
                    RXLogger.Print("originalItem konnte nicht gefunden werden!");
                    return;
                }

                var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == originalItem.ItemId);
                if (model == null)
                {
                    RXLogger.Print("Model konnte nicht gefunden werden!");
                    return;
                }

                itemsToAdd.Add(model, shopItemResponse.Amount);
                totalAmount = totalAmount + ((int)originalItem.Price * shopItemResponse.Amount);

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

            if (itemsToAdd.Count < 1)
            {
                RXLogger.Print("itemsToAdd Count is too low!");
                return;
            }
            if (player.Container.GetInventoryUsedSpace() + weight > player.Container.MaxWeight)
            {
                await player.SendNotify("Du hast keinen Platz im Inventar!", 3500, "red", "Waffenkammer");
                return;
            }

            if (player.Container.GetUsedSlots() + requiredSlots > player.Container.MaxSlots)
            {
                await player.SendNotify("Du hast keinen Platz im Inventar!", 3500, "red", "Waffenkammer");
                return;
            }

            PaymentModule.CreateFrakPayment(player, totalAmount, async player =>
            {
                await RX.GiveMoneyToStaatskonto(totalAmount, "Store - Einkauf - " + await player.GetNameAsync());

                await itemsToAdd.forEach(item => player.Container.AddItem(item.Key, item.Value));
                await player.SendNotify("Du hast einen Einkauf im Wert von " + totalAmount.FormatMoneyNumber() + " getätigt.", 3500, "green", "Waffenkammer");

            }, "Store - Einkauf", false, false);
        }

        public async Task ProcessWeed(RXPlayer player)
        {
            if (!player.CanInteract() || !await player.CanInteractAntiFloodNoMSG(2)) return;

            if (await NAPI.Task.RunReturnAsync(() => player.HasData("ProcessingWeed") && player.GetData<bool>("ProcessingWeed")))
            {
                player.IsTaskAllowed = true;

                await player.disableAllPlayerActions(false);
                await NAPI.Task.RunAsync(() => player.SetData("ProcessingWeed", false));
                await player.SendNotify("Du hast mit dem Abpacken aufgehört!");

                await player.StopAnimationAsync();
            }
            else
            {
                player.IsTaskAllowed = false;

                await player.disableAllPlayerActions(true);
                await NAPI.Task.RunAsync(() => player.SetData("ProcessingWeed", true));
                await player.SendNotify("Du hast mit dem Abpacken angefangen!");

                await player.StopAnimationAsync();
            }
        }

        [RemoteEvent]
        public async Task GovMsg(RXPlayer player, uint ka, string message)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (player.Teamrank < 5) return;

            if (player.Team.Id == 1)
            {
                RX.SendGlobalNotifyToAll("LSPD: " + message, 10000, "white", Icon.LSPD);
            } else if (player.Team.Id == 3)
            {
                RX.SendGlobalNotifyToAll("LSMC: " + message, 10000, "white", Icon.LSMC);
            } else if (player.Team.Id == 5)
            {
                RX.SendGlobalNotifyToAll("FIB: " + message, 10000, "white", Icon.LSPD);
            }
        }

        [RemoteEvent]
        public async Task TeamBank(RXPlayer player)
        {
            try
            {
                if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

                if (player.Team.Id == 0) return;

                if (!player.TeamMemberData.Bank)
                {
                    await player.SendNotify("Du hast kein Zugriff auf die Fraktionsbank!");
                    return;
                }

                var playerBank = player.Team.BankAccount;
                if (playerBank == null)
                {
                    await player.SendNotify("Es gab einen Fehler!");
                    return;
                }

                var bank = new RXBank
                {
                    Title = player.Team.ShortName + " Fraktionsbank",
                    Balance = playerBank.Balance,
                    Money = player.Cash,
                    BankId = player.Team.Id,
                    DepositeeFeeMin = 0,
                    DepositFeeMax = 0,
                    DepositFeePer = 0,
                    WithdrawFeeMax = 0,
                    WithdrawFeePer = 0,
                    Frakbank = true,
                };

                await playerBank.History.forEach(history =>
                {

                });

                var window = new RXWindow("Bank");

                await window.OpenWindow(player, bank);

            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }


        [RemoteEvent]
        public async Task RqFrakBankInfo(RXPlayer player)
        {
            if (player == null) return;
            await player.TriggerEventAsync("RsBankInfo", NAPI.Util.ToJson(player.Team.BankAccount.History));
        }


        public override async Task OnFiveSecond()
        {
            var list = await NAPI.Task.RunReturnAsync(() => PlayerController.GetValidPlayers().ToList().Where(x => x.HasData("ProcessingWeed") && x.GetData<bool>("ProcessingWeed")));

            RXItemModel model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Weed");
            if (model == null) return;

            RXItemModel model2 = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Weedtüte");
            if (model2 == null) return;

            foreach (RXPlayer player in list)
            {
                await player.PlayAnimationAsync(33, "amb@prop_human_parking_meter@male@base", "base");
                await player.disableAllPlayerActions(true);

                NAPI.Task.Run(async () =>
                {
                    if (player.HasData("ProcessingWeed") && player.GetData<bool>("ProcessingWeed"))
                    {

                        if (player.Container != null && player.Container.GetItemAmount(model) > 4)
                        {
                            player.Container.RemoveItem(model, 5);
                            player.Container.AddItem(model2, 1);

                            await player.StopAnimationAsync();
                        }
                        else
                        {
                            player.SetData("ProcessingWeed", false);
                            player.IsTaskAllowed = true;

                            await player.disableAllPlayerActions(false);
                            await player.SendNotify("Du hast das Abpacken erfolgreich abgeschlossen!");
                            await player.StopAnimationAsync();
                        }
                    }
                }, 4000);
            }
        }

        //[HandleExceptions]
        public async Task OpenEquippoint(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.TeamId == 0 || player.Team == null || !await player.CanInteractAntiFloodNoMSG(1)) return;

            var nativeMenu = new NativeMenu("Ausrüstung", "", new List<NativeItem>()
            {
                new NativeItem("Schließen", player => player.CloseNativeMenu())
            });

            await player.Team.Equip.forEach(item =>
            {
                nativeMenu.Items.Add(new NativeItem(item.Amount + "x " + item.Name + " - " + item.Price.FormatMoneyNumber(), async player =>
                {
                    player.CloseNativeMenu();

                    var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == item.Name);
                    if (model == null) return;

                    if (!player.Container.CanInventoryItemAdded(model, item.Amount))
                    {
                        await player.SendNotify("Du hast keinen Platz im Inventar!", 3500, "red");
                        return;
                    }

                    if (await player.TakeMoney(item.Price))
                    {
                        player.Container.AddItem(model, item.Amount);

                        await player.SendNotify("Du hast dich mit " + item.Amount + "x " + item.Name + " ausgerüstet!");
                    }
                    else
                    {
                        await player.SendNotify("Du hast nicht genügend Bargeld bei dir!", 3500, "red");
                    }
                }));
            });

            player.ShowNativeMenu(nativeMenu);
        }

        [RemoteEvent]
        public async Task OpenGangwarEnter(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.TeamId == 0 || player.Team == null || !await player.CanInteractAntiFloodNoMSG(1)) return;

            RXWindow weaponselect = new RXWindow("GangwarWeaponSelect");
            await weaponselect.OpenWindow(player);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task BuyEquip(RXPlayer player, uint equipid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.TeamId == 0 || player.Team == null || player.TeamId == 0 || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if (equipid == 0)
            {
                if (await player.TakeMoney(500))
                {
                    await player.AddWeaponToLoadout(player.Team.NahKampfWeapon, true, 9999);
                }
                else
                {
                    await player.SendNotify("Du hast nicht genug Geld dabei!");
                }
            } else if (equipid == 1)
            {
                if (await player.TakeMoney(500))
                {
                    await player.AddWeaponToLoadout(WeaponHash.Heavypistol, true, 250);
                }
                else
                {
                    await player.SendNotify("Du hast nicht genug Geld dabei!");
                }
            } else if (equipid == 2)
            {
                if (await player.TakeMoney(2500))
                {
                    await player.AddWeaponToLoadout(WeaponHash.Pistol_mk2, true, 250);
                }
                else
                {
                    await player.SendNotify("Du hast nicht genug Geld dabei!");
                }
            }

 
        }

        public async Task EnterStorage(RXPlayer player, uint teamId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.TeamId == 0 || player.Team == null || player.TeamId != teamId || !await player.CanInteractAntiFloodNoMSG(1)) return;

            var window = new RXWindow("Base");

            var teammenu = new FrakMenuObject() { MagPrice = 2500, FrakLeader = player.Teamrank > 9, MultiDrop = 0, UseableMagsCount = 0, MaxDrugCounts = 100, DrugCounts = 0, DrugPrice = 1000, MagsCount = 0, DrugQuality = 0 };


            var gangwar = GangwarModule.Gangwars.Find(x => x.AttackerId == teamId && x.IsGettingAttacked || x.TeamId == teamId && x.IsGettingAttacked);

            if (gangwar != null)
            {
                teammenu.GangwarName = gangwar.Name;
            }

            await window.OpenWindow(player, teammenu);
        }

        //[HandleExceptions]
        public async Task OpenTeamGarage(RXPlayer player, uint teamId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.TeamId != teamId || !await player.CanInteractAntiFloodNoMSG(1)) return;

            var team = Teams.FirstOrDefault(x => x.Id == teamId);
            if (team == null) return;

            using var db = new RXContext();

            List<DbTeamVehicle> dbVehicles = await db.TeamVehicles.Where(x => x.Stored == true && x.TeamId == player.Team.Id).ToListAsync();

            List<GarageVehicle> garageVehicles = new List<GarageVehicle>();
            garageVehicles = dbVehicles.ConvertAll(x => new GarageVehicle(x.Id, VehicleModelModule.VehicleModels.FirstOrDefault(model => model.Hash == x.Hash)?.Name, player.Team.ShortName.ToUpper()));

            await this.Window.OpenWindow(player, new RXGarageNew() { Id = player.Team.Id, Name = player.Team.Name, Storage = false, data = garageVehicles });

            await NAPI.Task.RunAsync(() => player.SetData("teamGarage", true));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task requestTeamVehicleList(RXPlayer player, uint teamId, string state)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.TeamId != teamId || (state != "takeout" && state != "takein") || await NAPI.Task.RunReturnAsync(() => !player.HasData("teamGarage"))) return;

            var team = Teams.FirstOrDefault(x => x.Id == teamId);
            if (team == null) return;

            List<GarageVehicle> garageVehicles = new List<GarageVehicle>();

            if (state == "takeout") // Ausparken
            {
                using var db = new RXContext();

                List<DbTeamVehicle> dbVehicles = await db.TeamVehicles.Where(x => x.Stored == true && x.TeamId == teamId).ToListAsync();

                garageVehicles = dbVehicles.ConvertAll(x => new GarageVehicle(x.Id, GetVehicleName(x.Hash), player.Team.ShortName));
            }
            else if (state == "takein") // Einparken
            {
                List<RXVehicle> vehicles = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllVehicles().ToList().ConvertAll(v => (RXVehicle)v).Where(x => x.Position.DistanceTo(player.Position) <= 25 && x.ModelData == null && x.TeamId == player.TeamId).ToList());

                garageVehicles = vehicles.ConvertAll(x => new GarageVehicle(x.Id, GetVehicleName(x.TeamVehicleModel), player.Team.ShortName));
            }

            await this.Window.TriggerEvent(player, "responseVehicleList", JsonConvert.SerializeObject(garageVehicles));
        }

        //[HandleExceptions]
        public static async Task requestTeamVehicle(RXPlayer player, string state, uint teamId, uint vehicleId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.TeamId != teamId || (state != "takeout" && state != "takein") || await NAPI.Task.RunReturnAsync(() => !player.HasData("teamGarage"))) return;

            var team = Teams.FirstOrDefault(x => x.Id == teamId);
            if (team == null) return;

            if (state == "takeout") // Ausparken
            {
                RXGarageSpawn garageSpawn = null;

                List<RXVehicle> vehicles = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllVehicles().ConvertAll(x => (RXVehicle)x));

                foreach (var spawn in team.GarageSpawns)
                {
                    if (vehicles.FirstOrDefault(v => NAPI.Task.RunReturn(() => v.Position).DistanceTo(spawn.Position) <= 2f) == null)
                    {
                        garageSpawn = spawn;

                        break;
                    }
                }

                using var db = new RXContext();

                var dbVehicle = await db.TeamVehicles.FirstOrDefaultAsync(x => x.Id == vehicleId && x.TeamId == player.TeamId);
                if (dbVehicle == null) return;

                if (IsPlane(dbVehicle.Hash)) garageSpawn = team.HelicopterSpawn;

                if (garageSpawn == null)
                {
                    await player.SendNotify("Es ist aktuell kein Parkplatz für das Fahrzeug vorhanden!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);

                    return;
                }

                dbVehicle.Stored = false;

                await db.SaveChangesAsync();

                await NAPI.Task.RunAsync(async () =>
                {
                    RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey(dbVehicle.Hash), garageSpawn.Position, garageSpawn.Heading, 0, 0, player.Team.ShortName, 255, true, true, player.Dimension);

                    vehicle.Id = dbVehicle.Id;
                    vehicle.ModelData = VehicleModelModule.VehicleModels.Find(x => x.Hash == dbVehicle.Hash);
                    vehicle.Fuel = dbVehicle.Fuel;
                    if (vehicle.TeamId != 1)
                    {
                        vehicle.CustomPrimaryColor = new Color(dbVehicle.R, dbVehicle.G, dbVehicle.B);
                    }
                    vehicle.OwnerId = 0;
                    vehicle.TeamId = dbVehicle.TeamId;
                    vehicle.ContainerId = dbVehicle.ContainerId;
                    vehicle.Plate = "";
                    vehicle.Distance = dbVehicle.Distance;
                    vehicle.RXLivery = dbVehicle.Livery;
                    vehicle.TeamVehicleModel = dbVehicle.Hash;
                    vehicle.SetEngineStatus(false);
                    vehicle.SetLocked(true);
                });
            }
            else if (state == "takein") // Einparken
            {
                await NAPI.Task.RunAsync(() =>
                {
                    var veh = NAPI.Pools.GetAllVehicles().ConvertAll(v => (RXVehicle)v).FirstOrDefault(x => x.Id == vehicleId && x.TeamId == player.TeamId);
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

                var dbVehicle = await db.TeamVehicles.FirstOrDefaultAsync(x => x.Id == vehicleId && x.TeamId == player.TeamId);
                if (dbVehicle == null) return;

                dbVehicle.Stored = true;

                await db.SaveChangesAsync();
            }

            await player.SendNotify("Das Fahrzeug wurde erfolgreich " + (state == "takeout" ? "aus" : "ein") + "geparkt!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
        }
    }
}
