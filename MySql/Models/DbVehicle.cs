using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_vehicles")]
    public class DbVehicle
    {
        [Key]
        public uint Id { get; set; }
        public string Hash { get; set; } = "";
        public bool Stored { get; set; } = true;
        public uint GarageId { get; set; } = 1;
        public uint ContainerId { get; set; } = 0;
        public uint OwnerId { get; set; } = 0;
        public uint ModelId { get; set; } = 0;
        public string Plate { get; set; } = "";
        public string Position { get; set; } = "0,0,0";
        public string Rotation { get; set; } = "0,0,0";
        public int R { get; set; } = 0;
        public int G { get; set; } = 0;
        public int B { get; set; } = 0;
        public string Tuning { get; set; } = "{}";
        public string VehicleKeys { get; set; } = "[]";
        public double Distance { get; set; } = 0;
        public double Fuel { get; set; } = 100;
        public bool Registered { get; set; } = false;
        public bool Fav { get; set; } = false;
        public DateTime LastMoved { get; set; } = DateTime.Now.AddDays(-3);
    }
}
