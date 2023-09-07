using Backend.Models;
using Backend.Modules.Inventory;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.DailyWheel
{
    class DailyWheelModule : RXModule
    {
        public DailyWheelModule() : base("DailyWheel") { }

        public override async void LoadAsync()
        {

            await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("ch_prop_casino_diamonds_01a"), new Vector3(190.36768, -925.03705, 30.10681), new Vector3(0, 0, -163.92853 - 180f)));
            await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("ch_prop_casino_diamonds_01b"), new Vector3(195.94455, -925.0585, 30.301998), new Vector3(0, 0, -148.56 - 180f)));
            await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("ch_prop_casino_lucky_wheel_01a"), new Vector3(193.46007, -925.7948, 29.686811), new Vector3(0, 0, -172.80 - 180f)));

            var mcb = await NAPI.Entity.CreateMCB(new Vector3(193.5735, -926.8244, 30.68681), new Color(255, 140, 0), 0u, 2f, 2.4f, false, MarkerType.VerticalCylinder);
           
            mcb.ColShape.Message = new RXMessage
            {
                Text = "Drücke E um am Lucky-Wheel zu drehen!",
                Color = "white",
                Duration = 3500,
                Title = "Glücksrad"
            };

            mcb.ColShape.Action = async player => await LuckyWheel(player);

        }

        public async Task<string> getRandomGeschenk(RXPlayer player)
        {
            Random random = new Random();
            int geschenk = random.Next(1, 10);

            if (geschenk == 1)
            {
                player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Casino-Freispiele"), 30);
                return "30 Freispiele";
            }
            else if (geschenk == 2)
            {
                player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Rubbellos"), 5);
                return "5 Rubbellose";
            }
            else if (geschenk == 3)
            {
                player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Rubbellos"), 10);
                return "10 Rubbellose";
            }
            else if (geschenk == 4)
            {
                await player.GiveMoney(15000);
                return "Wertanlage 15.000$";
            }
            else if (geschenk == 5)
            {
                player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Verbandskasten"), 10);
                return "10 Verbandskästen";
            }
            else if (geschenk == 6)
            {
                player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Schutzweste"), 5);
                return "5 Schutzwesten";
            }
            else if (geschenk == 7)
            {
                player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Orangensaft"), 15);
                return "Was fürn Saft? ORANGENSAFT!";
            }
            else if (geschenk == 8)
            {
                await player.GiveMoney(30000);
                return "Wertanlage 30.000$";
            }
            else if (geschenk == 9)
            {
                player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Namechange Gutschein"), 1);
                return "Namechange Gutschein";
            } else
            {
                player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Joint"), 5);
                return "5x Joints";
            }
        }

        public async Task LuckyWheel(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.Freezed) return;

            if (player.LuckyWheel.AddDays(1) > DateTime.Now)
            {
                await player.SendNotify("Du hast bereits einmal am Glücksrad gedreht!", 3500, "red");
                return;
            }

            await player.SetPositionAsync(new Vector3(193.5735, -926.8244, 30.68681));
            await player.SetHeadingAsync(6.34f);
            player.Freezed = true;
            await player.disableAllPlayerActions(true);
            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim_casino_a@amb@casino@games@lucky7wheel@male", "enter_to_armraisedidle");
            await Task.Delay(3000);
            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim_casino_a@amb@casino@games@lucky7wheel@male", "armraisedidle_to_spinningidle_high");
            await Task.Delay(3000);
            await player.StopAnimationAsync();
            player.Freezed = false;
            await player.disableAllPlayerActions(false);
            player.LuckyWheel = DateTime.Now;
            await player.SendNotify("Du hast am Glücksrad gedreht und erhälst " + await getRandomGeschenk(player) + "! Viel Spaß damit! Versuch es gerne morgen wieder!", 5000, "white", "Glücksrad");
        }
    }
}
