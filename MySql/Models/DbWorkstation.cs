using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{

    [Table("workstations")]
    public class DbWorkstation
    {

        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint InputItemId { get; set; }
        public uint InputItemAmount { get; set; }
        public uint OutputItemId { get; set; }
        public uint OutputItemAmount { get; set; }
        public int RentPrice { get; set; }
        public string RentPosition { get; set; }
        public string RentPed { get; set; } 
        public float RentPedHeading { get; set; }
        public string InputPosition { get; set; }
        public string OutputPosition { get; set; }
        public int TimeType { get; set; }
        public bool Illegal { get; set; }

    }
}
