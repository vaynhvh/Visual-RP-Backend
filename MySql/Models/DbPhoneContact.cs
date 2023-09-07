using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("phone_contacts")]
    public class DbPhoneContact
    {
        [Key]
        [JsonIgnore]
        public uint Id { get; set; }

        [JsonIgnore]
        public uint PlayerId { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "t")]
        public string Note { get; set; }

        [JsonProperty(PropertyName = "i")]
        public uint Number { get; set; }
    }
}
