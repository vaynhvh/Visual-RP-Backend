using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_lscbills")]
    public class DbCustomsBill
    {
        public uint id { get; set; }
        public uint tuner_id { get; set; }
        public uint amount { get; set; }
        public uint payerID { get; set; }
        public uint vehicleID { get; set; }
    }
}
