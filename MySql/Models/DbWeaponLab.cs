using Backend.Models;
using Backend.Modules.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using static Backend.Models.RXContainer;

namespace Backend.MySql.Models
{
    [Table("weaponlab")]
    public class DbWeaponLab
    {
        [Key]
        public uint Id { get; set; }
        public uint TeamId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public uint FuelContainerId { get; set; }

        [NotMapped]
        public List<RXPlayer> ActivePlayers { get; set; } = new List<RXPlayer>();
        [NotMapped]
        public bool HackInProgress { get; set; } = false;
        [NotMapped]
        public bool FriskInProgress { get; set; } = false;
        [NotMapped]
        public bool Locked { get; set; } = true;
        [NotMapped]
        public bool ImpoundInProgress { get; set; } = false;
        [NotMapped]
        public bool LaborMemberCheckedOnHack { get; set; } = false;
        [NotMapped]
        public bool HasDefended { get; set; } = false;
        [NotMapped]
        public DateTime LastAttacked { get; set; } = DateTime.Now;

        [NotMapped]
        public RXContainerObj LabFuelContainer
        {
            get => ContainerModule.Containers.FirstOrDefault(s => s.Id == this.FuelContainerId);
        }

    }
}
