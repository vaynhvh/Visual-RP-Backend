using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Attachment;
using Backend.Modules.Inventory;
using Backend.MySql;
using Backend.Utils;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone
{
    public class PhoneApp
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        public PhoneApp(string id, string name, string icon)
        {
            Id = id;
            Name = name;
            Icon = icon;
        }
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class PhoneModule : RXModule
    {
        public PhoneModule() : base("Phone", new RXWindow("Phone")) { }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Keks(RXPlayer player, bool state)
        {
            if (!player.IsLoggedIn || player.DeathData.IsDead || player.IsCuffed || player.IsTied || !await player.CanInteractAntiFloodNoMSG(0.5)) return;

            var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Smartphone");
            if (model == null) return;

            if (player.Container.GetItemAmount(model) < 1) return;


            if (state)
            {
                await this.Window.OpenWindow(player);

            } else
            {
                await this.Window.CloseWindow(player);
            }

            if (state && !await player.GetIsInVehicleAsync()) { 
                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_stand_mobile@male@text@base", "base");
                await AttachmentModule.AddAttachment(player, (int)Attachment.Attachment.HANDY);                
            }

            if (!state && !await player.GetIsInVehicleAsync()) { 
                await player.StopAnimationAsync();
                await AttachmentModule.RemoveAllAttachments(player);
            }

        }

        public static async Task<uint> generateRandomPhonenumber(RXPlayer player)
        {
            using var db = new RXContext();

            var random = new Random();

            uint number = (uint)random.Next(10000, 99999);

            foreach (var tplayer in await db.Players.ToListAsync())
            {
                if (tplayer.Phone == number)
                { 
                    await generateRandomPhonenumber(player);
                    return 0;
                }
            }
            return number;
        }

        //[HandleExceptions]
        [RemoteEvent]
        public static async Task requestApps(RXPlayer player)
        {
            if (!player.IsLoggedIn) return;

            await Task.Delay(100);

            var homeApp = new RXWindow("HomeApp");

            List<PhoneApp> phoneApps = new List<PhoneApp>();

            if (player.TeamId > 0 && player.Team != null)
            {
                phoneApps.Add(new PhoneApp("TeamApp", "Team", "apps/TeamApp.png"));
            }
            if (player.Team.Type == Faction.TeamType.LSPD)
            {
            }
            phoneApps.Add(new PhoneApp("FunkApp", "Funkgerät", "apps/FunkApp.png"));
            phoneApps.Add(new PhoneApp("GpsApp", "GPS", "apps/GpsApp.png"));
            phoneApps.Add(new PhoneApp("ContactsApp", "Kontakte", "apps/ContactsApp.png"));
            phoneApps.Add(new PhoneApp("LifeInvaderApp", "Lifeinvader", "apps/LifeinvaderApp.png"));
            //phoneApps.Add(new PhoneApp("NewsApp", "News", "NewsApp.png"));
            phoneApps.Add(new PhoneApp("TelefonApp", "Telefon", "apps/TelefonApp.png"));
            phoneApps.Add(new PhoneApp("ProfileApp", "Profil", "apps/ProfilApp.png"));
            phoneApps.Add(new PhoneApp("MessengerApp", "SMS", "apps/MessengerApp.png"));
            phoneApps.Add(new PhoneApp("SettingsApp", "Settings", "apps/SettingsApp.png"));
            phoneApps.Add(new PhoneApp("CalculatorApp", "Rechner", "apps/CalculatorApp.png"));
            phoneApps.Add(new PhoneApp("ServiceRequestApp", "Service", "apps/ServiceApp.png"));
            phoneApps.Add(new PhoneApp("BankingApp", "Banking", "apps/BankingApp.png"));
            phoneApps.Add(new PhoneApp("WorkstationApp", "Workstation", "apps/BusinessApp.png"));

            //Business App

            await homeApp.TriggerEvent(player, "responseApps", JsonConvert.SerializeObject(phoneApps));
        }
    }
}
