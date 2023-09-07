using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("team_vehicles")]
    public class DbTeamVehicle
    {
        [Key]
        public uint Id { get; set; }
        public string Hash { get; set; } = "";
        public bool Stored { get; set; } = true;
        public uint ContainerId { get; set; } = 0;
        public uint TeamId { get; set; } = 0;
        public string Position { get; set; } = "0,0,0";
        public string Rotation { get; set; } = "0,0,0";
        public int R { get; set; } = 0;
        public int G { get; set; } = 0;
        public int B { get; set; } = 0;
        public int Livery { get; set; } = 0;
        public string Tuning { get; set; } = "{}";
        public double Distance { get; set; } = 0;
        public double Fuel { get; set; } = 100;
        public DateTime LastMoved { get; set; } = DateTime.Now.AddDays(-3);

    }
}
