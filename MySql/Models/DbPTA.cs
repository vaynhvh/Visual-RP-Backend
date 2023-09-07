using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("pta_logs")]
    public class DbPTA
    {
        [Key]
        public uint Id { get; set; }
        public string Username { get; set; }
        public string Teamname { get; set; }
        public string UserDiscord { get; set; }
        public string TeamDiscord { get; set; }
        public uint Points { get; set; }
        public string Reason { get; set; }
        public DateTime Date { get; set; }
    }
}
