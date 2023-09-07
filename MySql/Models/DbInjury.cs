using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{

    [Table("injury_pos")]
    public class DbInjury
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string KHPosition { get; set; }
        public uint KHDimension { get; set; }
        public bool IsBadFrak { get; set; }
        //public object TTPosition { get; internal set; }
    }
}
