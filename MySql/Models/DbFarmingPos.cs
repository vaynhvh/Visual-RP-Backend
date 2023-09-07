using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{

    [Table("farmingpos")]
    public class DbFarmingPos
    {
        [Key]
        public uint Id { get; set; }
        public uint FarmingId { get; set; }
        public string Position { get; set; }    
    }
}
