using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("paintballmaps_spawns")]
    public class DbPaintballSpawnpoints
    {
        [Key]
        public uint Id { get; set; }
        public uint MapId { get; set; }
        public string Position { get; set; } = "0,0,0";

    }
}
