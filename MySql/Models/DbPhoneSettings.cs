using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("phone_settings")]
    public class DbPhoneSettings
    {
        [Key]
        public uint Id { get; set; }
        public uint PlayerId { get; set; }
        public bool FlyMode { get; set; } = false;
        public bool Mute { get; set; } = false;
        public bool DenyCalls { get; set; } = false;
        public string Wallpaper { get; set; } = "https://cdn.discordapp.com/attachments/1009462217856524390/1141282401771860008/1.png";
        public string Ringtone { get; set; } = "";
        public uint RingtoneVolume { get; set; } = 25;
        public bool InjuryStatus { get; set; } = false;
        public bool RadarActive { get; set; } = false;

    }
}
