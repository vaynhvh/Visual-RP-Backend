using Backend.Models;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    public class ProfileItem
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }

        public ProfileItem(string name, string value, string image)
        {
            Name = name;
            Value = value;
            Image = image;
        }
    }
    public class ProfileData
    {
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "l")]
        public int Visum { get; set; }

        [JsonProperty(PropertyName = "h")]
        public int House { get; set; }

        [JsonProperty(PropertyName = "f")]
        public string Frak { get; set; }

        [JsonProperty(PropertyName = "j")]
        public string Job { get; set; }

        [JsonProperty(PropertyName = "b")]
        public string Business { get; set; }
        [JsonProperty(PropertyName = "p")]
        public int Phone { get; set; }
    }
    class ProfileApp : RXModule
    {
        public ProfileApp() : base("ProfileApp", new RXWindow("Phone")) { }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task RqCharacterInfo(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            string[] names = (await player.GetNameAsync()).Split('_');
            if (names == null || names.Length < 2) return;

            var profile = new ProfileData() { Name = names[0] + "_" + names[1], Business = "Kein Business", House = (int)player.HouseId, Frak = player.Team.Name, Job = "Arbeitslos", Phone = (int)player.Phone, Visum = player.Level };

            await player.TriggerEventAsync("RsCharacterInfo", JsonConvert.SerializeObject(profile));
        }
    }
}
