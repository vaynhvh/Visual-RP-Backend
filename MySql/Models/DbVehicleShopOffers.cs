using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("vehicleshopoffers")]
    public class DbVehicleShopOffers
    {
        [Key]
        public uint Id { get; set; }
        public uint VehShopId { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public int Price { get; set; }
        public string Position { get; set; }
        public float Heading { get; set; }
        public bool Live { get; set; } = false;
    }
}
