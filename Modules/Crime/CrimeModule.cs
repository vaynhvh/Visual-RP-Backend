using Backend.Models;
using Backend.MySql.Models;
using Backend.MySql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Modules.Faction;
using Backend.Modules.Jail;
using Backend.Modules.Laptop.Apps;
using Backend.Modules.Voice;
using System.Linq;
using System.Net;
using GTANetworkAPI;

namespace Backend.Modules.Crime
{
    class CrimeModule : RXModule
    {
        public CrimeModule() : base("CrimeModule") { }

        public static List<DbPlayerCrimeData> PlayerCrimeData = new List<DbPlayerCrimeData>();
        public static List<DbPlayerCrimes> PlayerCrimes = new List<DbPlayerCrimes>();
        public static List<DbCrimes> Crimes = new List<DbCrimes>();


        //[HandleExceptions]
        public override async Task OnTwoSecond()
        {


            using var db = new RXContext();

            Crimes = await db.Crimes.ToListAsync();
            PlayerCrimes = await db.PlayerCrimes.ToListAsync();
            PlayerCrimeData = await db.PlayerCrimeData.ToListAsync();
        }

        public static int CalcJailCosts(RXPlayer player)
        {

            if (player == null) return 0;

            if (player.PlayerCrimes == null) return 0;

            int money = 0;

            foreach (var crimes in player.PlayerCrimes)
            {
                var crime = Crimes.Find(x => x.Id == crimes.CrimeId);

                money += crime.Bussgeld;
            }

            return money;

        }

        public static int CalcJailTime(RXPlayer player)
        {

            if (player == null) return 0;

            if (player.PlayerCrimes == null) return 0;

            int jailtime = 0;

            foreach (var crimes in player.PlayerCrimes)
            {
                var crime = Crimes.Find(x => x.Id == crimes.CrimeId);

                jailtime += crime.Haftzeit;
            }

            return jailtime;

        }

        public static async Task ArrestPlayer(RXPlayer iPlayer, RXPlayer iPlayerCop, bool SpawnPlayer = true)
        {
            if (iPlayer.Team.IsState() && iPlayer.InDuty) return;

            var wanteds = iPlayer.Jailtime;
            if (iPlayer.Jailtime < 30) wanteds = 30;

            int jailtime = CrimeModule.CalcJailTime(iPlayer);
            int jailcosts = CrimeModule.CalcJailCosts(iPlayer);

            iPlayer.Jailtime = (uint)jailtime;

            // Checke auf Jailtime
            if (iPlayerCop != null && iPlayer.Jailtime == 0)
            {
                await iPlayerCop.SendNotify(await iPlayer.GetNameAsync() + " hat keine Haftzeit offen!");
                return;
            }

            string JailStringHistroy = $"Inhaftierung ({jailtime} | $ {jailcosts}):";

            string ListCrimes = "Sie wurden wegen folgenden Verbrechen Inhaftiert: ";

            foreach (var ss in iPlayer.PlayerCrimes)
            {

                var crime = CrimeModule.Crimes.Find(x => x.Id == ss.Id);
                ListCrimes += crime.Grund + ",";

                if (crime.Haftzeit > 0)
                {
                    JailStringHistroy += crime.Grund + ",";
                }
            }


            using var db = new RXContext();


            List<DbPlayerCrimes> playercrimes = await db.PlayerCrimes.ToListAsync();

            foreach (var playercrime in playercrimes.Where(x => x.PlayerId == iPlayer.Id))
            {
                db.PlayerCrimes.Remove(playercrime);
            }
            await db.SaveChangesAsync();

            iPlayer.PlayerCrimes.Clear();

            iPlayer.ResetData("follow");

            if (iPlayerCop != null)
            {
                iPlayerCop.ResetData("follow");
                await iPlayerCop.SendNotify("Du hast " + await iPlayer.GetNameAsync() + " für " + (jailtime - 1) + " Hafteinheiten ins Gefängnis gesteckt!");
                await iPlayer.SendNotify("Du wurdest von " + await iPlayerCop.GetNameAsync() + " ins Gefängnis gesteckt! Hafteinheiten: " + (jailtime - 1) +" Minuten");
            }

            await iPlayer.TakeAnyMoney((int)jailcosts, ListCrimes, true);
            await RX.GiveMoneyToStaatskonto(jailcosts, "");

            await iPlayer.SendNotify("Durch Ihre Inhaftierung wurde Ihnen eine Strafzahlung von $" + jailcosts + " in Rechnung gestellt!");

            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState("An Alle Einheiten, " + await iPlayer.GetNameAsync() + " sitzt nun hinter Gittern!");

            await iPlayer.SendNotify(ListCrimes);

            // Set Voice To Normal
            NAPI.Task.Run(() => iPlayer.SetSharedData("voiceRange", (int)VoiceRange.whisper));
           
            iPlayer.SetData("voiceType", 3);
            await iPlayer.TriggerEventAsync("setVoiceType", 3);

            if (SpawnPlayer)
            {
                await iPlayer.RemoveAllWeaponsAsync();

                await iPlayer.SetPositionAsync(JailModule.PrisonSpawn);
            }

        }
    }
}
