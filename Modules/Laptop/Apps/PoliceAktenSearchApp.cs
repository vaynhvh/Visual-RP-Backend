using Backend.Models;
using Backend.Modules.Faction;
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

namespace Backend.Modules.Laptop.Apps
{

    public class PlayerSearchObject
    {
        [JsonProperty("i")]
        public int Id { get; set; }

        [JsonProperty("n")]
        public string Name { get; set; }

    }


    class PoliceAktenSearchApp : RXModule
    {

        public PoliceAktenSearchApp() : base("PoliceAktenSearchApp", new RXWindow("PoliceComputer")) { }

        [RemoteEvent]
        public async Task PolPlayersByName(RXPlayer client, string searchQuery)
        {

            await HandlePoliceAktenSearch(client, searchQuery);
        }

        public async Task HandlePoliceAktenSearch(RXPlayer p_DbPlayer, string searchQuery)
        {
            if (p_DbPlayer == null)
                return;

            var l_Overview = await GetSearchResults(searchQuery);

            List<PlayerSearchObject> playerSearchObjects = new List<PlayerSearchObject>();

            foreach (DbPlayer bp in l_Overview)
            {
                playerSearchObjects.Add(new PlayerSearchObject() { Id = (int)bp.Id, Name = bp.Username });
            }



            await p_DbPlayer.TriggerEventAsync("SendPolPlayers", NAPI.Util.ToJson(playerSearchObjects));
        }
        public async Task<List<DbPlayer>> GetSearchResults(string searchQuery)
        {
            List<DbPlayer> results = new List<DbPlayer>();
            using var db = new RXContext();

            var players = await db.Players.ToListAsync();

            foreach (DbPlayer player in players)
            {
                if (player.Username.Contains(searchQuery))
                {
                    results.Add(player);
                }
                if (player.Id.ToString().Contains(searchQuery))
                {
                    results.Add(player);
                }

                    if (player.TeamId != 0) { 
                    if (TeamModule.Teams.Find(x => x.Id == player.TeamId).Name.ToString().Contains(searchQuery))
                    {
                        results.Add(player);
                    }
                }
            }

            return results;

        }


    }
}
