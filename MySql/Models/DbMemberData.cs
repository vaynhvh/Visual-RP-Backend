using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_team_data")]
    public class DbTeamMemberData
    {
        [Key]
        public uint Id { get; set; }
        public uint PlayerId { get; set; }
        public bool Manage { get; set; } = false;
        public bool Bank { get; set; } = false;
        public uint Dienstnummer { get; set; } = 0;
        public bool Inventory { get; set; } = false;
        public string Title { get; set; } = ""; 
    }

    [Table("player_business_data")]
    public class DbBusinessMemberData
    {
        [Key]
        public uint Id { get; set; }
        public uint PlayerId { get; set; }
        public bool Manage { get; set; } = false;
        public bool Bank { get; set; } = false;
        public int Salary { get; set; } = 0;
    }
}
