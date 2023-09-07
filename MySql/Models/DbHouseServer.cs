using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("houses_server")]
    public class DbHouseServer
    {
        [Key]
        [JsonProperty(PropertyName ="i")]
        public uint id { get; set; }
        [JsonProperty(PropertyName = "hi")]
        public uint houseid { get; set; }
        [JsonProperty(PropertyName = "status")]
        public bool isActive { get; set; }

        [JsonProperty(PropertyName = "crypto")]
        public uint CryptoValue { get; set; }
        [JsonProperty(PropertyName = "graphic")]
        public uint GraphicCard { get; set; }
        [JsonProperty(PropertyName = "cpu")]
        public uint CPU { get; set; }
        [JsonProperty(PropertyName = "powersupply")]
        public uint Netzteil { get; set; }
        [JsonProperty(PropertyName = "memory")]
        public uint RAM { get; set; }




    }
}
