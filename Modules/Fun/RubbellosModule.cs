using Backend.Models;
using Backend.Utils;
using Google.Protobuf.WellKnownTypes;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Fun
{
    class RubbellosModule : RXModule
    {
        public RubbellosModule() : base("Rubbellos") { }

        public static Dictionary<RXPlayer, int[]> lose = new Dictionary<RXPlayer, int[]>();
        public static int[] losgewinne = new int[12] { 250, 500, 1000, 2000, 4000, 5000, 8000, 10000, 20000, 40000, 100000, 400000 };
        public static int[] losgewinnebad = new int[7] { 250, 500, 1000, 2000, 4000, 5000, 8000 };

        [RemoteEvent]
        public async Task Scratchcard(RXPlayer player)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return;

            if (lose.ContainsKey(player))
            {
                int[] value = new int[6];
                if (lose.TryGetValue(player, out value))
                {

                    var groups = value.GroupBy(v => v);
                    foreach (var group in groups)
                    {
                        if (group.Count() == 3)
                        {
                            await player.SendNotify("Du hast " + group.Key + "$ gewonnen!", 5000, "yellow");
                            await player.GiveMoney(group.Key);
                            lose.Remove(player);
                            return;
                        }
                    }
                    await player.SendNotify("Du hast verloren!", 5000, "red");
                    lose.Remove(player);

                }
            }
            else
            {
                await player.SendNotify("Es ist ein Fehler aufgetreten. Bitte melde diesen umgehend im Support!", 5000, "red");
            }

        }

    }
}
