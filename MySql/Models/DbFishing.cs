using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Backend.Models;
using Backend.Utils.Extensions;

namespace Backend.MySql.Models
{
    [Table("fishing")]
    public class DbFishing
    {
        [Key]
        public uint Id { get; set; }
        public string Position { get; set; }
        public float Heading { get; set; }

        [NotMapped]
        public bool InUse { get; set; } = false;

        [NotMapped]
        public MCB MCB { get; set; }

        [NotMapped]
        public uint Player { get; set; }

        [NotMapped]
        public DateTime LastCatch { get; set; }
    }
}
