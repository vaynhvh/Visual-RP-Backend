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
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Laptop.Apps
{
    class ExportApp : RXModule
    {
        public ExportApp() : base("ExportApp", new RXWindow("ExportApp")) { }

        public static List<DbItemExport> ItemExports = new List<DbItemExport>();
        public static List<DbItemExportItem> ItemExportItems = new List<DbItemExportItem>();

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            
            foreach (var export in await db.ItemExports.ToListAsync())
            {
                ItemExports.Add(export);

                await NAPI.Task.RunAsync(() =>
                {
                    new NPC((PedHash)NAPI.Util.GetHashKey(export.PedHash), export.Position.ToPos(), export.PedHeading, 0u);
                });

                var mcb = await NAPI.Entity.CreateMCB(export.Position.ToPos(), new Color(255, 140, 0), 0u, 2f, 2.4f, false, MarkerType.VerticalCylinder, false);

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Drücke E um auf den Exporthändler zuzugreifen!",
                    Color = "yellow",
                    Duration = 3500,
                    Title = "Exporthändler (" + export.Name + ")"
                };

                mcb.ColShape.Action = async player => await OpenExport(player, export);
            }

            foreach (var export in await db.ItemExportItems.ToListAsync())
            {
                Random random = new Random();

                int price = random.Next(1000, 2500);

                export.Price = export.Price - (uint)price;
                ItemExports.Find(x => x.Id == export.ExportId).items.Add(export);
                ItemExportItems.Add(export);
            }

        }

        public async Task OpenExport(RXPlayer player, DbItemExport export)
        {
            if (player == null) return;

            if (await player.GetIsInVehicleAsync()) return;


            var window = new RXWindow("Seller");

            object exportseller = new
            {
                s = export.Id,
                n = export.Name,
                d = export.items
            };

            await window.OpenWindow(player, exportseller);

        }

        [RemoteEvent]
        public async Task SellAllItems(RXPlayer player, uint exportId)
        {
            var export = ItemExports.Find(x => x.Id == exportId);

            if (export == null) return;

            foreach (var exportitem in export.items)
            {
                if (exportitem == null) continue;

                var rxitem = ItemModelModule.ItemModels.Find(x => x.Name == exportitem.Name);

                if (rxitem == null) continue;

                var itemcount = player.Container.GetItemAmount(rxitem);

                if (itemcount < 1)
                {
                    continue;
                }

                player.Container.RemoveItem(rxitem, itemcount);

                uint fullprice = exportitem.Price * (uint)itemcount;

                await player.GiveMoney((int)fullprice);
                await player.SendNotify("Du hast erfolgreich " + itemcount + "x " + exportitem.Name + " verkauft und verdienst dir " + fullprice + "$");
            }
        }

        [RemoteEvent]
        public async Task SellItems(RXPlayer player, uint id)
        {
            var exportitem = ItemExportItems.Find(x => x.Id == id);

            if (exportitem == null) return;

            var rxitem = ItemModelModule.ItemModels.Find(x => x.Name == exportitem.Name);

            if (rxitem == null) return;

            var itemcount = player.Container.GetItemAmount(rxitem);

            if (itemcount < 1)
            {
                await player.SendNotify("Du hast nicht genug dabei!");
                return;
            }

            player.Container.RemoveItem(rxitem, itemcount);

            uint fullprice = exportitem.Price * (uint)itemcount;

            await player.GiveMoney((int)fullprice);
            await player.SendNotify("Du hast erfolgreich " + itemcount + "x " + exportitem.Name + " verkauft und verdienst dir " + fullprice + "$");
        }

        [RemoteEvent]
        public async Task RqWareExportPrices(RXPlayer player)
        {
            if (player == null) return;

            await player.TriggerEventAsync("RsWareExportPrices", NAPI.Util.ToJson(ItemExportItems), 1);
        }
        [RemoteEvent]
        public async Task findExport(RXPlayer player, int id)
        {
            if (player == null) return;

            var exporti = ItemExportItems.Find(x => x.Id == id);

            if (exporti == null) return;

            var export = ItemExports.Find(x => x.Id == exporti.ExportId);

            if (export == null) return;


            await player.TriggerEventAsync("setPlayerGpsMarker", export.Position.ToPos().X, export.Position.ToPos().Y);

            await player.SendNotify("Der Exporthändler wurde auf ihrer interaktiven Karte markiert!");

        }
    }
}
