using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Inventory;
using Backend.Modules.Voice;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    public class VoiceSettings
    {
        public double Room { get; set; }
        public int Active { get; set; }

        public VoiceSettings(double room, int active)
        {
            Room = room;
            Active = active;
        }
    }

    class FunkApp : RXModule
    {
        public FunkApp() : base("FunkApp", new RXWindow("FunkApp")) { }

        public static Dictionary<double, List<RXPlayer>> voiceFQ;
        public static Dictionary<double, string> voiceFQDataStrings;

        public override void LoadAsync()
        {
            voiceFQ = new Dictionary<double, List<RXPlayer>>();
            voiceFQDataStrings = new Dictionary<double, string>();
        }

        //[HandleExceptions]
        public async Task sendSoundToFrequenz(double frequenz, string sound1, string sound2)
        {

        }

        [RemoteEvent]
        public async Task AddFunkSettings(RXPlayer player, uint fav)
        {
            if (player.FunkFav.Contains(fav))
            {
                await player.SendNotify("Dieser Funk ist bereits gespeichert!");
                return;
            }
            player.FunkFav.Add(fav);

            await player.TriggerEventAsync("SendFunkSettings", NAPI.Util.ToJson(player.FunkFav));
        }

        [RemoteEvent]
        public async Task RemoveFunkSettings(RXPlayer player, uint fav)
        {
            if (player.FunkFav.Contains(fav))
            {
                player.FunkFav.Remove(fav);
            }

            await player.TriggerEventAsync("SendFunkSettings", NAPI.Util.ToJson(player.FunkFav));
        }

        [RemoteEvent]
        public async Task GetFunkData(RXPlayer player)
        {

            double funk = await player.GetFrequencyAsync();
            string funkstr = funk.ToString();

            if (funk == 0)
            {
                funkstr = "000";
            }
            await player.TriggerEventAsync("SendFunkData", funkstr.Substring(0, funkstr.Length - 2), (int)await player.GetFunkStatusAsync());
        }

        [RemoteEvent]
        public async Task GetFunkSettings(RXPlayer player)
        {
            await player.TriggerEventAsync("SendFunkSettings", NAPI.Util.ToJson(player.FunkFav));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task openFunkThing(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.DeathData.IsDead || player.IsCuffed || player.IsTied || !await player.CanInteractAntiFloodNoMSG(0.5)) return;

            var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Funkgerät");
            if (model == null) return;

            if (player.Container.GetItemAmount(model) < 1) return;

            await player.TriggerEventAsync("openFunk");

        }
        //[HandleExceptions]
        [RemoteEvent]
        public async Task JoinFunk(RXPlayer player, int frequency)
        {
            if (!player.CanInteract()) return;


            await VoiceModule.ChangeFrequency(player, frequency);
        }
        public static async Task refreshFQVoiceForPlayerFrequenz(RXPlayer dbPlayer)
        {
            if (dbPlayer == null)
                return;


                double frequenz = dbPlayer.Frequency;
                await refreshFQVoiceForFrequenz(frequenz);
       
        }



        public static async Task refreshFQVoiceForFrequenz(double frequenz)
        {
            await actualizeFrequenzDataString(frequenz);
            foreach (RXPlayer xx in voiceFQ[frequenz].ToList())
            {
                if (xx.Frequency == frequenz)
                {
                    string frequenzString = voiceFQDataStrings[frequenz];
                    await xx.TriggerEventAsync("setRadioChatPlayers", frequenzString);
                }
            }
        }
        public static void CheckFrequenz(double frequenz)
        {
            if (!voiceFQ.ContainsKey(frequenz)) voiceFQ.Add(frequenz, new List<RXPlayer>());
            if (!voiceFQDataStrings.ContainsKey(frequenz)) voiceFQDataStrings.Add(frequenz, "");
        }

        public static async Task actualizeFrequenzDataString(double frequenz)
        {
            try
            {
                if (frequenz < 1)
                {
                    voiceFQDataStrings[frequenz] = "";
                    return;
                }
                string s = "";
                foreach (RXPlayer xx in voiceFQ[frequenz].ToList().Where(p => p != null))
                {
                    if (await xx.GetFunkStatusAsync() == FunkStatus.Active)
                    {

                        s += ";" + xx.VoiceHash + "~-6~0~0~2";
                    }
                }

                if (!voiceFQDataStrings.ContainsKey(frequenz)) voiceFQDataStrings.Add(frequenz, "");
                voiceFQDataStrings[frequenz] = s + ";";
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public void leaveRadio(RXPlayer player) => player.Frequency = 0;

        //[HandleExceptions]
        [RemoteEvent]
        public async Task SetFunk(RXPlayer player, int state)
        {
            if (!player.CanInteract()) return;

            if (await player.GetFrequencyAsync() < 1)
            {
                player.FunkStatus = FunkStatus.Deactive;
                await player.TriggerEventAsync("updateVoiceState", 0);

                return;
            }

        

            FunkStatus old = await player.GetFunkStatusAsync();
            player.FunkStatus = (FunkStatus)state;

            double frequency = await player.GetFrequencyAsync();

            await refreshFQVoiceForPlayerFrequenz(player);


            var funkStatus = await player.GetFunkStatusAsync();

            switch (funkStatus)
            {
                case FunkStatus.Active:
                    await player.TriggerEventAsync("DauerFunk", false);
                    if (!await player.GetIsInVehicleAsync() && !player.IsCuffed && !player.IsTied)
                        await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "random@arrests", "generic_radio_chatter");

                    break;
                case FunkStatus.Hearing:
                    await player.TriggerEventAsync("DauerFunk", true);
                    break;
                case FunkStatus.Deactive:
                    await player.TriggerEventAsync("leaveFunk");
                    await player.TriggerEventAsync("DauerFunk", false);
                    if (await player.GetIsInVehicleAsync())
                        break;

                    await player.StopAnimationAsync();
                    break;
                default:
                    break;
            }

            await player.TriggerEventAsync("SetFunkStatus", state);
        }
    }
}
