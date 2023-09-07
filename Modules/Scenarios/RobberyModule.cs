using Backend.Models;
using Backend.Modules.Crime;
using Backend.Modules.Faction;
using Backend.Modules.Phone;
using Backend.Modules.Shops;
using Backend.MySql.Models;
using GTANetworkAPI;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Scenarios
{
    public class Rob
    {
        public int Id { get; set; }
        public RXPlayer Player { get; set; }
        public int Interval { get; set; }
        public int CopInterval { get; set; }
        public int EndInterval { get; set; }
        public bool Disabled { get; set; }
        public RobType Type { get; set; }
    }

    public enum RobType
    {
        Shop,
        Juwelier,
        Staatsbank,
        VespucciBank
    }

    class RobberyModule : RXModule
    {
        public RobberyModule() : base("Robbery") { }

        public static uint MarkierteScheineID = 74;
        public const int Juwelier = -2;

        public static Dictionary<int, Rob> Robberies;

        public static DateTime LastScenario = DateTime.Now.AddHours(-2);

        public static Dictionary<uint, int> RobbedAtms = new Dictionary<uint, int>();

        public override void LoadAsync()
        {
            Robberies = new Dictionary<int, Rob>();
            RobbedAtms = new Dictionary<uint, int>();
        }

        public static bool IsJuweRobbable()
        {
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (StaatsbankRobberyModule.IsActive)
            {
                return false;
            }

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 30)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 30)
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }

        public static bool CanAtmRobbed()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 45)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 25)
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }



        public static Rob GetScenarioById(int id)
        {
            return Robberies.TryGetValue(id, out var rob) ? rob : null;
        }

        public static void Add(int id, RXPlayer iPlayer, int startinterval, RobType type = RobType.Shop, int copinterval = 2, int endinterval = 30)
        {
            if (Robberies.ContainsKey(id))
            {
                Robberies[id].Player = iPlayer;
            }
            else
            {
                var rob = new Rob
                {
                    Id = id,
                    Player = iPlayer,
                    Interval = startinterval,
                    CopInterval = copinterval,
                    EndInterval = endinterval,
                    Disabled = false,
                    Type = type
                };
                Robberies.Add(id, rob);
            }
        }

        public static void RemovePlayerRobs(RXPlayer dbPlayer)
        {
            var index = 0;
            while (index < Robberies.Count)
            {
                if (index >= Robberies.Count) break;
                var rob = Robberies.ElementAt(index++).Value;
                if (rob.Player.Id == dbPlayer.Id)
                {
                    Robberies.Remove(rob.Id);
                }
            }
        }

        public static bool IsActive(int robid)
        {
            var rob = GetScenarioById(robid);
            if (rob == null) return false;
            return !rob.Disabled && rob.Interval > 0;
        }

        public static List<Rob> GetActiveShopRobs()
        {
            return (from rob in Robberies
                    where rob.Value.Id > 0 && rob.Value.Type == RobType.Shop && rob.Value.Player != null &&
                          !rob.Value.Disabled
                    select rob.Value).ToList();
        }

        public static bool IsAnyShopInRobbing()
        {
            return Robberies.Any(rob =>
                rob.Value.Id > 0 && !rob.Value.Disabled && rob.Value.Interval > 0 && rob.Value.Player != null &&
                rob.Value.Type == RobType.Shop);
        }

        public static Dictionary<int, Rob> GetRobs()
        {
            return Robberies;
        }

        public static List<Rob> GetActiveRobs(bool displayonly = false)
        {
            return (from rob in Robberies where !rob.Value.Disabled && (rob.Value.Interval > 2 || rob.Value.Type != RobType.Shop || !displayonly) && rob.Value.Player != null select rob.Value)
                .ToList();
        }


        public override async Task OnMinute()
        {
       /*     var rnd = new Random();
            var hour = DateTime.Now.Hour;

            var robs = GetActiveRobs();
            if (robs == null) return;
            var index = 0;
            while (index < robs.Count)
            {
                if (index >= Robberies.Count) break;
                var rob = robs[index++];
                if (rob == null) continue;

                var iPlayer = rob.Player;
                if (iPlayer == null)
                {
                    rob.Disabled = true;
                }
                else
                {
                    iPlayer.IsInRob = true;
                    if (rob.Type == RobType.Juwelier)
                    {
                        var robplayerpos = await iPlayer.GetPositionAsync();
                        if (robplayerpos.DistanceTo(new Vector3(-622.5494, -229.5598, 38.05706)) < 10.0f && !iPlayer.DeathData.IsDead)
                        {
                            if (rob.Interval > 0)
                            {
                                if (rob.Interval == 19)
                                {
                                    await iPlayer.SendNotify(

                                        "Eine Nachricht an die Staatsmächte wurde gesendet. Yallah raus hier!");
                                }
                                if (rob.Interval < 17)
                                {
                                    var erhalt = rnd.Next(5, 9);
                                    if (!iPlayer.Container.AddItem(75, erhalt))
                                    {
                                        await iPlayer.SendNotify("Dafür hast du keinen Platz!");
                                    }
                                }
                                rob.Interval--;
                            }
                            else
                            {
                                var erhalt = rnd.Next(75, 90);
                                if (!iPlayer.Container.AddItem(75, erhalt))
                                {
                                    await iPlayer.SendNotify("Dafür hast du keinen Platz!");
                                }
                                await iPlayer.SendNotify(
                                    "Du hast alle Diamanten geklaut!");
                                rob.Disabled = true;
                                iPlayer.IsInRob = false;
                            }
                        }
                        else
                        {
                            await iPlayer.SendNotify("Raub abgebrochen!");
                            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState(
                                "An Alle Einheiten, der Juwelier Raub wurde erfolgreich verhindert!");
                            rob.Disabled = true;
                            iPlayer.IsInRob = false;
                        }
                    }
                    else if (rob.Type == (int)RobType.Shop)
                    {
                        var shop = await ShopModule.GetNearShop(iPlayer, 15.0f);
                        var ppos = await iPlayer.GetPositionAsync();
                        if (shop == null)
                        {
                            await iPlayer.SendNotify("Raub abgebrochen!");
                            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState(
                                "An Alle Einheiten, ein Raub auf einen Store wurde verhindert!");

                            if (rob.Interval < rob.CopInterval + 2)
                                iPlayer.PlayerCrimes.Add(new DbPlayerCrimes { PlayerId = iPlayer.Id, CrimeId = 39 });

                            iPlayer.IsInRob = false;
                            rob.Disabled = true;
                            return;
                        }

                        if (shop != null && !iPlayer.DeathData.IsDead && (Math.Abs(ppos.Z - shop.Position.Z) <= 2f))
                        {
                            if (rob.Interval >= 2)
                            {
                                if (rob.Interval == rob.CopInterval)
                                {
                                    TeamModule.Teams.Find(x => x.Id == 1).SendNotification($"An Alle Einheiten, ein Einbruch in den Store {shop.CustomName} ({shop.Id}) wurde gemeldet!");
                                }
                                else if (rob.Interval >= rob.EndInterval)
                                {
                                    await iPlayer.SendNotify("Die Kasse ist leer!");
                                    iPlayer.IsInRob = false;
                                    rob.Disabled = true;
                                }
                                else
                                {
                                    var erhalt = rnd.Next(110, 190) * rob.Interval;
                                    iPlayer.Container.AddItem(MarkierteScheineID, erhalt);
                                    await iPlayer.SendNotify($"${erhalt} markierte Scheine erbeutet!");

                                }
                                rob.Interval++;
                            }
                            else
                            {
                                await iPlayer.SendNotify("Raub abgebrochen!");
                                TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState(
                                    "An Alle Einheiten, ein Raub auf einen Store wurde verhindert!");

                                if (rob.Interval < rob.CopInterval + 2)
                                    iPlayer.PlayerCrimes.Add(new DbPlayerCrimes { PlayerId = iPlayer.Id, CrimeId = 39 });

                                iPlayer.IsInRob = false;
                                rob.Disabled = true;
                            }
                        }
                    }
                }
            }*/
        }
    }
}