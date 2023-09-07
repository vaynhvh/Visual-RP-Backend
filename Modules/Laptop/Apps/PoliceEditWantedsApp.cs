using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Crime;
using Backend.Modules.Jail;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Laptop.Apps
{

    public class PlayerActiveCrimes
    {
        [JsonProperty("i")]
        public int Id { get; set; }

        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("d")]
        public string Date { get; set; }

        [JsonProperty("c")]
        public int Costs { get; set; }

        [JsonProperty("j")]
        public int Jailtime { get; set; }

        [JsonProperty("o")]
        public string Officer { get; set; }
    }

    public class PlayerActiveCrimesAUTOMAT
    {
        [JsonProperty("i")]
        public int Id { get; set; }

        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("d")]
        public string Date { get; set; }

        [JsonProperty("p")]
        public int Costs { get; set; }

        [JsonProperty("j")]
        public int Jailtime { get; set; }

        [JsonProperty("o")]
        public string Officer { get; set; }
    }
    public class CrimeJsonObject
    {
        [JsonProperty(PropertyName = "id")]
        public uint id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }
    }
    public class ReasonObject
    {
        [JsonProperty(PropertyName = "id")]
        public uint id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }
    }

    public class CategoryObject
    {
        [JsonProperty(PropertyName = "id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }
    }

    class PoliceEditWantedsApp : RXModule
    {

        public PoliceEditWantedsApp() : base("PoliceEditWantedsApp", new RXWindow("PoliceEditWantedsApp")) { }

        [RemoteEvent]
        public async Task requestWantedCategories(RXPlayer p_Player)
        {
            List<CategoryObject> l_List = new List<CategoryObject>();

            l_List.Add(new CategoryObject() { id = 1, name = "StVO" });
            l_List.Add(new CategoryObject() { id = 6, name = "StVO Erweiterung" });
            l_List.Add(new CategoryObject() { id = 2, name = "BtMG" });
            l_List.Add(new CategoryObject() { id = 3, name = "StGB" });
            l_List.Add(new CategoryObject() { id = 4, name = "StGB Erweiterung" });
            l_List.Add(new CategoryObject() { id = 5, name = "StGB Erweiterung 2" });


            var l_Json = NAPI.Util.ToJson(l_List);
            await this.Window.TriggerEvent(p_Player, "responseCategories", l_Json);
        }

        [RemoteEvent]
        public async Task requestCategoryReasons(RXPlayer p_Player, int p_ID)
        {
            var l_CrimeReasons = CrimeModule.Crimes;
            List<ReasonObject> l_List = new List<ReasonObject>();
            foreach (var l_Reason in l_CrimeReasons.ToList())
            {
                if (l_Reason.CatId == p_ID)
                {
                    if (l_List.Find(x => x.id == l_Reason.Id) == null)
                    {
                        var obj = new ReasonObject() { id = l_Reason.Id, name = l_Reason.Grund };
                        l_List.Add(obj);
                    }
                }
            }


            var l_Json = NAPI.Util.ToJson(l_List);
            await this.Window.TriggerEvent(p_Player, "responseCategoryReasons", l_Json);

        }

        [RemoteEvent]
        public async Task removeAllCrimes(RXPlayer dbPlayer, string name)
        {

            if (dbPlayer == null || !dbPlayer.Team.IsState()) return;

                var suspect = await PlayerController.FindPlayerByName(name);
                if (suspect == null) return;

                if (suspect.PlayerCrimes.Count > 0)
                {
                using var db = new RXContext();


                List<DbPlayerCrimes> playercrimes = await db.PlayerCrimes.ToListAsync();

                foreach (var playercrime in playercrimes.Where(x => x.PlayerId == suspect.Id))
                {
                    db.PlayerCrimes.Remove(playercrime);
                }
                dbPlayer.Team.SendMessageToAllState($"{await dbPlayer.GetNameAsync()} hat die Akte von {await suspect.GetNameAsync()} erlassen!");
                await db.SaveChangesAsync();
            }
        }

        [RemoteEvent]
        public async Task RemoveCrimeFrom(RXPlayer dbPlayer, int id, int crime)
        {

            if (dbPlayer == null || !dbPlayer.Team.IsState()) return;

         
                var suspect = await PlayerController.FindPlayerById(id);
                if (suspect == null) return;

            using var db = new RXContext();

            var allcrime = await db.PlayerCrimes.ToListAsync();

            DbPlayerCrimes crimePlayerReason = allcrime.Where(cpr => cpr.CrimeId == (uint)crime).FirstOrDefault();
                if (crimePlayerReason != null)
                {
                    db.PlayerCrimes.Remove(crimePlayerReason);
                    dbPlayer.Team.SendMessageToAllState($"{await dbPlayer.GetNameAsync()} hat die Akte von {await suspect.GetNameAsync()} bearbeitet!");

                await db.SaveChangesAsync();
            }
        }

        [RemoteEvent]
        public async Task GiveCrimeTo(RXPlayer dbPlayer, int pid, string crimes)
        {
     
            if (!dbPlayer.Team.IsState()) return;

            List<uint> crimesList = JsonConvert.DeserializeObject<List<uint>>(crimes);

            var suspect = await PlayerController.FindPlayerById(pid);
            if (suspect == null || crimesList == null || suspect.Injured) return;
            using var db = new RXContext();

            foreach (uint crime in crimesList)
            {
                var crimeModule = await db.NewCrimes.FirstOrDefaultAsync(x => x.i == crime);
                if (crimeModule == null) continue;

                var newCrime = new DbPlayerCrimes { CrimeId = crimeModule.i, PlayerId = suspect.Id, OfficerId = dbPlayer.Id, Uhrzeit = DateTime.Now.ToString("dd\\/MM\\/yyyy h\\:mm")                };
                await db.PlayerCrimes.AddAsync(newCrime);
            }

            await db.SaveChangesAsync();

            dbPlayer.Team.SendMessageToAllState($"{await dbPlayer.GetNameAsync()} hat {await suspect.GetNameAsync()} eine Akte angelegt!");
        }

    }
}
