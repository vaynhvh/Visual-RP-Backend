using Backend.Models;
using Backend.Modules.Bank;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static Backend.Models.RXContainer;

namespace Backend.MySql.Models
{
    [Table("team")]
    public class DbTeam
    {

        [Key]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "s")]
        public string ShortName { get; set; }
        [JsonIgnore]
        public uint Dimension { get; set; }
        [JsonIgnore]
        public string Spawn { get; set; }
        [JsonIgnore]
        public string Storage { get; set; }
        [JsonIgnore]
        public string Garage { get; set; }
        [JsonIgnore]
        public string Armory { get; set; }
        [JsonIgnore]
        public float ArmoryHeading { get; set; }
        [JsonIgnore]
        public string ArmoryNPC { get; set; }
        [JsonIgnore]
        public string ToggleDuty { get; set; }
        [JsonIgnore]
        public float ToggleDutyHeading { get; set; }
        [JsonIgnore]
        public string ToggleDutyNPC { get; set; }
        [JsonIgnore]
        public string BankPosition { get; set; }
        [JsonIgnore]
        public string Wardrobe { get; set; }
        [JsonIgnore]
        public string GangwarEnter { get; set; }
        [JsonIgnore]
        public int MaxMembers { get; set; }
        [JsonIgnore]
        public int BlipType { get; set; } = 0;
        [JsonIgnore]
        public int BlipColor { get; set; } = 0;
        [JsonIgnore]
        public int ColorId { get; set; }
        [JsonIgnore]
        public uint MedicPlayer { get; set; } = 0;
        [JsonIgnore]
        public string Image { get; set; } = "";
        [JsonIgnore]
        public bool HasDuty { get; set; } = false;
        [JsonIgnore]
        public string MOTD { get; set; }
        [JsonIgnore]
        public uint Type { get; set; }
        public bool CanRegisterVehicles { get; set; } = false;
        public uint ContainerId { get; set; }
        public uint BankAccount { get; set; }
        [JsonIgnore]
        public string NPC { get; set; } = "";
        [JsonIgnore]
        public float NPCHeading { get; set; } = 0f;
        [JsonProperty(PropertyName = "image")]
        public string Logo { get; set; } = "";
        public string Hex { get; set; } = "";
        public int R { get; set; } = 0;
        public int G { get; set; } = 0;
        public int B { get; set; } = 0;

        public string NahkampfWeapon { get; set; }

    }
}
