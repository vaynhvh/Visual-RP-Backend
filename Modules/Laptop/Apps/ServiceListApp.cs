using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Faction;
using Backend.Modules.Leitstellen;
using Backend.Modules.Phone;
using Backend.MySql.Models;
using Backend.Utils;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Laptop.Apps
{


    public class serviceObject
    {
        [JsonProperty(PropertyName = "i")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "a")]
        public string bearbeiter { get; set; }

        [JsonProperty(PropertyName = "m")]
        public string message { get; set; }

        [JsonProperty(PropertyName = "d")]
        public string datum { get; set; }

        [JsonProperty(PropertyName = "x")]
        public float posX { get; set; }

        [JsonProperty(PropertyName = "y")]
        public float posY { get; set; }

        [JsonProperty(PropertyName = "z")]
        public float posZ { get; set; }

        [JsonProperty(PropertyName = "accepted")]
        public string accepted { get; set; }
        [JsonProperty(PropertyName = "telnr")]
        public string telnr { get; set; }
    }
    public class Service
    {
        public Vector3 Position { get; }
        public string Message { get; set; }
        public uint TeamId { get; }
        public RXPlayer Player { get; }
        public HashSet<string> Accepted { get; }
        public string Telnr { get; }

        public DateTime Created { get; set; }

        public Service(Vector3 position, string message, uint teamId, RXPlayer iPlayer, string description = "", string telefon = "0")
        {
            Position = position;
            Message = message;
            TeamId = teamId;
            Player = iPlayer;
            Telnr = telefon;
            Accepted = new HashSet<string>();
            Created = DateTime.Now;
        }
    }

    public class ServiceEvaluation
    {
        public uint id { get; set; }

        public string name { get; set; }

        public int amount { get; set; }

        public DateTime timestr { get; set; }

        public ServiceEvaluation(uint PlayerId, string name, int Amount)
        {
            string nstr = "";
            string pName = name;
            if (pName != null)
            {
                nstr = pName;
            }
            id = PlayerId;
            name = nstr;
            amount = Amount;
            timestr = DateTime.Now;
        }
    }

    public class ServiceEvaluationJson
    {
        public uint id { get; set; }

        public string name { get; set; }

        public int amount { get; set; }

        public string timestr { get; set; }

    }
    class ServiceListApp : RXModule
    {

        public ServiceListApp() : base("ServiceListApp") { }

        public static Dictionary<uint, List<Service>> serviceList = new Dictionary<uint, List<Service>>();

        public static Dictionary<uint, List<ServiceEvaluation>> evaluations = new Dictionary<uint, List<ServiceEvaluation>>();

        public override void LoadAsync()
        {
            serviceList = new Dictionary<uint, List<Service>>();
            evaluations = new Dictionary<uint, List<ServiceEvaluation>>();
        }

        public async Task AddForEvaluation(RXPlayer dbPlayer)
        {
            try
            {
                if (evaluations.ContainsKey(dbPlayer.TeamId))
                {
                    List<ServiceEvaluation> evals = evaluations[dbPlayer.TeamId].ToList();

                    ServiceEvaluation eval = evals.Where(e => e.id == dbPlayer.Id).FirstOrDefault();
                    if (eval != null)
                    {
                        eval.amount++;
                        eval.timestr = DateTime.Now;
                    }
                    else
                    {
                        evals.Add(new ServiceEvaluation(dbPlayer.Id, await dbPlayer.GetNameAsync(), 1));
                    }
                    evaluations[dbPlayer.TeamId] = evals;
                }
                else
                {
                    evaluations.Add(dbPlayer.TeamId, new List<ServiceEvaluation>() { new ServiceEvaluation(dbPlayer.Id, await dbPlayer.GetNameAsync(), 1) });
                }
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        public static async Task<bool> IsServiceInRangeOfTeam(uint teamid, Vector3 Position)
        {

            List<Service> teamServices = await GetServicesForTeam(teamid);

            if (teamServices != null && teamServices.Count > 0 && teamServices.Where(s => s.Position.DistanceTo(Position) < 10.0f).Count() > 0)
            {
                return true;
            }

            return false;
        }

        public static async Task<List<Service>> GetServicesForTeam(uint teamId)
        {
            // CopSpecials

            List<Service> teamServices;

            if (!serviceList.ContainsKey(teamId))
            {
                teamServices = new List<Service>();
                serviceList.Add(teamId, teamServices);
            }
            else
            {
                teamServices = serviceList[teamId];
            }

            return teamServices;
        }

        public async Task<List<Service>> GetAvailableServices(RXPlayer iPlayer)
        {
            List<Service> teamServices = await GetServicesForTeam(iPlayer.TeamId);


            return teamServices.Where(service => service.Accepted.Count() == 0).ToList();
        }

        public static async Task<List<Service>> GetCreatedServices(RXPlayer iPlayer)
        {
            try
            {
                List<Service> returnServices = new List<Service>();

                foreach (KeyValuePair<uint, List<Service>> kvp in serviceList)
                {
                    if (kvp.Value != null && kvp.Value.Count > 0)
                    {
                        foreach (Service service in kvp.Value.ToList().Where(s => s.Player.Id == iPlayer.Id))
                        {
                            returnServices.Add(service);
                        }
                    }
                }

                return returnServices;
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
                return new List<Service>();
            }
        }

        public async Task<List<Service>> GetAcceptedTeamServices(RXPlayer iPlayer)
        {
            List<Service> teamServices = await GetServicesForTeam(iPlayer.TeamId);

            return teamServices.Where(service => service.Accepted.Count() > 0).ToList();
        }

        public async Task<List<Service>> GetAcceptedServices(RXPlayer iPlayer)
        {
            List<Service> teamServices = await GetServicesForTeam(iPlayer.TeamId);

            string name = await iPlayer.GetNameAsync();

            return teamServices.Where(service => service.Accepted.Contains(name)).ToList();
        }

        public override async Task OnMinute()
        {
            if (!serviceList.ContainsKey((int)3)) return;
            foreach (Service service in serviceList[(int)3].Where(s => s.Player != null && s.Player.Injured))
            {
                string optional = "";

                if (service.Player.TeamId == (int)3)
                {
                    optional = "[LSMC]";
                    service.Message = $"{optional} Verletzung - {service.Player.DeathData.DeathTime.ToString()} Min";
                }
            }
        }

        /*
        public void RemoveInjuredPlayerService(DbPlayer dbPlayer)
        {
            if (!serviceList.ContainsKey((int)teams.TEAM_MEDIC)) return;
            Instance.CancelOwnService(dbPlayer, (uint)teams.TEAM_MEDIC);
            dbPlayer.ResetData("service");
        }*/

        public static async Task<bool> Add(RXPlayer iPlayer, uint teamId, Service service)
        {
            var teamServices = await GetServicesForTeam(teamId);
            foreach (var itr in teamServices)
            {
                if (itr.Player == null)
                    continue;

                if (itr.Player.Id == iPlayer.Id)
                    return false;
            }

            teamServices.Add(service);
            return true;
        }

        public async Task<string> GetSpecialDescriptionForPlayer(RXPlayer iPlayer, Service service)
        {
            string desc = "[" + Convert.ToInt32(service.Position.DistanceTo(await iPlayer.GetPositionAsync())) + "m - gesendet: " + service.Created.ToString("HH:mm:ss") + "]";
            

            desc += service.Message;

            return desc;
        }

        public async Task<bool> Accept(RXPlayer iPlayer, RXPlayer destinationPlayer)
        {
            var createdService = await GetCreatedServices(destinationPlayer);
            if (createdService == null || createdService.Count() <= 0) return false;
            uint playerTeam = iPlayer.TeamId;
            string name = await iPlayer.GetNameAsync();

            if (playerTeam != createdService[0].TeamId) return false;
            if (createdService[0].Accepted.Contains(name)) return false;

            bool status = createdService[0].Accepted.Add(name);

            // add evaluation app data
            await AddForEvaluation(iPlayer);


            return status;
        }
    

        public static async Task<bool> CancelOwnService(RXPlayer iPlayer, uint teamId)
        {
            var teamServices = await GetServicesForTeam(teamId);
            if (teamServices.Count == 0) return false;

            var createdService = await GetCreatedServices(iPlayer);
            if (createdService.Count <= 0) return false;

            if (!teamServices.Contains(createdService[0])) return false;
            bool status = teamServices.Remove(createdService[0]);
            return status;
        }

        public async Task<bool> Cancel(RXPlayer iPlayer, RXPlayer player, uint teamId)
        {
            var teamServices = await GetServicesForTeam(teamId);
            if (teamServices.Count == 0) return false;

            var createdService = await GetCreatedServices(player);
            if (createdService == null || createdService.Count() <= 0) return false;

            bool status = teamServices.Remove(createdService[0]);
            return status;
        }

        public override async Task OnPlayerDisconnect(RXPlayer player, DisconnectionType type, string reason)
        {
            if (player.Injured)
            {
                await CancelOwnService(player, 3);
            }
        }

        [RemoteEvent]
        public async Task Services(RXPlayer dbPlayer)
        {
            if (dbPlayer == null) return;
            if (!dbPlayer.Team.IsLowestState()) return;

            List<serviceObject> serviceList = new List<serviceObject>();
            List<serviceObject> serviceListplayer = new List<serviceObject>();

            var teamServices = await GetAvailableServices(dbPlayer);

            foreach (var service in teamServices)
            {
                string accepted = string.Join(',', service.Accepted);

                if (service.Player == null) continue;

                string varname = await service.Player.GetNameAsync();

                if (dbPlayer.TeamId == 3)
                {
                    if (service.Player.Team.IsLowestState())
                    {
                        varname = "[STATE]";
                    }
                    else varname = "Verletzte Person";

                  
                    varname = varname + " (" + await service.Player.GetNameAsync() + ")";
                    
                }

                serviceList.Add(new serviceObject() { id = (int)service.Player.Id, bearbeiter = "Kein Bearbeiter", datum = service.Created.ToString("hh:mm dd:MM:yyyy"), name = varname, message = await GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
            }

            var playerservices = await GetAcceptedServices(dbPlayer);


            foreach (var service in playerservices)
            {
                string accepted = string.Join(',', service.Accepted);

                if (service.Player == null) continue;

                string varname = await service.Player.GetNameAsync();

                if (dbPlayer.TeamId == 3)
                {
                    if (service.Player.Team.IsLowestState())
                    {
                        varname = "[STATE]";
                    }
                    else varname = "Verletzte Person";


                    varname = varname + " (" + await service.Player.GetNameAsync() + ")";

                }

                serviceListplayer.Add(new serviceObject() { id = (int)service.Player.Id, bearbeiter = service.TeamId.ToString(), datum = service.Created.ToString("dd:MM:yyyy hh:mm"), name = varname, message = await GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
            }

            var window = new RXWindow("ServiceList");


            object data = new
            {
                oo = LeitstellenModule.IsLeiststelle(dbPlayer),
                ad = serviceListplayer,
                d = serviceList,
                t = TeamModule.Teams.Where(x => x.Id == dbPlayer.Team.Id),
            };

            await window.OpenWindow(dbPlayer, data);
        }

        [RemoteEvent]
        public async Task AssignService(RXPlayer dbPlayer, int playerId)
        {
            if (dbPlayer == null) return;
            if (!dbPlayer.Team.IsLowestState()) return;

            var findplayer = await PlayerController.FindPlayerById(playerId);
            if (findplayer == null) return;

            bool response = await Accept(dbPlayer, findplayer);

            await dbPlayer.SendNotify(response ? "Du hast einen Service angenommen!" : "Der Service konnte nicht angenommen werden!");
            await findplayer.SendNotify("Dein Service wurde angenommen!");

            if (dbPlayer.TeamId == 3)
            {
                string optional = "";



                dbPlayer.Team.SendNotification($"{await dbPlayer.GetNameAsync()} hat einen {optional} Service angenommen");
            }
            else dbPlayer.Team.SendNotification($"{await dbPlayer.GetNameAsync()} hat den Service von {await findplayer.GetNameAsync()} angenommen");
        }


        [RemoteEvent]
        public async Task GetAssignedServices(RXPlayer dbPlayer)
        {
            if (dbPlayer == null) return;
            if (!dbPlayer.Team.IsLowestState() ) return;

            List<serviceObject> serviceList = new List<serviceObject>();
            var acceptedServices = await GetAcceptedServices(dbPlayer);

            foreach (var service in acceptedServices)
            {
                string accepted = string.Join(',', service.Accepted);

                string varname = await service.Player.GetNameAsync();

                if (dbPlayer.TeamId == 3)
                {
                    if (service.Player.TeamId == 3)
                    {
                        varname = "[LSMC]";
                    }
                    else varname = "Verletzte Person";

                    varname = varname + " (" + await service.Player.GetNameAsync() + ")";
                    
                }

                serviceList.Add(new serviceObject() { id = (int)service.Player.Id, name = varname, message = await GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
            }

            var serviceJson = NAPI.Util.ToJson(serviceList);
            await dbPlayer.TriggerEventAsync("SendAssignedServices", serviceJson);
        }

        [RemoteEvent]
        public async Task DeleteService(RXPlayer dbPlayer, int creatorId)
        {
            if (dbPlayer == null) return;
            if (!dbPlayer.Team.IsLowestState() ) return;

            var findplayer = await PlayerController.FindPlayerById(creatorId);
            if (findplayer == null) return;

            bool response = await Cancel(dbPlayer, findplayer, dbPlayer.TeamId);

            if (response)
            {
                findplayer.ResetData("service");

                await findplayer.SendNotify("Dein Service wurde geschlossen!");
                await dbPlayer.SendNotify($"Du hast einen Service geschlossen!");
            }
            else
            {
                await dbPlayer.SendNotify("Der Service konnte nicht beendet werden!");
            }
        }

        [RemoteEvent]
        public async Task GetAllServices(RXPlayer dbPlayer)
        {
            if (dbPlayer == null) return;
            if (!dbPlayer.Team.IsLowestState() ) return;

            List<serviceObject> serviceList = new List<serviceObject>();
            var teamServices = await GetAcceptedTeamServices(dbPlayer);

            foreach (var service in teamServices)
            {
                string accepted = string.Join(',', service.Accepted);

                string varname = await service.Player.GetNameAsync();

                if (dbPlayer.TeamId == 3)
                {
                   
                    if (service.Player.TeamId == 3)
                    {
                        varname = "[LSMC]";
                    }
                    else varname = "Verletzte Person";

                    varname = varname + " (" + await service.Player.GetNameAsync() + ")";
                    
                }

                serviceList.Add(new serviceObject() { id = (int)service.Player.Id, name = varname, message = await GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
            }

            var serviceJson = NAPI.Util.ToJson(serviceList);
            await dbPlayer.TriggerEventAsync("SendAllServices", serviceJson);

        }

        [RemoteEvent]
        public async Task requestEvalutionServices(RXPlayer dbPlayer)
        {
            if (dbPlayer == null) return;
            if (!dbPlayer.Team.IsLowestState() ) return;

            var teamRankPermission = dbPlayer.TeamMemberData.Manage;
            if (teamRankPermission == false) return;


            if (!ServiceListApp.evaluations.ContainsKey(dbPlayer.TeamId)) return;

            List<ServiceEvaluation> evaluations = ServiceListApp.evaluations[dbPlayer.TeamId].ToList();

            List<ServiceEvaluationJson> jsonData = new List<ServiceEvaluationJson>();

            foreach (ServiceEvaluation eval in evaluations)
            {
                jsonData.Add(new ServiceEvaluationJson()
                {
                    id = eval.id,
                    amount = eval.amount,
                    name = eval.name,
                    timestr = eval.timestr.ToString("yyyy-MM-dd H:mm:ss")
                });
            }

            var serviceJson = NAPI.Util.ToJson(jsonData);
            await dbPlayer.TriggerEventAsync("componentServerEvent", "ServiceEvaluationApp", "responseEvaluationService", serviceJson);

        }
    }
}
