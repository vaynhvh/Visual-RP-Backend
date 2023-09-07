using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_crimes")]
    public class DbCrimes
    {
        [Key]
        public uint Id { get; set; }
        public string Absatz { get; set; }
        public string Grund { get; set; }
        public int Bussgeld { get; set; }
        public int Haftzeit { get; set; }
        public int CatId { get; set; }

    }
}
