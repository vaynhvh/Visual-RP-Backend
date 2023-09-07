using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("metalldetector")]
    public class DbMetallDetector
    {
        [Key]
        public uint Id { get; set; }
        public string Position { get; set; }
        public float Range { get; set; }
        public DateTime LastDetected { get; set; } = DateTime.Now;
    }
}
