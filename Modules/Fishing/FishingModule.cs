using Backend.Models;
using Backend.Modules.Attachment;
using Backend.Modules.Inventory;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Fishing
{
    class FishingModule : RXModule
    {
        public FishingModule() : base("Fishing") { }

        public static List<DbFishing> fishingspots = new List<DbFishing>();

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            fishingspots = await db.Fishing.ToListAsync();

            foreach (var fishing in fishingspots)
            {
                await LoadFishingSpot(fishing);
            }
        }

        public static async Task LoadFishingSpot(DbFishing fishing)
        {
            if (fishing == null) return;
            var mcb = await NAPI.Entity.CreateMCB(fishing.Position.ToPos().Add(new Vector3(0, 0, 1)), new Color(0, 238, 255, 180), 0u, 1.4f, 1.4f, true, (MarkerType)35, false);

            mcb.ColShape.Action = async player => await StartFishing(player, fishing);

            fishing.MCB = mcb;
        }

        public static async Task StartFishing(RXPlayer player, DbFishing fishing)
        {
            if (fishing == null) return;

            if (fishing.InUse)
            {
                await player.SendNotify("Dieser Fishingspot ist bereits besetzt!");
                return;
            }

            fishing.InUse = true;
            fishing.Player = player.Id;

            NAPI.Task.Run(() => fishing.MCB.Marker.Scale = 0f);

         

            await player.SetPositionAsync(fishing.Position.ToPos());
            await player.SetHeadingAsync(fishing.Heading);
            await player.PlayAnimationAsync(1, "amb@world_human_stand_fishing@idle_a", "idle_b");

            await AttachmentModule.AddAttachment(player, 666, true);

            var window = new RXWindow("Fishing");

            await window.OpenWindow(player);
        }

        [RemoteEvent]
        public async Task CatchFish(RXPlayer player)
        {
            var fishing = fishingspots.Find(x => x.Player == player.Id);
            if (fishing == null) return;
            if (fishing.LastCatch.AddSeconds(5) > DateTime.Now)
            {
                await player.BanPlayer("Cheating [FISHING]");
                return;
            }

            fishing.LastCatch = DateTime.Now;

            int fish = new Random().Next(1, 4);
            RXItemModel itemModel;

            switch (fish)
            {
                case 1:
                    itemModel = ItemModelModule.ItemModels.Find(x => x.Id == 110);
                    break;
                case 2:
                    itemModel = ItemModelModule.ItemModels.Find(x => x.Id == 109); 
                    break;
                case 3:
                    itemModel = ItemModelModule.ItemModels.Find(x => x.Id == 108);
                    break;
                case 4:
                    itemModel = null;
                    break;
                default:
                    itemModel = ItemModelModule.ItemModels.Find(x => x.Id == 108);
                    break;
            }

            if (itemModel == null)
            {
                await player.SendNotify("Der Fisch ist entkommen!");
                return;
            }

            if (!player.Container.CanInventoryItemAdded(itemModel, 1)) {
                await player.SendNotify("Dein Inventar ist voll!");
                return;
            }
            player.Container.AddItem(itemModel, 1);
            await player.SendNotify("Du hast einen " + itemModel.Name + " geangelt!");

        }

        [RemoteEvent]
        public async Task StopFishing(RXPlayer player)
        {
            var fishing = fishingspots.Find(x => x.Player == player.Id);   
            if (fishing == null) return;

            fishing.InUse = false;
            fishing.Player = 0;

            await AttachmentModule.RemoveAllAttachments(player);
            await player.StopAnimationAsync();

            await player.SendNotify("Du angelst nun nicht mehr!");

            NAPI.Task.Run(() => fishing.MCB.Marker.Scale = 1.4f);
        }
    }
}
