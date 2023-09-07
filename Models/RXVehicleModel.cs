using Backend.Modules.Vehicle;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.Models
{
    public class RXVehicleModel
    {
        public uint Id { get; set; }
        public string Hash { get; set; }
        public string Name { get; set; }
        public float Multiplier { get; set; }
        public int Fuel { get; set; }
        public float FuelConsumption { get; set; }
        public VehicleClassificationTypes Classification { get; set; }
        public int InventorySize { get; set; }
        public int InventoryWeight { get; set; }
        public int Seats { get; set; }
        public int MaxKMH { get; set; }
        public string Type { get; set; }
    }
}
