using Backend.Models;
using Backend.Modules.Faction;
using Backend.MySql.Models;
using GTANetworkAPI;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Leitstellen
{
    public class TeamLeitstellenObject
    {
        public uint TeamId { get; set; }
        public int Number { get; set; }
        public RXPlayer Acceptor { get; set; }
        public bool StaatsFrakOnly { get; set; }
    }
    public class CustomMarkerPlayerObject
    {
        [JsonProperty(PropertyName = "pos")]
        public Vector3 Position { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "color")]
        public int Color { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int MarkerId { get; set; }
    }
        
    public static class CustomMarkersKeys
    {
        public static string GarbageJob = "garbage";
        public static string AirFlightControl = "airflight";
        public static string FishingJob = "fishing";
        public static string Leitstelle = "leitst";
    }

    class LeitstellenModule : RXModule
    {
        public LeitstellenModule() : base("Leistelle") { }
        public static Dictionary<int, TeamLeitstellenObject> TeamNumberPhones = new Dictionary<int, TeamLeitstellenObject>();

        public override void LoadAsync()
        {
            RegisterNumber(1, 911, true);
            RegisterNumber(5, 914, true);
            RegisterNumber(3, 912, false);
            RegisterNumber(20, 915, false);
            RegisterNumber(6, 916, false);

        }
        public void RegisterNumber(uint teamId, int number, bool staatsfrakonly)
        {
            if (!TeamNumberPhones.ContainsKey(number))
            {
                TeamNumberPhones.Add(number, new TeamLeitstellenObject()
                {
                    TeamId = teamId,
                    Number = number,
                    Acceptor = null,
                    StaatsFrakOnly = staatsfrakonly
                });
            }
        }

        public override async Task OnPlayerDisconnect(RXPlayer player, DisconnectionType type, string reason)
        {
            TeamLeitstellenObject teamLeitstellenObject = GetByAcceptor(player);

            if (teamLeitstellenObject != null)
            {
                teamLeitstellenObject.Acceptor = null;
                TeamModule.Teams.Find(x => x.Id == teamLeitstellenObject.TeamId).SendNotification("Ihre Leitstelle ist nun nicht mehr besetzt!"); 
            }
        }

        public static bool hasLeitstelleFunction(uint teamid)
        {
            return TeamNumberPhones.ToList().Where(lt => lt.Value.TeamId == teamid).Count() > 0;
        }

        public static bool IsLeiststelle(RXPlayer player)
        {
            return TeamNumberPhones.Values.ToList().Where(lt => lt.Acceptor == player).Count() > 0;
        }
        public static TeamLeitstellenObject GetLeitstelle(uint teamid)
        {
            return TeamNumberPhones.Values.ToList().Where(lt => lt.TeamId == teamid).FirstOrDefault();
        }

        public static TeamLeitstellenObject GetLeitstelleByNumber(int number)
        {
            if (!TeamNumberPhones.ContainsKey(number)) return null;
            return TeamNumberPhones[number];
        }

        public static TeamLeitstellenObject GetByAcceptor(RXPlayer player)
        {
            return TeamNumberPhones.Values.ToList().Where(lt => lt.Acceptor != null && lt.Acceptor == player).FirstOrDefault();
        }
    }
}
