using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_xmas")]
    public class DbPlayerXMAS
    {
        [Key]
        public uint Id { get; set; }
        public uint ForumId { get; set; }
        public uint GiftType { get; set; }
        public uint Day { get; set; }
        public uint UsedIngame { get; set; }   

    }
}
