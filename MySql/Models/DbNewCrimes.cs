using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("newcrimes")]
    public class DbNewCrimes
    {
        [Key]
        public uint i { get; set; }
        public string n { get; set; }
        public int p { get; set; }
        public int j { get; set; }

    }
}
