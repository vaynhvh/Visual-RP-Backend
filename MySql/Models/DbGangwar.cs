using Backend.Models;
using Backend.Modules.Gangwar;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Backend.MySql.Models
{
    [Table("gangwar")]
    public class DbGangwar
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint TeamId { get; set; }
        public uint AttackerId { get; set; }
        public string Position { get; set; } = "0,0,0";
        public string AttackerPosition { get; set; } = "0,0,0";
        public string DefenderPosition { get; set; } = "0,0,0";
        public string Flag1 { get; set; } = "0,0,0";
        public string Flag2 { get; set; } = "0,0,0";
        public string Flag3 { get; set; } = "0,0,0";
        public string AttackerAusparker { get; set; } = "0,0,0";
        public float AttackerAusparkerRotation { get; set; }
        public string AttackerVehSpawn { get; set; } = "0,0,0";
        public float AttackerVehSpawnRotation { get; set; }
        public string DefenderAusparker { get; set; } = "0,0,0";
        public float DefenderAusparkerRotation { get; set; }
        public string DefenderVehSpawn { get; set; } = "0,0,0";
        public float DefenderVehSpawnRotation { get; set; }
        public float Size { get; set; }
        public uint Dimension { get; set; }
        public uint Flagcount { get; set; }
        public DateTime LastAttacked { get; set; } = DateTime.Now;

        [NotMapped]
        public int AttackerPoints { get; set; } = 0;
        [NotMapped]
        public int DefenderPoints { get; set; } = 0;

        [NotMapped]
        public int Flag1Team { get; set; } = 0;
        [NotMapped]
        public int Flag2Team { get; set; } = 0;
        [NotMapped]
        public int Flag3Team { get; set; } = 0;
        [NotMapped]
        public int AttackerFlags { get; set; } = 0;
        [NotMapped]
        public int DefenderFlags { get; set; } = 0;
        [NotMapped]
        public bool IsGettingAttacked { get; set; } = false;
        [NotMapped]
        public List<RXGangwarStats> GangwarStats { get; set; }
        [NotMapped]
        public Marker GangwarMarker { get; set; }

        [NotMapped]
        public Blip GangwarBlip { get; set; }

        [NotMapped]
        public GangwarWeaponPack weaponPack { get; set; }


        public DbGangwar(uint id, string name, string position, uint teamId, float size, DateTime lastAttacked)
        {
            Id = id;
            Name = name;
            Position = position;
            TeamId = teamId;
            Size = size;
            LastAttacked = lastAttacked;
            AttackerFlags = 0;
            DefenderFlags = 0;
        }

        public async Task UpdateGangwarHud()
        {

            var gangwar = GangwarModule.Gangwars.Find(x => x.Id == this.Id);

            if (gangwar == null) return;

            var gwplayers = GangwarModule.gangwarPlayers.Where(x => x.Value.Id == this.Id).ToList();
            
            foreach (var player in gwplayers)
            {
                await player.Key.TriggerEventAsync("UpdateGangwarHud", gangwar.AttackerPoints, gangwar.DefenderPoints, gangwar.AttackerFlags, gangwar.DefenderFlags);
            }

        }
    }
}
