using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{

    [Table("logs")]
    public class DbLog
    {

        [Key]
        public uint Id { get; set; }   
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public uint Type { get; set; }
    }
}
