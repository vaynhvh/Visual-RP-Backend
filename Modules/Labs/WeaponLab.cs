using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
using Backend.Modules.Jail;
using Backend.Modules.Native;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Labs
{

    class WeaponLab : RXModule
    {
        public WeaponLab() : base("WeaponLab") { }

        public static List<DbWeaponLab> WeaponLabs = new List<DbWeaponLab>();

        public static List<uint> RessourceItemIds = new List<uint> { 47, 67, 68, 69 }; //Eisenbarren, Plastik, Abzug, Alubarren
        public static uint EndProductItemId = 66; //Waffenset
        public static uint FuelItemId = 65; //Benzin
        public static uint FuelAmountPerProcessing = 5; //Fuelverbrauch pro 15-Minuten-Kochvorgang (Spielerunabhängig)
        public List<RXTeam> HasAlreadyHacked = new List<RXTeam>();

        // Item Id, Price (Aufpreis)
        public Dictionary<uint, int> WeaponHerstellungList = new Dictionary<uint, int>()
        {
            { 3, 40000 }, // Advanced
        };

        public override async void LoadAsync()
        {
            try
            {
                RequireModule("Team");

                await Task.Delay(8000);
                using var db = new RXContext();
                foreach (var lab in await db.WeaponLabs.ToListAsync())
                {
                    WeaponLabs.Add(lab);
                }

                foreach (var lab in WeaponLabs)
                {
                    var team = TeamModule.Teams.Find(x => x.Id == lab.TeamId);

                    if (team == null)
                    {
                        RXLogger.Print("Dieses Team existiert nicht!");
                        return;
                    }
                    var labcomputer = await NAPI.Entity.CreateMCB(LabCoords.WeaponlaboratoryComputerPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    labcomputer.ColShape.Action = async player => await EnterLabComputer(player, lab);

                    var labweaponbuildmenu = await NAPI.Entity.CreateMCB(LabCoords.WeaponlaboratoryWeaponBuildMenuPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    labweaponbuildmenu.ColShape.Action = async player => await OpenWeaponBuildMenu(player, lab);

                    var labweaponinputinv = await NAPI.Entity.CreateMCB(LabCoords.WeaponlaboratoryInvInputPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    var labweaponoutputinv = await NAPI.Entity.CreateMCB(LabCoords.WeaponlaboratoryInvOutputPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    var labweaponfuelinv = await NAPI.Entity.CreateMCB(LabCoords.WeaponlaboratoryInvFuelPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, true, MarkerType.HorizontalCircleSkinny, false);
                    var labenter = await NAPI.Entity.CreateMCB(lab.Position.ToPos(), new Color(255, 140, 0), 0u, 1.4f, 2.4f, false, MarkerType.HorizontalCircleSkinny, false);
                    var labexit = await NAPI.Entity.CreateMCB(LabCoords.WeaponlaboratoryEntranceExitPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, false, MarkerType.HorizontalCircleSkinny, false);

                    var labenterlock = await NAPI.Entity.CreateMCB(lab.Position.ToPos(), new Color(255, 140, 0), 0u, 1.4f, 2.4f, false, MarkerType.HorizontalCircleSkinny, false);
                    var labexitlock = await NAPI.Entity.CreateMCB(LabCoords.WeaponlaboratoryEntranceExitPosition, new Color(255, 140, 0), team.Dimension, 1.4f, 2.4f, false, MarkerType.HorizontalCircleSkinny, false);

                    labenterlock.ColShape.ColShapeKeyType = ColShape.ColShapeKeyType.L;
                    labenterlock.ColShape.Action = async player => await ToggleLabLock(player, lab);

                    labexitlock.ColShape.ColShapeKeyType = ColShape.ColShapeKeyType.L;
                    labexitlock.ColShape.Action = async player => await ToggleLabLock(player, lab);

                    labexit.ColShape.Message = new RXMessage
                    {
                        Text = "Benutze E um das Labor zu verlassen!",
                        Color = team.RGB.ConvertHTML(),
                        Duration = 3500,
                        Title = "Waffenlabor " + lab.Name,
                        RestrictedToTeam = lab.TeamId,
                    };

                    labexit.ColShape.Action = async player => await LeaveLab(player, lab);

                    labenter.ColShape.Message = new RXMessage
                    {
                        Text = "Benutze E um das Labor zu betreten!",
                        Color = team.RGB.ConvertHTML(),
                        Duration = 3500,
                        Title = "Waffenlabor " + lab.Name,
                        RestrictedToTeam = lab.TeamId,
                    };

                    labenter.ColShape.Action = async player => await EnterLab(player, lab);

                    var fuelcontainer = await db.Containers.FirstOrDefaultAsync(x => x.Id == lab.FuelContainerId);
                    if (fuelcontainer == null)
                    {
                        var dblab = await db.WeaponLabs.FirstOrDefaultAsync(x => x.Id == lab.Id);

                        fuelcontainer = new DbContainer
                        {
                            Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                            Name = "Labor Fuel",
                            MaxSlots = 60,
                            MaxWeight = 300000,
                        };

                        dblab.FuelContainerId = fuelcontainer.Id;
                        lab.FuelContainerId = fuelcontainer.Id;

                        await db.Containers.AddAsync(fuelcontainer);
                        await db.SaveChangesAsync();

                    }


                    await NAPI.Task.RunAsync(() =>
                    {
                        labweaponfuelinv.ColShape.IsContainerColShape = true;
                        labweaponfuelinv.ColShape.ContainerId = lab.FuelContainerId;
                        labweaponfuelinv.ColShape.ContainerOpen = true;
                        labweaponfuelinv.ColShape.ContainerRestrictedTeam = team.Id;

                    });

                }
            } catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        public async Task OpenWeaponBuildMenu(RXPlayer player, DbWeaponLab lab)
        {

            if (player.TeamId == lab.TeamId)
            {
                var nativeMenu = new NativeMenu("Herstellung", "", new List<NativeItem>()
                {
                    new NativeItem("Schließen", player => player.CloseNativeMenu()),
                });

                foreach (KeyValuePair<uint, int> kvp in WeaponHerstellungList)
                {
                    var witem = ItemModelModule.ItemModels.Find(x => x.Id == kvp.Key);
                    if (witem == null) continue;
                    nativeMenu.Items.Add(new NativeItem(witem.Name + " - " + kvp.Value + "$", async player => { player.CloseNativeMenu(); await BuildWeapon(player, lab, kvp); }));
                }

                player.ShowNativeMenu(nativeMenu);
            }


        }
        public async Task BuildWeapon(RXPlayer player, DbWeaponLab lab, KeyValuePair<uint, int> kvp)
        {
            if (player.Container.GetItemAmount(66) <= 0)
            {
                await player.SendNotify("Du hast kein Waffenset!");
                return;
            }
            var witem = ItemModelModule.ItemModels.Find(x => x.Id == kvp.Key);
            if (witem == null) return;

            if (!player.Container.CanInventoryItemAdded(witem))
            {
                await player.SendNotify("Du hast nicht genug Platz!");
            }

            if (!await player.TakeBlackMoney(kvp.Value))
            {
                await player.SendNotify("Du hast nicht genug Schwarzgeld für diese Aktion!");
            }
            player.Container.RemoveItem(66);
            await player.SendProgressbar(15000);
            player.Freezed = true;
            await player.disableAllPlayerActions(true);
            await Task.Delay(15000);
            player.Freezed = false;
            await player.disableAllPlayerActions(false);

            player.Container.AddItem(witem, 1);
            await player.SendNotify($"Du hast {witem.Name} für {kvp.Value}$ hergestellt!");
        }

        public async Task EnterLabComputer(RXPlayer player, DbWeaponLab lab)
        {

            if (player.TeamId == lab.TeamId)
            {
                if (lab.ActivePlayers.Contains(player)) await StopProcess(player, lab);
                else await StartProcess(player, lab);
                return;
            }

            var nativeMenu = new NativeMenu("Labor Inhalte", "", new List<NativeItem>()
            {
                new NativeItem("Schließen", player => player.CloseNativeMenu()),
                new NativeItem("Durchsuchen", async player => { player.CloseNativeMenu(); await FriskLaboratory(player, lab); }),
            });


            player.ShowNativeMenu(nativeMenu);
        }

        public async Task<bool> FriskLaboratory(RXPlayer dbPlayer, DbWeaponLab lab)
        {
            if (dbPlayer.TeamId == 0)
            {
                return false;
            }
            if (!lab.HackInProgress)
            {
                await dbPlayer.SendNotify("Das Labor muss zuerst gehackt werden...");
                return false;
            }
            if (lab.FriskInProgress)
            {
                await dbPlayer.SendNotify("Das Labor wird schon durchsucht...");
                return false;
            }
            lab.FriskInProgress = true;

            Dictionary<int, int> itemsFound = new Dictionary<int, int>();
            foreach (int itemId in RessourceItemIds)
            {
                itemsFound.Add(itemId, 0);
            }

            itemsFound.Add((int)EndProductItemId, 0);

            bool found = false;
            int timeToFrisk = LabManager.TimeToFrisk;

            dbPlayer.Team.SendNotification("Das Labor  wird durchsucht...", 60000);
            timeToFrisk = timeToFrisk * 3;

            await dbPlayer.SendProgressbar(timeToFrisk);
            await dbPlayer.PlayAnimationAsync(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01");
            await dbPlayer.TriggerEventAsync("freezePlayer", true);
            await Task.Delay(timeToFrisk);
            if (dbPlayer == null || !dbPlayer.CanInteract())
            {
                lab.FriskInProgress = false;
                return false;
            }
            await dbPlayer.TriggerEventAsync("freezePlayer", false);
            await dbPlayer.StopAnimationAsync();
            foreach (var member in PlayerController.GetValidPlayers().Where(x => x.TeamId == dbPlayer.TeamId))
            { 
                if (member == null) continue;
                {
                    var inputcontainer = ContainerModule.Containers.Find(x => x.Id == member.LabInputContainerId);
                    foreach (var kvpSlots in inputcontainer.Slots.ToList())
                    {
                        if (kvpSlots != null & kvpSlots.Amount > 0)
                        {
                            itemsFound[(int)kvpSlots.ItemModelId] += kvpSlots.Amount;
                            found = true;
                        }
                    }
                }
                var outputcontainer = ContainerModule.Containers.Find(x => x.Id == member.LabOutputContainerId);

                if (outputcontainer != null)
                {
                    foreach (var kvpSlots in outputcontainer.Slots.ToList())
                    {
                        if (kvpSlots != null & kvpSlots.Amount > 0)
                        {
                            itemsFound[(int)kvpSlots.ItemModelId] += kvpSlots.Amount;
                            found = true;
                        }
                    }
                }
            }
            if (found)
            {
                string info = "Funde: ";
                foreach (KeyValuePair<int, int> kvp in itemsFound)
                    info += kvp.Value + " " + ItemModelModule.ItemModels.Find(x => x.Id == kvp.Key).Name + ", ";
                info = info.Substring(0, info.Length - 1);
                await dbPlayer.SendNotify(info);
            }
            else
            {
                await dbPlayer.SendNotify("Nichts gefunden");
            }
            lab.FriskInProgress = false;
            return false;
        }

        public async Task EnterLab(RXPlayer player, DbWeaponLab lab)
        {

            if (lab.Locked)
            {
                await player.SendNotify("Die Tür ist abgeschlossen!", 3500, "red");
                return;
            }
            var team = TeamModule.Teams.Find(x => x.Id == lab.TeamId);


            await player.SetDimensionAsync(team.Dimension);

            await player.SetPositionAsync(LabCoords.WeaponlaboratoryEntranceExitPosition);
        }

        public async Task ToggleLabLock(RXPlayer player, DbWeaponLab lab)
        {
            if (player.TeamId != lab.TeamId) return;
            if (lab.Locked)
            {
                await player.SendNotify("Du hast die Tür aufgeschlossen!", 3500, "green");
                lab.Locked = false;
            } else
            {
                await player.SendNotify("Du hast die Tür abgeschlossen!", 3500, "red");
                lab.Locked = true;
            }
        }
        

        public async Task LeaveLab(RXPlayer player, DbWeaponLab lab)
        {

            if (lab.Locked)
            {
                await player.SendNotify("Die Tür ist abgeschlossen!", 3500, "red");
                return;
            }
            var team = TeamModule.Teams.Find(x => x.Id == lab.TeamId);


            await player.SetDimensionAsync(0);

            await player.SetPositionAsync(lab.Position.ToPos());
        }
        public async Task Processing(RXPlayer dbPlayer, DbWeaponLab lab)
        {
            if (!lab.ActivePlayers.ToList().Contains(dbPlayer)) return;

            foreach (uint itemId in RessourceItemIds)
            {
                if (dbPlayer.LabInputContainer.GetItemAmount(itemId) < 1 ||
                    !dbPlayer.LabOutputContainer.CanInventoryItemAdded(ItemModelModule.ItemModels.Find(x => x.Id == EndProductItemId)))
                {
                    await StopProcess(dbPlayer, lab);
                    return;
                }
            }
            foreach (uint itemId in RessourceItemIds)
                dbPlayer.LabInputContainer.RemoveItem(itemId, 1);

            dbPlayer.LabOutputContainer.AddItem(EndProductItemId);
        }

        public async Task StopProcess(RXPlayer dbPlayer, DbWeaponLab lab)
        {
            if (lab.ActivePlayers.ToList().Contains(dbPlayer))
            {
                lab.ActivePlayers.Remove(dbPlayer);
                await dbPlayer.SendNotify("Prozess gestoppt!");
            }
        }

        public async Task StartProcess(RXPlayer dbPlayer, DbWeaponLab lab)
        {
            int menge = 1;

            foreach (uint itemId in RessourceItemIds)
            {
                if (dbPlayer.LabInputContainer.GetItemAmount(itemId) < menge)
                {
                    await StopProcess(dbPlayer, lab);
                    await dbPlayer.SendNotify("Es fehlen Materialien... ( " + menge + " " + ItemModelModule.ItemModels.Find(x => x.Id == itemId).Name + " )");
                    return;
                }
                uint fuelAmount = (uint)lab.LabFuelContainer.GetItemAmount(FuelItemId);
                if (fuelAmount < FuelAmountPerProcessing)
                {
                    await dbPlayer.SendNotify("Es fehlt Kraftstoff...");
                    await StopProcess(dbPlayer, lab);
                    return;
                }
            }
            if (lab.ActivePlayers.ToList().Contains(dbPlayer))
            {
                await dbPlayer.SendNotify("Der Prozess ist bereits im Gange...");
                return;
            }
            lab.ActivePlayers.Add(dbPlayer);
            await dbPlayer.SendNotify("Prozess gestartet!");
        }
    }
}
