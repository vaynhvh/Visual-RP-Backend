using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("paintballmaps")]
    public class DbPaintball
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }

    }
}
