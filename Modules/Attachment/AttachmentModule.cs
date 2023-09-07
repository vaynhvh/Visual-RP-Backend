using Backend.Controllers;
using Backend.Models;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Attachment
{
    public enum Attachment
    {
        BOX = 1,
        BEER = 2,
        TRASH = 3,
        FISHINGROD = 4,
        HANDY = 5,
        DRILL = 6,
        CIGARRETES = 7,
        CIGAR = 8,
        JOINT = 9,
        WELDING = 10,
        GUITAR = 11,
        DRINKBOTTLE = 12,
        BONGOS = 13,
        DRINKCAN = 14,
        COMBATSHIELD = 21,
        TABLET = 23,
        KLAPPSTUHL = 27,
        MEDICBAG = 49,
        KLAPPSTUHLBLAU = 57,
    }
    class AttachmentModule : RXModule
    {
        public AttachmentModule() : base("Attachment") { }

        public static List<DbAttachment> AttachmentItems = new List<DbAttachment>();

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            AttachmentItems = await db.Attachments.ToListAsync();

            foreach (var item in AttachmentItems)
            {
                item.Position = item.RawPosition.ToPos();
                item.Rotation = item.RawRotation.ToPos();
            }

        }

        public override async Task OnTenSecond()
        {
            using var db = new RXContext();

            AttachmentItems = await db.Attachments.ToListAsync();

            foreach (var item in AttachmentItems)
            {
                item.Position = item.RawPosition.ToPos();
                item.Rotation = item.RawRotation.ToPos();
            }
        }

        public static float Attachmentrange = 30;

        // Handle attachment
        public static async Task HandleAttachment(RXPlayer player, int uid, bool remove)
        {
            if (player == null) return;

            if (AttachmentItems.Find(x => x.Id == uid) == null) return;

            if (player.Attachments.Count == 0)
            {
                if (!remove)
                {
                    player.Attachments.TryAdd(uid, AttachmentItems.Find(x => x.Id == uid));
                }
            }
            else
            {
                if (remove)
                {
                    if (player.Attachments.ContainsKey(uid))
                    {
                        player.Attachments.Remove(uid);
                    }
                }
                else
                {
                    player.Attachments.TryAdd(uid, AttachmentItems.Find(x => x.Id == uid));
                }
            }


            var players = await PlayerController.GetPlayersInRange(await player.GetPositionAsync(), Attachmentrange);

            foreach(var hund in players)
            {
                await hund.TriggerEventAsync("setAttachments", player, SerializeAttachments(player));
            }
            
        }

        public async Task ClearAllAttachments(RXPlayer player)
        {
            if (player == null) return;

            if (player.Attachments.Count > 0)
            {
                player.Attachments.Clear();
                var players = await PlayerController.GetPlayersInRange(await player.GetPositionAsync(), Attachmentrange);

                foreach (var hund in players)
                {
                    await hund.TriggerEventAsync("setAttachments", player, SerializeAttachments(player));
                }
            }
        }

        // Add attachment
        public static async Task AddAttachment(RXPlayer player, int type, bool removeAllOthers = false)
        {
            if (player == null) return;

            if (player.Attachments.ContainsKey(type)) return; // bereits vorhanden

            if (removeAllOthers) await RemoveAllAttachments(player);

            await HandleAttachment(player, type, false);
        }
   
        public async Task RemoveAttachment(RXPlayer player, int type)
        {
            if (player == null) return;
            await HandleAttachment(player, type, true);
        }

        public static async Task RemoveAllAttachments(RXPlayer player)
        {
            if (player == null) return;

            if (player.Attachments.Count > 0)
            {
                player.Attachments.Clear();
            }
            var players = await PlayerController.GetPlayersInRange(await player.GetPositionAsync(), Attachmentrange);

            foreach (var hund in players)
            {
                await hund.TriggerEventAsync("removeAllAttachments", player, SerializeAttachments(player));
            }
        }

        public async Task ResyncAllAttachments(RXPlayer player)
        {
            if (player == null) return;

            if (player.Attachments.Count > 0)
            {
                var players = await PlayerController.GetPlayersInRange(await player.GetPositionAsync(), Attachmentrange);

                foreach (var hund in players)
                {
                    await hund.TriggerEventAsync("resyncAttachments", player);
                }
            }
        }
        public static string SerializeAttachments(RXPlayer player)
        {
            if (player == null) return "";

            return NAPI.Util.ToJson(player.Attachments.Values.ToList());
        }

        public override async Task OnPlayerEnterVehicle(RXPlayer player, RXVehicle vehicle, sbyte seat)
        {
            await RemoveAllAttachments(player);
        }

        public override async Task OnPlayerExitVehicle(RXPlayer player, RXVehicle vehicle)
        {
           
                await player.SyncAttachmentOnlyItems(player);
        }
    }

    public class AttachmentsSync : Script
    {
        [RemoteEvent]
        public async Task requestAttachmentsPlayer(RXPlayer player, RXPlayer destinationPlayer)
        {

            if (destinationPlayer == null) return;
            if (destinationPlayer == null || await destinationPlayer.GetIsInVehicleAsync()) return;


            if (destinationPlayer.Attachments.Count > 0)
            {


                await player.TriggerEventAsync("setAttachments", destinationPlayer, AttachmentModule.SerializeAttachments(player));
            }
            return;
        }
    }

    
}

