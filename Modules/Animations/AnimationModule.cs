using Backend.Models;
using Backend.Modules.Gangwar;
using Backend.Modules.Native;
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

namespace Backend.Modules.Animations
{
    public class AnimationShortcutJson
    {
        [JsonProperty(PropertyName = "slot")]
        public int Slot { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "server")]
        public bool isServer { get; set; }

        [JsonProperty(PropertyName = "anim")]
        public int animId { get; set; }

        [JsonProperty(PropertyName = "args")]
        public string Args { get; set; }

        [JsonProperty(PropertyName = "event")]
        public string Event { get; set; }
    }

    class AnimationModule : RXModule
    {
        public AnimationModule() : base("Animation", new RXWindow("AnimMenu")) { }

        public static List<DbAnimationCategory> AnimationCategories = new List<DbAnimationCategory>();
        public static List<DbAnimationItem> AnimationItems = new List<DbAnimationItem>();

        public static Dictionary<uint, int> animFlagDic = new Dictionary<uint, int>();

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            AnimationCategories = await db.AnimationCategories.ToListAsync();
            AnimationItems      = await db.AnimationItems.ToListAsync();

            animFlagDic.Add(1, (int)AnimationFlags.Loop);
            animFlagDic.Add(2, (int)AnimationFlags.StopOnLastFrame);
            animFlagDic.Add(3, (int)AnimationFlags.OnlyAnimateUpperBody);
            animFlagDic.Add(4, (int)AnimationFlags.AllowPlayerControl);
            animFlagDic.Add(5, (int)AnimationFlags.Cancellable);
            animFlagDic.Add(6, (int)(AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl));
            animFlagDic.Add(7, (int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop));
            animFlagDic.Add(8, (int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody));
            animFlagDic.Add(9, (int)(AnimationFlags.AllowPlayerControl | AnimationFlags.StopOnLastFrame | AnimationFlags.Loop));
            animFlagDic.Add(10, (int)(AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.StopOnLastFrame | AnimationFlags.Loop));
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task openAnimationMenu(RXPlayer player)
        {

            if (GangwarModule.IsPlayerInGangwar(player)) return;

            if (!player.CanInteract()) return;


            await this.Window.OpenWindow(player);
        }
        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task PlayAnim(RXPlayer player, string dict, string name, uint flag)
        {
            if (!player.CanInteract() || await player.GetIsInVehicleAsync() || player.Freezed) return;

            await player.PlayAnimationAsync(animFlagDic[flag], dict, name);
                    
                
            
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task PlayAnimFromNMenu(RXPlayer player, string slotStr)
        {
            RXLogger.Print(slotStr);

            if (!player.CanInteract() ||   await player.GetIsInVehicleAsync() || player.Freezed) return;

            uint slot = uint.Parse(slotStr);


            if (slot == 0)
            {
                await player.StopAnimationAsync();
                player.Freezed = false;
            }
            else
            {
                if (player.AnimationShortcuts.ContainsKey(slot) && player.AnimationShortcuts[slot] != null)
                {
                    if (AnimationItems.FirstOrDefault(x => x.Id == player.AnimationShortcuts[slot]) != null)
                    {
                        DbAnimationItem animationItem = AnimationItems.FirstOrDefault(x => x.Id == player.AnimationShortcuts[slot]);
                        if (animationItem == null) return;

                        await player.StopAnimationAsync();
                        await Task.Delay(500);

                        if (!animFlagDic.ContainsKey((uint)animationItem.Flag) || animFlagDic[(uint)animationItem.Flag] == null) return;

                        await player.PlayAnimationAsync(animFlagDic[(uint)animationItem.Flag], animationItem.Dict, animationItem.Name);
                    }
                }
            }
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task SaveAnim(RXPlayer player, uint slot, string name)
        {
            if (!player.CanInteract() || await player.GetIsInVehicleAsync()) return;

  

            if (AnimationItems.FirstOrDefault(x => x.Text == name) == null) return;
            if (!player.AnimationShortcuts.ContainsKey(slot)) return;

            DbAnimationItem animationItem = AnimationItems.FirstOrDefault(x => x.Text == name);
            if (animationItem == null) return;

            player.AnimationShortcuts[slot] = animationItem.Id;

            await player.SendNotify("Animationsslot " + slot + " mit " + animationItem.Text + " belegt!");

            await player.SaveAnimationShortcuts();
            await player.UpdateAnimationShortcuts();
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task requestPlayerSyncData(RXPlayer player, RXPlayer requestedPlayer)
        {
            if (!player.IsLoggedIn || !requestedPlayer.IsLoggedIn) return;

            bool HasAnimation = requestedPlayer.PlayingAnimation;
            string CurrentAnimation = requestedPlayer.AnimationName;
            int AnimationFlags = requestedPlayer.CurrentAnimFlags;
            string CurrentAnimationDict = requestedPlayer.AnimationDict;
            float AnimationSpeed = requestedPlayer.AnimationSpeed;

            AnimationSyncItem animationSyncItem = new AnimationSyncItem(HasAnimation, CurrentAnimationDict, CurrentAnimation, AnimationFlags, AnimationSpeed, await NAPI.Task.RunReturnAsync(() => requestedPlayer.Heading));

            await player.TriggerEventAsync("responsePlayerSyncData", requestedPlayer, false, JsonConvert.SerializeObject(animationSyncItem), requestedPlayer.IsCrouched);
        }
    }
}
