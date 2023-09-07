using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Modules.Vehicle
{
    public class GTAAPI
    {
        public GTAAPIImages images { get; set; }
        public string manufacturer { get; set; }
        public string model { get; set; }
        public int seats { get; set; }
        public int price { get; set; }
        public GTAAPISpeed topSpeed { get; set; }
        public double speed { get; set; }
        public double acceleration { get; set; }
        public double braking { get; set; }
        public double handling { get; set; }
    }

    public class GTAAPISpeed
    {
        public int mph { get; set; }
        public int kmh { get; set; }

    }
    public class GTAAPIImages
    {
        public string frontQuarter { get; set; }
        public string rearQuarter { get; set; }
        public string front { get; set; }
        public string rear { get; set; }
        public string side { get; set; }
    }
}
