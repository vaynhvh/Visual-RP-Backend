using Backend.Models;
using Backend.Modules.Shops;
using Backend.Utils.Extensions;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.BlackMarket
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class BlackMarketModule : RXModule
    {
        public BlackMarketModule() : base("BlackMarket") { }

        public static List<Tuple<Vector3, float>> MarketPositions = new List<Tuple<Vector3, float>>
        {
            new Tuple<Vector3, float>(new Vector3(43.36047, -2662.019, 6.0090613), -93.97612f),
            new Tuple<Vector3, float>(new Vector3(-69.91789, -1230.8265, 28.944416), -131.59653f),
            new Tuple<Vector3, float>(new Vector3(2193.9863, 5594.022, 53.75537), -15.274638f),
            new Tuple<Vector3, float>(new Vector3(-2173.7986, 4282.2896, 49.121967), -116.41314f),
        };

        public static Dictionary<string, int> MarketStorage = new Dictionary<string, int>
        {
            { "Weedsamen", 0 }
        };

        public override async void LoadAsync()
        {
            Random rnd = new Random();
            Tuple<Vector3, float> MarketPos = MarketPositions[rnd.Next(MarketPositions.Count)];

            if (MarketPos == null) return;

            MarketStorage.forEachAlternative(product =>
            {
                MarketStorage[product.Key] = new Random().Next(100);
            });

            await NAPI.Task.RunAsync(() => new NPC((PedHash)NAPI.Util.GetHashKey("s_m_y_dealer_01"), MarketPos.Item1, MarketPos.Item2, 0u));

            {
                var mcb = await NAPI.Entity.CreateMCB(MarketPos.Item1, new Color(255, 140, 0), 0u, 4.4f, 2.4f, false, MarkerType.VerticalCylinder, false, 285, 82, "Schwarzmarkt WIRD ENTFERNT");

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um mit dem Schwarzmarkthändler zu sprechen.",
                    Color = "dgray",
                    Duration = 3500,
                    Title = "Schwarzmarkt",
                    OnlyBadFaction = false
                };

                mcb.ColShape.Action = async player =>
                {
                    int amount = 0;

                    await MarketStorage.forEach(product => amount += product.Value);

                    if (player.TeamId == 0 || DateTime.Now.Hour < 20 || amount < 1 || player.Team.Type != Faction.TeamType.Gang)
                    {
                        if (!await player.CanInteractAntiFloodNoMSG(3)) return;

                        if (DateTime.Now.Hour < 20 || amount < 1) await player.SendNotify("Hier gibt es nix zu sehen, komme zu einem späteren Zeitpunkt wieder", 3500, "dgray", "Schwarzmarkt");
                        
                        return;
                    }

                    await ShopModule.OpenShop(player, 9992);
                };
            }
        }
    }
}
