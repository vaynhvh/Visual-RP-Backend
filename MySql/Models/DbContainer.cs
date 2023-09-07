using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("containers")]
    public class DbContainer
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public int MaxWeight { get; set; } = 0;
        public int MaxSlots { get; set; } = 0;
    }
}
