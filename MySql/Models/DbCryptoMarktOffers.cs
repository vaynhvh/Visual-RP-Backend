using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("crypto_offers")]
    public class DbCryptoMarktOffers
    {
        [Key]
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonIgnore]
        public uint PlayerId { get; set; }

        [JsonProperty(PropertyName = "n")]
        public double Coins { get; set; }
        [JsonProperty(PropertyName = "v")]
        public double Value { get; set; }
        [JsonProperty(PropertyName = "t")]
        public string Datum { get; set; }
        [JsonProperty(PropertyName = "isOwn")]
        public bool isOwn { get; set; }
    }
}
