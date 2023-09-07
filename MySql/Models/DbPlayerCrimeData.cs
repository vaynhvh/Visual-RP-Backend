using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_crimedata")]
    public class DbPlayerCrimeData
    {
        [Key]
        public uint Id { get; set; }
        public uint PlayerId { get; set; } 
        public string Address { get; set; }
        public string Membership { get; set; }
        public string Phone { get; set; }
        public string Info { get; set; }
        public bool CanAktenView { get; set; }
        public string Note { get; set; }


    }
}
