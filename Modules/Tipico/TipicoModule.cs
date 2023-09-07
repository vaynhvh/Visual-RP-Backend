using Backend.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Newtonsoft.Json;
using Org.BouncyCastle.Math.Field;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Tipico
{
    /*public class BundesligaGame
    {
        public string id { get; set; }
        public string sport_key { get; set; }
        public string sport_title { get; set; }
        public string commence_time { get; set; }
        public bool completed { get; set; }
        public string home_team { get; set; }
        public string away_team { get; set; }
        public int[] scores { get; set; }
        public double odd0 { get; set; } = 0.0;
        public double odd1 { get; set; } = 0.0;
        public double odd2 { get; set; } = 0.0;
   
    }

    public class BundesligaMarket
    {
        public string id { get; set; }
        public string sport_key { get; set; }
        public string sport_title { get; set; }
        public string commence_time { get; set; }
        public string home_team { get; set; }
        public string away_team { get; set; }
        public List<BundesligaBookmarker> bookmakers { get; set;}  

    }

    public class BundesligaBookmarker
    {
        public string key { get; set; }
        public string title { get; set; }
        public List<BundesligaMarkets> markets { get; set; }
    }

    public class BundesligaMarkets
    {
        public string key { get; set; }
        public List<BundesligaOutcomes> outcomes { get; set; }

    }

    public class BundesligaOutcomes
    {
        public string name { get; set; }
        public double price { get; set; }

    }
    class TipicoModule : RXModule
    {
        public TipicoModule() : base("Tipico", new Models.RXWindow("Tipico")) { }

        public List<BundesligaGame> BundesligaGames = new List<BundesligaGame>();
        public List<BundesligaMarket> BundesligaMarket = new List<BundesligaMarket>();

        public override async void LoadAsync()
        {
        /*    using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync("https://api.the-odds-api.com/v4/sports/soccer_germany_bundesliga/scores?apiKey=d454912eb1d6c5119e97dd07e1c19a8a&daysFrom=3&dateFormat=unix");
                BundesligaGames = JsonConvert.DeserializeObject<List<BundesligaGame>>(json);
            }
            using (var httpClient = new HttpClient())
            {
                var json = await httpClient.GetStringAsync("https://api.the-odds-api.com/v4/sports/soccer_germany_bundesliga/odds/?regions=eu&dateFormat=unix&oddsFormat=decimal&markets=h2h&apiKey=d454912eb1d6c5119e97dd07e1c19a8a");
            
                
                BundesligaMarket = JsonConvert.DeserializeObject<List<BundesligaMarket>>(json);
            }

            foreach (BundesligaGame game in BundesligaGames)
            {
                try
                {
                    var market = BundesligaMarket.Find(x => x.id == game.id);

                    if (market == null)
                    {
                        RXLogger.Print("Market is null!");
                        return;
                    }

                    game.odd1 = market.bookmakers[0].markets[0].outcomes.Find(x => x.name == game.home_team).price;
                    game.odd0 = market.bookmakers[0].markets[0].outcomes.Find(x => x.name == game.away_team).price;
                    game.odd2 = market.bookmakers[0].markets[0].outcomes.Find(x => x.name == "Draw").price;

                } catch (Exception ex)
                {
                    RXLogger.Print(ex.Message);
                }

            }
        
            //var mcb = await NAPI.Entity.CreateMCB(new GTANetworkAPI.Vector3(959.2302, 25.226269, 76.991325), new Color(255, 140, 0), 0u, 2f, 2.4f, false, MarkerType.VerticalCylinder, false);

            /*mcb.ColShape.Message = new RXMessage
            {
                Text = "Drücke E um eine Wette abzugeben!",
                Color = "gold",
                Duration = 3500,
                Title = "Tipico"
            };

            //mcb.ColShape.Action = async player => await OpenTipico(player);

        }

        public async Task OpenTipico(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            await this.Window.OpenWindow(player, BundesligaGames);
        }
    }*/
}
