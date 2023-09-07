using Backend.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{

    [Table("jumppoints")]
    public class DbJumppoint
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string Position { get; set; }
        public string SecondPosition { get; set; }
        public string Teams { get; set; }
        public uint Dimension { get; set; }
        public uint SecondDimension { get; set; }
        public bool FloorObject { get; set; }
        public bool WithVehicle { get; set; }
        public float Range { get; set; }
        public float SecondRange { get; set; }

        [NotMapped]
        public bool Locked { get; set; } = true;

    }
}
