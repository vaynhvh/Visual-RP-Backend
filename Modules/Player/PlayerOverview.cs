using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Tablet.Apps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Player
{
    public class PlayerOverviewObject
    {
        [JsonProperty("t")]
        public bool isTeam { get; set; }

        [JsonProperty("d")]
        public List<PlayerOverviewPlayerObject> data { get; set; }
    }

    public class PlayerOverviewPlayerObject
    {
        [JsonProperty("i")]
        public uint PlayerId { get; set; }

        [JsonProperty("n")]
        public string PlayerName { get; set; }

        [JsonProperty("f")]
        public string Funk { get; set; }

        [JsonProperty("vh")]
        public string VoiceHash { get; set; }
    }

    class PlayerOverview : RXModule
    {
        public PlayerOverview() : base("PlayerOverview", new RXWindow("PlayerOverview")) { }

        [RXCommand("players", 1)]
        public async Task players(RXPlayer player, string[] args)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            var players = PlayerController.GetValidPlayers();

            var pp = new List<PlayerOverviewPlayerObject>();


            foreach (var pl in players)
            {
                pp.Add(new PlayerOverviewPlayerObject() { PlayerId = pl.Id, PlayerName = await pl.GetNameAsync(), Funk = pl.Frequency.ToString(), VoiceHash = pl.VoiceHash });
            }

            var playeroverview = new PlayerOverviewObject() { isTeam = false, data = pp };

            await this.Window.OpenWindow(player, playeroverview);
        }

        [RXCommand("team", 1)]
        public async Task team(RXPlayer player, string[] args)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            var players = PlayerController.GetValidPlayers();

            var pp = new List<PlayerOverviewPlayerObject>();


            foreach (var pl in players)
            {
                if (pl.Teamrank < 1)
                {
                    pp.Add(new PlayerOverviewPlayerObject() { PlayerId = pl.Id, PlayerName = await pl.GetNameAsync(), Funk = pl.Frequency.ToString(), VoiceHash = pl.VoiceHash });
                }
            }

            var playeroverview = new PlayerOverviewObject() { isTeam = true, data = pp };

            await this.Window.OpenWindow(player, playeroverview);
        }

    }
}
