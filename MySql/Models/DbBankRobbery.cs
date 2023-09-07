using Backend.Models;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{

    [Table("scenario_bankrobbery")]
    public class DbBankRobbery
    {

        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        public string StartPosition { get; set; }
        public string HackingPosition { get; set; }
        public string WeldingPosition { get; set; }
        public string DrillPosition { get; set; }
        public string ExplodePosition { get; set; }
        public string FluchttunnelPosition { get; set; }
        public bool TeamRestricted { get; set; }
        public int TeamCountMinOnline { get; set; }

        [NotMapped]
        public RXTeam RobbingTeam { get; set; }
        [NotMapped]
        public bool Active { get; set; } = false;
        [NotMapped]
        public DateTime LastRobbedTime { get; set; } = DateTime.Now;
        [NotMapped]
        public bool HackingDone { get; set; } = false;
        [NotMapped]
        public bool WeldingDone { get; set; } = false;
        [NotMapped]
        public bool DrillDone { get; set; } = false; 
        [NotMapped]
        public bool ExplodeDone { get; set; } = false;

    }
}
