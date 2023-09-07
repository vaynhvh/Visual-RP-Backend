using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class RXShopProduct
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "p")]
        public int Price { get; set; } = 0;

        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; } = "";

        public RXShopProduct() { }
    }

    public class RXShop
    {
        [JsonProperty(PropertyName = "n")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "m")]
        public uint Money { get; set; }
        [JsonProperty(PropertyName = "r")]
        public bool isRob { get; set; }
        [JsonProperty(PropertyName = "fs")]
        public bool isFrakshop { get; set; } = false;

        [JsonProperty(PropertyName = "data")]
        public List<RXShopProduct> Items { get; set; } = new List<RXShopProduct>();

        public RXShop() { }
    }
}
