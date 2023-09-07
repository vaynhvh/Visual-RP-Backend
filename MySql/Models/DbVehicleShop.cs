using Backend.Modules.Faction;
using Backend.Modules.Vehicle;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("vehicleshop")]
    public class DbVehicleShop
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string NPCHash { get; set; }
        public float NPCHeading { get; set; }
        public bool TeamShop { get; set; }
        public int Teams { get; set; }
    }
}
