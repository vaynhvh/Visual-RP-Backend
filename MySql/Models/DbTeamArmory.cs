using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("team_armory")]
    public class DbTeamArmory
    {
        [Key]
        public uint Id { get; set; }
        public uint TeamId { get; set; }
        public uint ItemId { get; set; }
        public uint Amount { get; set; }
        public uint Price { get; set; }

    }
}
