using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{

    [Table("farmingprocess")]
    public class DbFarmingProcess
    {
        [Key]
        public uint Id { get; set; }    
        public string Name { get; set; }
        public uint RequiredItemId { get; set; }    
        public uint ProcessItemId { get; set; }
        public string Position { get; set; }
        public string PedHash { get; set; } 
        public float PedRotation { get; set; }
        public uint Time { get; set; }  
        public uint MinCount { get; set; }
        public uint MaxCount { get; set; }
        public bool Illegal { get; set; }
        public bool RestrictedForTeam { get; set; }
        public int TeamType { get; set; }
    }
}
