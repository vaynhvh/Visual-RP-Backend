using Backend.Models;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    class SettingsApp : RXModule
    {
        public SettingsApp() : base("SettingsApp", new RXWindow("Phone")) { }

        public static List<DbPhoneSettings> PhoneSettings = new List<DbPhoneSettings>();

        //[HandleExceptions]
        public override async Task OnTwoSecond()
        {
            using var db = new RXContext();

            List<DbPhoneSettings> copyPhoneSettings = new List<DbPhoneSettings>();

            TransferDBContextValues(await db.PhoneSettings.ToListAsync(), phoneSettings => copyPhoneSettings.Add(phoneSettings));

            PhoneSettings = copyPhoneSettings;
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task GetPhoneSettings(RXPlayer player)
        {
            var settings = player.PhoneSettings;
            if (settings == null) return;

            await player.TriggerEventAsync("SendPhoneSettings", settings.Mute, settings.DenyCalls, uint.Parse(settings.Ringtone), settings.RingtoneVolume);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task savePhoneSettings(RXPlayer player, bool flyMode, bool mute, bool denyCalls, bool injuryStatus)
        {
            if (!player.CanInteract()) return;

            player.PhoneSettings.FlyMode = flyMode;
            player.PhoneSettings.Mute = mute;
            player.PhoneSettings.DenyCalls = denyCalls;
            player.PhoneSettings.InjuryStatus = injuryStatus;

            using var db = new RXContext();

            var settings = await db.PhoneSettings.FirstOrDefaultAsync(x => x.PlayerId == player.Id);
            if (settings == null) return;

            settings.FlyMode = flyMode;
            settings.Mute = mute;
            settings.DenyCalls = denyCalls;
            settings.InjuryStatus = injuryStatus;

            await db.SaveChangesAsync();

            await player.TriggerEventAsync("SendPhoneSettings", settings.Mute, settings.DenyCalls, uint.Parse(settings.Ringtone), settings.RingtoneVolume);

        }

        [RemoteEvent]
        public async Task ChangeSmsMute(RXPlayer player, bool state)
        {
            if (!player.CanInteract()) return;

            player.PhoneSettings.Mute = state;

            using var db = new RXContext();

            var settings = await db.PhoneSettings.FirstOrDefaultAsync(x => x.PlayerId == player.Id);
            if (settings == null) return;

            settings.Mute = state;

            await db.SaveChangesAsync();

            await player.TriggerEventAsync("SendPhoneSettings", settings.Mute, settings.DenyCalls, uint.Parse(settings.Ringtone), settings.RingtoneVolume);

        }

        [RemoteEvent]
        public async Task ChangeBeepMute(RXPlayer player, bool state)
        {
            if (!player.CanInteract()) return;

            player.PhoneSettings.InjuryStatus = state;

            using var db = new RXContext();

            var settings = await db.PhoneSettings.FirstOrDefaultAsync(x => x.PlayerId == player.Id);
            if (settings == null) return;

            settings.InjuryStatus = state;

            await db.SaveChangesAsync();

            await player.TriggerEventAsync("SendPhoneSettings", settings.Mute, settings.DenyCalls, uint.Parse(settings.Ringtone), settings.RingtoneVolume);

        }



        [RemoteEvent]
        public async Task ChangeCallMute(RXPlayer player, bool state)
        {
            if (!player.CanInteract()) return;

            player.PhoneSettings.DenyCalls = state;

            using var db = new RXContext();

            var settings = await db.PhoneSettings.FirstOrDefaultAsync(x => x.PlayerId == player.Id);
            if (settings == null) return;

            settings.DenyCalls = state;

            await db.SaveChangesAsync();

            await player.TriggerEventAsync("SendPhoneSettings", settings.Mute, settings.DenyCalls, uint.Parse(settings.Ringtone), settings.RingtoneVolume);

        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ChangeRingtone(RXPlayer player, string ringtoneId)
        {
            if (!player.CanInteract()) return;

            player.PhoneSettings.Ringtone = ringtoneId;

            using var db = new RXContext();

            var settings = await db.PhoneSettings.FirstOrDefaultAsync(x => x.PlayerId == player.Id);
            if (settings == null) return;

            settings.Ringtone = ringtoneId;

            await db.SaveChangesAsync();
            await player.TriggerEventAsync("UpdateRingtone", player.PhoneSettings.Ringtone);

            await player.TriggerEventAsync("SendPhoneSettings", settings.Mute, settings.DenyCalls, uint.Parse(settings.Ringtone), settings.RingtoneVolume);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ChangeRingtoneVolume(RXPlayer player, uint volume)
        {
            if (!player.CanInteract()) return;

            player.PhoneSettings.RingtoneVolume = volume;

            using var db = new RXContext();

            var settings = await db.PhoneSettings.FirstOrDefaultAsync(x => x.PlayerId == player.Id);
            if (settings == null) return;

            settings.RingtoneVolume = volume;

            await db.SaveChangesAsync();
            await player.TriggerEventAsync("SendPhoneSettings", settings.Mute, settings.DenyCalls, settings.Ringtone, settings.RingtoneVolume);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task SendWallpapers(RXPlayer player)
        {
            using var db = new RXContext();
            var Wallpapers = await db.PhoneWallpaper.ToListAsync();
            var wallpaperList = new List<DbPhoneWallpaper>();
            foreach (var wallpaper in Wallpapers)
            {
                if (wallpaper.RestrictedForStaff && player.Rank != null && player.Rank.Permission > 90)
                {
                    wallpaperList.Add(wallpaper);
                }
                if (wallpaper.RestrictedPlayer != 0 && wallpaper.RestrictedPlayer == player.Id)
                {
                    wallpaperList.Add(wallpaper);
                }
                if (wallpaper.RestrictedTeam != 0 && wallpaper.RestrictedTeam == player.Team.Id)
                {
                    wallpaperList.Add(wallpaper);
                }
                if (wallpaper.RestrictedTeam == 0 && wallpaper.RestrictedPlayer == 0 && !wallpaper.RestrictedForStaff)
                {
                    wallpaperList.Add(wallpaper);
                }
            }

            await player.TriggerEventAsync("SendWallpapers", NAPI.Util.ToJson(wallpaperList));
        }
        [RemoteEvent]
        public async Task UpdateWallpaper(RXPlayer player)
        {

            using var db = new RXContext();
            await player.TriggerEventAsync("UpdateWallpaper", player.PhoneSettings.Wallpaper);
        }


        //[HandleExceptions]
        [RemoteEvent]
        public async Task ChangeWallpaperId(RXPlayer player, string wallpaperId)
        {
            if (!player.CanInteract()) return;

            if (player.PhoneSettings == null) return;

            player.PhoneSettings.Wallpaper = wallpaperId;

            using var db = new RXContext();

            var settings = await db.PhoneSettings.FirstOrDefaultAsync(x => x.PlayerId == player.Id);
            if (settings == null) return;

            settings.Wallpaper = wallpaperId;
            await player.TriggerEventAsync("UpdateWallpaper", player.PhoneSettings.Wallpaper);

            await db.SaveChangesAsync();
        }
    }
}
