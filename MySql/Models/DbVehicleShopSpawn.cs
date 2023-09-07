using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("vehicleshopspawns")]
    public class DbVehicleShopSpawn
    {
        [Key]
        public uint Id { get; set; }
        public uint VehShopId { get; set; }
        public float Heading { get; set; }
        public string Position { get; set; }
    }
}
