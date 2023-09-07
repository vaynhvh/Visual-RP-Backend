using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("team_garagespawns")]
    public class DbTeamGaragePoints
    {
        [Key]
        public uint id { get; set; }
        public uint teamid { get; set; }
        public string position { get; set; } = "0, 0, 0";
        public float heading { get; set; }

    }
}
