using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("houses_serverlogs")]
    public class DbHouseServerLogs
    {
        [Key]
        [JsonProperty(PropertyName ="i")]
        public uint id { get; set; }

        [JsonProperty(PropertyName = "si")]
        public uint serverid { get; set; }

        [JsonProperty(PropertyName = "pi")]
        public uint playerid { get; set; }

        [JsonProperty(PropertyName = "v")]
        public uint value { get; set; }

        [JsonProperty(PropertyName = "t")]
        public string date { get; set; }
    }
}
