using Backend.Models;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("blitzer")]
    public class DbBlitzer
    {
        [Key]
        public uint Id { get; set; }
        public float Range { get; set; }
        public string RawPosition { get; set; }
        public string RawObjectPosition { get; set; }
        public float Heading { get; set; }
        public int SpeedLimit { get; set; }
        public bool Active { get; set; }

        [NotMapped]
        public RXColShape Shape { get; set; }
        [NotMapped]
        public Vector3 Position { get; set; }
        [NotMapped]
        public Vector3 ObjectPosition { get; set; }
        [NotMapped]
        public int Tolleranz { get; set; }
        [NotMapped]
        public bool Hacked { get; set; } = false;
        [NotMapped]
        public DateTime LastHacked { get; set; } = DateTime.Now.AddMinutes(-30);


    }
}
