using Backend.Controllers;
using Backend.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Sound
{
    /*public class Sound
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "volume")]
        public double Volume { get; set; }

        [JsonProperty(PropertyName = "range")]
        public int Range { get; set; }

        [JsonProperty(PropertyName = "listeners")]
        public List<uint> Listeners { get; set; }

        [JsonProperty(PropertyName = "paused")]
        public bool Paused { get; set; }

        [JsonProperty(PropertyName = "startTime")]
        public int StartTime { get; set; }

        [JsonProperty(PropertyName = "trackLength")]
        public int TrackLength { get; set; }

    }

    class SoundModule : RXModule
    {
        public SoundModule() : base("3D Sound") { }

        public static Dictionary<uint, Sound> ServerSounds = new Dictionary<uint, Sound>();

        public override void Load()
        {
            //50ms
            Timer timer = new Timer(50);

            timer.Elapsed += async (sender, e) =>
            {
                await SetVehiclePositions();
            };

            timer.AutoReset = true;
            timer.Enabled = true;
        }

        //[HandleExceptions]
        public async Task SetVehiclePositions()
        {
            var vehicles = await NAPI.Task.RunReturnAsync(() => VehicleController.GetVehicles());

            foreach (RXVehicle vehicle in vehicles.ToList())
            {
                if (!ServerSounds.ContainsKey(vehicle.Id) || ServerSounds[vehicle.Id].Paused) continue;
                
                var maxRange = ServerSounds[vehicle.Id].Range;

                foreach (uint listenerId in ServerSounds[vehicle.Id].Listeners.ToList())
                {
                    var listener = await PlayerController.FindPlayerById(listenerId);
                    if (listener == null || !await NAPI.Task.RunReturnAsync(() => listener.Exists)) continue;

                    var distance = await NAPI.Task.RunReturnAsync(() => vehicle.Position.DistanceTo(listener.Position));
                    if (distance > maxRange)
                    {
                        ServerSounds[vehicle.Id].Listeners.Remove(listenerId);
                        await listener.TriggerEventAsync("destroySound", vehicle.Id);
                    }
                    else
                    {
                        var volume = ServerSounds[vehicle.Id].Volume * (1 - (distance / maxRange));
                        volume = volume < 0 ? 0 : volume;
                        await listener.TriggerEventAsync("setSoundVolume", vehicle.Id, volume * volume);
                    }
                }

                var playersInNear = await PlayerController.GetPlayersInRange(await NAPI.Task.RunReturnAsync(() => vehicle.Position), maxRange);

                foreach (RXPlayer player in playersInNear.ToList())
                {
                    if (player == null || !await NAPI.Task.RunReturnAsync(() => player.Exists) || ServerSounds[vehicle.Id].Listeners.Contains(player.Id)) continue;

                    var distance = await NAPI.Task.RunReturnAsync(() => vehicle.Position.DistanceTo(player.Position));
                    var volume = ServerSounds[vehicle.Id].Volume * (1 - (distance / maxRange));

                    volume = volume < 0 ? 0 : volume;
                    ServerSounds[vehicle.Id].Listeners.Add(player.Id);

                    await player.TriggerEventAsync("createSound", JsonConvert.SerializeObject(ServerSounds[vehicle.Id]), volume * volume);
                }

                if (ServerSounds[vehicle.Id].TrackLength <= ServerSounds[vehicle.Id].StartTime)
                {
                    foreach (uint listenerId in ServerSounds[vehicle.Id].Listeners.ToList())
                    {
                        var listener = await PlayerController.FindPlayerById(listenerId);
                        if (listener == null || !await NAPI.Task.RunReturnAsync(() => listener.Exists)) continue;

                        await listener.TriggerEventAsync("destroySound", vehicle.Id);
                    }

                    ServerSounds.Remove(vehicle.Id);
                    continue;
                }

                ServerSounds[vehicle.Id].StartTime += 50;
            }
        }

        //[HandleExceptions]
        [RemoteEvent("sound:create")]
        public async Task SoundCreate(RXPlayer player, uint id, string url, int range, double volume, int trackLength)
        {
            if (!player.IsLoggedIn || id == 0) return;

            if (ServerSounds.ContainsKey(id))
            {
                foreach (uint listenerId in ServerSounds[id].Listeners.ToList())
                {
                    var listener = await PlayerController.FindPlayerById(listenerId);
                    if (listener == null || !await NAPI.Task.RunReturnAsync(() => listener.Exists)) continue;

                    await listener.TriggerEventAsync("destroySound", id);
                }

                ServerSounds.Remove(id);
            }

            ServerSounds.Add(id, new Sound
            {
                Id = id,
                Url = url,
                Volume = volume,
                Range = range,
                Listeners = new List<uint>(),
                Paused = false,
                StartTime = 0,
                TrackLength = trackLength * 1000
            });
        }

        //[HandleExceptions]
        [RemoteEvent("sound:destroy")]
        public async Task SoundDestroy(RXPlayer player, uint id)
        {
            if (!player.IsLoggedIn || id == 0) return;

            if (ServerSounds.ContainsKey(id))
            {
                foreach (uint listenerId in ServerSounds[id].Listeners.ToList())
                {
                    var listener = await PlayerController.FindPlayerById(listenerId);
                    if (listener == null || !await NAPI.Task.RunReturnAsync(() => listener.Exists)) continue;

                    await listener.TriggerEventAsync("destroySound", id);
                }

                ServerSounds.Remove(id);
            }
        }

        //[HandleExceptions]
        [RemoteEvent("sound:pause")]
        public async Task SoundPause(RXPlayer player, uint id)
        {
            if (!player.IsLoggedIn || id == 0) return;

            if (ServerSounds.ContainsKey(id))
            {
                ServerSounds[id].Paused = true;

                foreach (uint listenerId in ServerSounds[id].Listeners.ToList())
                {
                    var listener = await PlayerController.FindPlayerById(listenerId);
                    if (listener == null || !await NAPI.Task.RunReturnAsync(() => listener.Exists)) continue;

                    await listener.TriggerEventAsync("pauseSound", id);
                }
            }
        }


        //[HandleExceptions]
        [RemoteEvent("sound:resume")]
        public async Task SoundResume(RXPlayer player, uint id)
        {
            if (!player.IsLoggedIn || id == 0) return;

            if (ServerSounds.ContainsKey(id))
            {
                ServerSounds[id].Paused = false;

                foreach (uint listenerId in ServerSounds[id].Listeners.ToList())
                {
                    var listener = await PlayerController.FindPlayerById(listenerId);
                    if (listener == null || !await NAPI.Task.RunReturnAsync(() => listener.Exists)) continue;

                    await listener.TriggerEventAsync("resumeSound", id);
                }
            }
        }

        //[HandleExceptions]
        [RemoteEvent("sound:setVolume")]
        public void SoundSetVolume(RXPlayer player, uint id, double volume)
        {
            if (!player.IsLoggedIn || id == 0) return;

            if (ServerSounds.ContainsKey(id)) ServerSounds[id].Volume = volume;
        }
    }*/
}
