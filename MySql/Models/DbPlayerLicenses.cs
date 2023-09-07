using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_licenses")]
    public class DbPlayerLicenses
    {
        [Key]
        public uint Id { get; set; }
        public uint PlayerId { get; set; }
        public uint LicenseId { get; set; }
        public uint SignerId { get; set; }
        public DateTime DateOfSign { get; set; }
    }
}
