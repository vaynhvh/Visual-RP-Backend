using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("blacklisted_identifiers")]
    public class DbIdentifier
    {
        [Key]
        public uint Id { get; set; }
        public string Identifier { get; set; }
    }
}
