using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("wardrobe_items")]
    public class DbWardrobeItem
    {
        [Key]
        [JsonProperty("i")]
        public uint Id { get; set; }
        public uint PlayerId { get; set; } = 0;
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("c")]
        public int ComponentId { get; set; } = 0;
        [JsonProperty("d")]
        public int DrawableId { get; set; } = 0;
        [JsonProperty("t")]
        public int TextureId { get; set; } = 0;
        [JsonIgnore]
        public bool IsProp { get; set; } = false;
        [JsonIgnore]
        public bool Gender { get; set; } = true;
    }

    [Table("wardrobe_outfits")]
    public class DbWardrobeOutfit
    {
        [Key]
        public uint Id { get; set; }
        public uint PlayerId { get; set; } = 0;
        public string Name { get; set; }
        public string Clothes { get; set; } = "{}";
        public string Accessories { get; set; } = "{}";
        public bool Gender { get; set; } = true;
    }
}
