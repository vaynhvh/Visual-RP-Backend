using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_opencrimes")]
    public class DbPlayerCrimes
    {
        [Key]
        public uint Id { get; set; }
        public uint PlayerId { get; set; }
        public uint OfficerId { get; set; }
        public string Uhrzeit { get; set; } 
        public uint CrimeId { get; set; }
    }
}
