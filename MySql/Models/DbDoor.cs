using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{

    [Table("doors")]
    public class DbDoor
    {

        [Key]
        public uint Id { get; set; }
        public string Position { get; set; }
        public long Model { get; set; }
        public string Teams { get; set; }
        public int RangRestriction { get; set; }
        public bool Locked { get; set; }
        public float Range { get; set; }
        public bool OpenWithWelding { get; set; }
        public bool OpenWithHacking { get; set; }
        public string PlayerIds { get; set; }

    }
}
