using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("pta_settings")]
    public class DbPTASettings
    {
        [Key]
        public uint Id { get; set; }
        public DateTime PTAStart { get; set; }
        public DateTime PTAEnd { get; set; }
        public bool Active { get; set; }

    }
}
