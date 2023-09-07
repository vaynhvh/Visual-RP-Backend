using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class RXClothing
    {
        [JsonProperty("i")]
        public uint Id { get; set; }
        [JsonProperty("n")]
        public string Name { get; set; }
        public int Price { get; set; }
        public int ComponentId { get; set; }
        public int DrawableId { get; set; }
        public int TextureId { get; set; }
        public bool Prop { get; set; } = false;

        public RXClothing() { }
    }

    public class RXClothingSlot
    {
        [JsonProperty(PropertyName = "i")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        public RXClothingSlot() { }
    }

    public class RXClothingCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public RXClothingCategory() { }
    }

    public class RXClothingShop
    {
        [JsonProperty(PropertyName = "i")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "data")]
        public List<RXClothingSlot> Slots { get; set; }

        public RXClothingShop() { }
    }
}
