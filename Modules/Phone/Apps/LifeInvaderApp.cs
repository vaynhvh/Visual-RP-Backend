using Backend.Models;
using Backend.Modules.Inventory;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    public class AdsFound
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; }

        [JsonProperty(PropertyName = "n")]
        public uint Title { get; }

        [JsonProperty(PropertyName = "m")]
        public string Content { get; }

        [JsonProperty(PropertyName = "d")]
        public string Date { get; }

        public AdsFound(uint id, uint title, string content)
        {
            Id = id;
            Title = title;
            Content = content;
            Date = DateTime.Now.ToLocalTime().ToString("dd\\/MM\\/yyyy h\\:mm");
        }

        public AdsFound(uint id, uint title, string content, string dateTime)
        {
            Id = id;
            Title = title;
            Content = content;
            Date = dateTime;
        }
    }

    class LifeInvaderApp : RXModule
    {
        public LifeInvaderApp() : base("LifeInvaderApp", new RXWindow("LifeInvader")) { }

        public static List<AdsFound> Ads = new List<AdsFound>();

        public override async void LoadAsync()
        {
            var mcb = await NAPI.Entity.CreateMCB(new Vector3(-1082.2278, -247.62811, 37.763256), new Color(255, 140, 0), 0, 2.4f, 2.4f, false, MarkerType.VerticalCylinder);

            mcb.ColShape.Message = new RXMessage
            {
                Text = "Benutze E um eine Werbung zu schalten!",
                Duration = 3500,
                Title = "Life Invader",
                Color = "yellow"
            };

            mcb.ColShape.Action = async player => await OpenLifeInvader(player);
        }

        //[HandleExceptions]
        public async Task OpenLifeInvader(RXPlayer player)
        {
            if (!player.CanInteract()) return;


            await this.Window.OpenWindow(player);

            await player.TriggerEventAsync("LifeInvaderTime", 10);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task GetLifeInvaderMessages(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            await player.TriggerEventAsync("SendLifeInvaderMessages", JsonConvert.SerializeObject(Ads.ToList()));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task SendLifeInvaderMessage(RXPlayer player, string ad, uint number)
        {
            if (!player.CanInteract()) return;

            if (ad.ToLower().Contains("discord") || ad.ToLower().Contains("twitch") || ad.ToLower().Contains("youtube") || ad.ToLower().Contains("dc.gg") || ad.ToLower().Contains(".com") || ad.ToLower().Contains(".gg") || ad.ToLower().Contains(".de") || ad.ToLower().Contains("http")) return;

            if (!await player.CanInteractAntiFloodNoMSG(10))
            {
                await player.SendNotify("Du kannst aktuell keine Werbung schalten, bitte warte kurz!");
                return;
            }

            int newsprice = 0;

            if (ad.Length < 10 || ad.Length > 96)
            {
                await player.SendNotify("Werbungen müssen zwischen 10 und 96 Zeichen lang sein!");
                return;
            }

            newsprice = ((int)(ad.Length * 3)) + 4;

            if (!await player.BankAccount.TakeBankMoney(newsprice, "Werbung - Life Invader"))
            {
                await player.SendNotify("Dein Bankkonto ist nicht ausreichend gedeckt!", 3500, "red", "Zahlung fehlgeschlagen");
                return;
            }

            ad = ad.Replace("\"", "");

            Ads.Add(new AdsFound(player.Id, number, ad, $"{DateTime.Now:dd.MM.yyyy HH:mm}"));

            await player.SendNotify("Werbung abgesendet! Kosten: 3$ / Buchstabe (insgesamt: " + newsprice.FormatMoneyNumber() + ")");

            var phoneItem = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Smartphone");
            if (phoneItem == null) return;

            RX.SendNotifyToAllWhich(p => p.IsLoggedIn && p.Container != null && p.PhoneSettings != null && p.Container.GetItemAmount(phoneItem) > 0 && !p.PhoneSettings.FlyMode, "Eine neue Anzeige wurde veröffentlicht.", 5000, "Darkred", "Lifeinvader");
        }
    }
}
