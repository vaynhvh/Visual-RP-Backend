using Backend.Modules.Farming;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_plants")]
    public class DbPlant
    {
        [Key]
        public uint Id { get; set; }
        public uint OwnerId { get; set; } = 0;
        public DateTime PlantTime { get; set; } = DateTime.Now;
        public bool Watered { get; set; } = false; 
        public int Type { get; set; } = 0;
        public string Position { get; set; } = "0,0,0";

        [NotMapped]
        public PlantType PlantType => (PlantType)this.Type;
    }
}
