using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Crime;
using Backend.Modules.Faction;
using Backend.Modules.Gangwar;
using Backend.Modules.Laptop.Apps;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Backend.Modules.Staatsfraktionen.LSPD
{
    class TicketMachine : RXModule
    {
        public TicketMachine() : base("TicketMachine", new RXWindow("TicketMachine")) { }

        public override async void LoadAsync()
        {
            var mcb = await NAPI.Entity.CreateMCB(new Vector3(435.1083f, -976.8969f, 30.71304f), new Color(255, 140, 0), 0u, 1.4f, 1.4f, false, MarkerType.UpsideDownCone, true, 60, 63, "Los Santos Police Department");

            mcb.ColShape.Message = new RXMessage
            {
                Text = "Benutze E um auf den Ticket-Automat zuzugreifen",
                Color = "darkblue",
                Duration = 3500,
                Title = "LOS SANTOS POLICE DEPARTMENT"
            };

            mcb.ColShape.Action = async player => await OpenTicketAutomat(player);
        }

        public async Task OpenTicketAutomat(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;
            using var db = new RXContext();

            List<PlayerActiveCrimesAUTOMAT> l_List = new List<PlayerActiveCrimesAUTOMAT>();

            try
            {
                foreach (var l_Reason in player.PlayerCrimes)
                {
                    var crime = await db.NewCrimes.FirstOrDefaultAsync(x => x.i == l_Reason.CrimeId);

                    if (crime == null) continue;
                    if (crime.j > 0) continue;
                    var target = await db.Players.FirstOrDefaultAsync(x => x.Id == l_Reason.OfficerId);

                    l_List.Add(new PlayerActiveCrimesAUTOMAT() { Id = (int)crime.i, Name = crime.n, Costs = crime.p, Jailtime = crime.j, Officer = target.Username, Date = l_Reason.Uhrzeit });
                }
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }

            object confirmationBoxObject = new
            {
                m = player.Cash,
                crimes = l_List,
            };

            await this.Window.OpenWindow(player, confirmationBoxObject);
        }
        

        [RemoteEvent]
        public async Task PayForCrime(RXPlayer dbPlayer, int id)
        {

            if (dbPlayer == null) return;



            using var db = new RXContext();

            var allcrime = await db.PlayerCrimes.ToListAsync();
            var newcrimes = await db.NewCrimes.ToListAsync();
            DbPlayerCrimes crimePlayerReason = allcrime.Where(cpr => cpr.CrimeId == (uint)id).FirstOrDefault();

            if (!await dbPlayer.TakeMoney(newcrimes.Where(x => x.i == crimePlayerReason.CrimeId).FirstOrDefault().p)) {
                await dbPlayer.SendNotify("Du hast nicht genug Geld!");
                return;
            }

            if (crimePlayerReason != null)
            {
                db.PlayerCrimes.Remove(crimePlayerReason);
                TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState($"Leitstelle (TICKETAUTOMAT) hat die Akte von {await dbPlayer.GetNameAsync()} bearbeitet!");

                await db.SaveChangesAsync();
            }
        }

        [RemoteEvent]
        public async Task PayAllCrime(RXPlayer dbPlayer)
        {

            if (dbPlayer == null) return;



            using var db = new RXContext();

            var allcrime = await db.PlayerCrimes.ToListAsync();
            var newcrimes = await db.NewCrimes.Where(x => x.j == 0).ToListAsync();
            uint price = 0;
            foreach (var c in allcrime)
            {
                var crime = newcrimes.FirstOrDefault(x => x.i == c.CrimeId);

                if (crime == null) continue;

                price += (uint)crime.p;
            }

            if (!await dbPlayer.TakeMoney((int)price))
            {
                await dbPlayer.SendNotify("Du hast nicht genug Geld!");
                return;
            }

            foreach (var c in allcrime)
            {
                var crime = newcrimes.FirstOrDefault(x => x.i == c.CrimeId);
                if (crime == null) continue;

                if (crime.j > 0) return;

                db.PlayerCrimes.Remove(c);

                await db.SaveChangesAsync();
            }
            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState($"Leitstelle (TICKETAUTOMAT) hat die Akte von {await dbPlayer.GetNameAsync()} erlassen!");

        }

    }
}
