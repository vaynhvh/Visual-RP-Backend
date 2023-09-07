using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("houses")]
    public class DbHouse
    {
        [Key]
        public uint id { get; set; }
        public uint type { get; set; }
        public uint price { get; set; }
        public uint slots { get; set; }
        public uint weight { get; set; }
        public uint interiorid { get; set; }
        public uint ownerID { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
        public float heading { get; set; }
        public uint maxrents { get; set; }
        public uint inv_cash { get; set; }
        public uint keller { get; set; }
        public uint moneykeller { get; set; }
        public uint garage { get; set; }
        public string show_phonenumber { get; set; }
        public uint bl_amount { get; set; }
        public uint container_id { get; set; }
        public string rents { get; set; }
        public uint money { get; set; }
        public uint werkbank { get; set; }
        public uint server { get; set; }
        public string note { get; set; }
        public uint kellerlevel { get; set; }

        public uint rentprice { get; set; }


        [NotMapped]
        public bool Locked { get; set; } = true;
        [NotMapped]
        public List<uint> RentList { get; set; }


    }
}
