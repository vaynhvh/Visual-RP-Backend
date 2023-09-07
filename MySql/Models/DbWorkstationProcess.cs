using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("workstation_process")]
    public class DbWorkstationProcess
    {

        [Key]
        public uint Id { get; set; }
        public uint PlayerId { get; set; }
        public uint WorkstationId { get; set; }
        public uint InputItemId { get; set; }
        public uint InputItemAmount { get; set; }
        public uint OutputItemId { get; set; }
        public uint OutputItemAmount { get; set; }
        public DateTime StartTime { get; set; }
        public bool Finished { get; set; }
    }
}
