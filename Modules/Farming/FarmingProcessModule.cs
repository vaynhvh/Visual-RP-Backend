using Backend.Models;
using Backend.Modules.Inventory;
using Backend.Modules.Native;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Farming
{
    class FarmingProcessModule : RXModule
    {
        public FarmingProcessModule() : base("FarmingProcess") { }

        public static List<DbFarmingProcess> FarmingProcesses = new List<DbFarmingProcess>();   

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            FarmingProcesses = await db.FarmingProcess.ToListAsync();

            foreach (var process in FarmingProcesses)
            {
                var mcb = await NAPI.Entity.CreateMCB(process.Position.ToPos(), new Color(255, 140, 0), 0u, 1.4f, 2.4f, false, MarkerType.VerticalCylinder, false);

                mcb.ColShape.Action = async player => await OpenProcessMenu(player, process);
                mcb.ColShape.Message = new RXMessage
                {
                    Text = $"Drücke E um {process.Name} zu verarbeiten!",
                    Color = "green",
                    Duration = 3500,
                    Title = "Farming",
                    OnlyBadFaction = process.Illegal,
                };

                await NAPI.Task.RunAsync(() =>
                {
                    new NPC((PedHash)NAPI.Util.GetHashKey(process.PedHash), process.Position.ToPos(), process.PedRotation, 0u);
                });
            }
        }

        public async Task OpenProcessMenu(RXPlayer player, DbFarmingProcess process)
        {
            if (player == null) return;
            if (process == null) return;

            if (await player.GetIsInVehicleAsync()) return;
            if (player.Freezed) return;
            if (!player.CanInteract()) return;

            var item = ItemModelModule.ItemModels.Find(x => x.Id == process.RequiredItemId);
            var finalitem = ItemModelModule.ItemModels.Find(x => x.Id == process.ProcessItemId);


            var nativeMenu = new NativeMenu("Verarbeiter", "", new List<NativeItem>()
            {
                new NativeItem("Schließen", player => player.CloseNativeMenu()),
                new NativeItem(item.Name + " verarbeiten (" + process.MinCount + "x benötigt)", async player => await ProcessItem(player, process)),
            });

            player.ShowNativeMenu(nativeMenu);

        }

        public async Task ProcessItem(RXPlayer player, DbFarmingProcess process)
        {
            if (player == null) return;
            if (process == null) return;

            if (await player.GetIsInVehicleAsync()) return;
            if (player.Freezed) return;
            if (!player.CanInteract()) return;
            player.CloseNativeMenu();
            player.Freezed = true;
            await player.disableAllPlayerActions(true);
            await player.SendProgressbar((int)process.Time);
            await Task.Delay((int)process.Time);

            player.Freezed = false;
            await player.disableAllPlayerActions(false);

            var item = ItemModelModule.ItemModels.Find(x => x.Id == process.RequiredItemId);
            var finalitem = ItemModelModule.ItemModels.Find(x => x.Id == process.ProcessItemId);

            if (player.Container.GetItemAmount(item.Id) < process.MinCount)
            {
                await player.SendNotify("Du hast nicht genug von " + item.Name + " um dies zu verarbeiten!");
                return;
            }

            if (!player.Container.CanInventoryItemAdded(finalitem, (int)process.MaxCount))
            {
                await player.SendNotify("Du kannst so viel nicht tragen!");
                return;
            }

            player.Container.RemoveItem(item, (int)process.MinCount);
            player.Container.AddItem(finalitem, (int)process.MaxCount);
            await player.SendNotify("Du hast erfolgreich " + process.MinCount + "x "+ item.Name + " zu " + (int)process.MaxCount + "x " + finalitem.Name + " verarbeitet!");


        }

    }
}
