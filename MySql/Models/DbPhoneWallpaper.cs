using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Backend.MySql.Models
{
    [Table("phone_wallpaper")]
    public class DbPhoneWallpaper
    {
        [Key]
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "p")]
        public string Image { get; set; }
        public uint RestrictedTeam { get; set; }
        public uint RestrictedPlayer { get; set; }
        public bool RestrictedForStaff { get; set; }
    }
}
