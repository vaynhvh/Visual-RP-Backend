using Backend.Controllers;
using Backend.Modules.Bank;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using GTANetworkMethods;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Backend.Models.RXContainer;

namespace Backend.Models
{
    public class RXGarageSpawn
    {
        public uint Id { get; set; }
        public float Heading { get; set; }
        public Vector3 Position { get; set; }

        public RXGarageSpawn(uint id, float heading, Vector3 position)
        {
            Id = id;
            Heading = heading;
            Position = position;
        }
    }

    public class RXTeam
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; } = 0;
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; } = "";
        [JsonProperty(PropertyName = "s")]
        public string ShortName { get; set; } = "";
        [JsonIgnore]
        public uint Dimension { get; set; } = 0;
        [JsonIgnore]
        public Vector3 Spawn { get; set; } = new Vector3(0, 0, 0);
        [JsonIgnore]
        public Vector3 Storage { get; set; } = new Vector3(0, 0, 0);
        [JsonIgnore]
        public Vector3 Garage { get; set; } = new Vector3(0, 0, 0);
        [JsonIgnore]
        public Vector3 Wardrobe { get; set; } = new Vector3(0, 0, 0);
        [JsonIgnore]
        public Vector3 Armory { get; set; } = new Vector3(0, 0, 0);
        [JsonIgnore]
        public float ArmoryHeading { get; set; } = 0f;
        [JsonIgnore]
        public string ArmoryNPC { get; set; } = "a_m_m_acult_01";
        [JsonIgnore]
        public Vector3 ToggleDuty { get; set; } = new Vector3(0, 0, 0);

        [JsonIgnore]
        public Vector3 BankPosition { get; set; } = new Vector3(0, 0, 0);

        [JsonIgnore]
        public float ToggleDutyHeading { get; set; } = 0f;
        [JsonIgnore]
        public string ToggleDutyNPC { get; set; } = "a_m_m_acult_01";
        [JsonIgnore]
        public Vector3 GangwarEnter { get; set; } = new Vector3(0, 0, 0);
        [JsonIgnore]
        public List<RXGarageSpawn> GarageSpawns { get; set; } = new List<RXGarageSpawn>();
        [JsonIgnore]
        public RXGarageSpawn HelicopterSpawn { get; set; } = new RXGarageSpawn(0, 0, new Vector3());
        [JsonIgnore]
        public Dictionary<int, int> Salary { get; set; } = new Dictionary<int, int>();
        [JsonIgnore]
        public int MaxMembers { get => this.IsGangster() ? 20 : 40; }
        [JsonIgnore]
        public int BlipType { get; set; } = 0;
        [JsonIgnore]
        public int BlipColor { get; set; } = 0;
        [JsonIgnore]
        public int ColorId { get; set; } = 0;
        [JsonIgnore]
        public uint MedicPlayer { get; set; } = 0;
        [JsonIgnore]
        public string Image { get; set; } = "";
        [JsonIgnore]
        public bool HasDuty { get; set; } = false;
        public Color RGB { get; set; } = new Color(0, 0, 0);

        [JsonIgnore]
        public string MOTD { get; set; } = "Keine MOTD";

        [JsonIgnore]
        public TeamType Type { get; set; } = TeamType.Mafia;
        [JsonIgnore]
        public List<RXTeamCloth> TeamClothes { get; set; } = new List<RXTeamCloth>();
        [JsonIgnore]
        public List<RXTeamEquipItem> Equip { get; set; } = new List<RXTeamEquipItem>();

        [JsonIgnore]
        public WeaponHash NahKampfWeapon { get; set; }
        [JsonIgnore]
        public bool CanRegisterVehicles { get; set; } = false;
        [JsonIgnore]
        public RXContainerObj Container
        {
            get => ContainerModule.Containers.FirstOrDefault(x => x.Name == Name);
        }
        [JsonIgnore]
        public DbBankAccount BankAccount
        {
            get => BankModule.BankAccounts.FirstOrDefault(x => x.Name == ShortName);
        }
        [JsonIgnore]
        public string NPC { get; set; } = "";
        [JsonIgnore]
        public float NPCHeading { get; set; } = 0f;
        [JsonProperty(PropertyName = "image")]
        public string Logo { get; set; } = "";

        [JsonProperty(PropertyName = "c")]
        public string Hex { get; set; }

        public int GetMemberCount()
        {
            using var db = new RXContext();

            return db.Players.Where(x => x.TeamId == this.Id).Count();
        }
        public int GetOnlineMemberCount()
        {

            return PlayerController.GetValidPlayers().Where(x => x.TeamId == this.Id).Count();
        }
        public async void SendNotification(string message, int duration = 5000, int rang = 0, string title = "", string color = "")
        {
            var members = await NAPI.Task.RunReturnAsync(() => PlayerController.GetValidPlayers().Where(x => x.Teamrank >= rang && x.TeamId == this.Id));

            foreach (var player in members.ToList())
            {
                await player.SendNotify(message, duration, color == "" ? this.RGB.ConvertHTML() : color, title == "" ? this.Name : title);
            }
        }
        public async void SendMessageToLowerState(string message)
        {
            foreach (var team in PlayerController.GetValidPlayers().Where(x => x.Team.IsLowerState() == true))
            {

                await team.SendNotify(message, 10000, this.RGB.ConvertHTML(), this.Name);
            }
        }
        public async void SendMessageToAllState(string message)
        {
            foreach (var team in PlayerController.GetValidPlayers().Where(x => x.Team.IsState() == true))
            {

                await team.SendNotify(message, 10000, this.RGB.ConvertHTML(), this.Name);
            }
        }
        public async void SendMessageToDepartmentsInRange(string message, Vector3 distance, float dis)
        {
            foreach (var team in PlayerController.GetValidPlayers().Where(x => NAPI.Task.RunReturn(() => x.Position).DistanceTo(distance) < dis && x.Team.IsState() == true))
            {

                await team.SendNotify(message, 10000, this.RGB.ConvertHTML(), this.Name);
            }
        }
        

        public bool IsGangster() => this.Type == TeamType.Mafia || this.Type == TeamType.Gang;
        public bool IsState() => this.Type == TeamType.LSPD;
        public bool IsLowerState() => this.Type == TeamType.Medic || this.Type == TeamType.LSPD;
        public bool IsLowestState() => this.Type == TeamType.Medic || this.Type == TeamType.LSPD || this.Type == TeamType.DMV || this.Type == TeamType.DPOS;

        public bool IsNeutral() => this.Type == TeamType.DMV || this.Type == TeamType.DPOS;

    }
}
