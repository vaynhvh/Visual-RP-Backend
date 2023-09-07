using Backend.Models;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
using Backend.Modules.Phone.Apps;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Voice
{
    internal enum VoiceRange
    {
        normal = 8,
        whisper = 3,
        shout = 15,
        megaphone = 40,
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class VoiceModule : RXModule
    {
        public VoiceModule() : base("Voice") { }

        //[HandleExceptions]
        public static async Task ChangeFrequency(RXPlayer player, double frequency)
        {
            var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Funkgerät");
            if (model == null) return;

            if (player.Container.GetItemAmount(model) < 1)
            {
                await player.TriggerEventAsync("setRadioChatPlayers", "");

                player.Frequency = 0;
                player.FunkStatus = FunkStatus.Deactive;

                return;
            }

            if (TeamModule.Teams.FirstOrDefault(x => x.Id == frequency) != null && player.TeamId != frequency)
            {
                await player.SendNotify("Du hast keinen Zugriff auf diese Frequenz!");
                return;
            }
            player.Frequency = frequency;

            FunkApp.CheckFrequenz(frequency);
            FunkApp.voiceFQ[frequency].Add(player);

            await player.TriggerEventAsync("SetRadioTalkUwe", frequency.ToString());
            await FunkApp.actualizeFrequenzDataString(frequency);
            await FunkApp.refreshFQVoiceForPlayerFrequenz(player);


            if (frequency == 0) player.FunkStatus = FunkStatus.Deactive;
        }
        [RemoteEvent]
        public async void VoiceSound(RXPlayer player, string sound, bool loop, string dic)
        {

            await player.TriggerEventAsync("VoiceSound", sound, loop, dic);

        }

        [RemoteEvent]
        public async void VoiceStop(RXPlayer player, string sound)
        {

            await player.TriggerEventAsync("VoiceStop", sound);

        }
        //[HandleExceptions]
        [RemoteEvent]
        public void changeVoiceRange(RXPlayer player)
        {
            if (!player.IsLoggedIn) return;

            //jail check

            // 1 = normal, 2 = whisper, 3 = schreien 4 (optional) = megaphone
            int voicetype = 1;
            if (player.HasData("voiceType"))
            {
                voicetype = player.GetData<int>("voiceType");
            }

            if (voicetype == 1)
            {
                player.SetSharedData("voiceRange", (int)VoiceRange.shout);
                player.SetData("voiceType", 2);
                player.TriggerEvent("setVoiceType", 2);
            }
            else if (voicetype == 2)
            {
                player.SetSharedData("voiceRange", (int)VoiceRange.whisper);
                player.SetData("voiceType", 3);
                player.TriggerEvent("setVoiceType", 3);
            }
            //megaphon
            else if (voicetype == 3)
            {
                player.SetSharedData("voiceRange", (int)VoiceRange.normal);
                player.SetData("voiceType", 1);
                player.TriggerEvent("setVoiceType", 1);
            }
            else if (voicetype == 4)
            {
                player.SetSharedData("voiceRange", (int)VoiceRange.normal);
                player.SetData("voiceType", 1);
                player.TriggerEvent("setVoiceType", 1);
            }
        }
    }
}
