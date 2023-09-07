using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{

    [Table("farming")]
    public class DbFarming
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ItemId { get; set; }
        public int RequiredItemId { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public float Range { get; set; }
        public uint RestrictedToTeam { get; set; } = 0;
        public bool OnlyBadFaction { get; set; } = false;
    }
}
