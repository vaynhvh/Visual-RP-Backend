using Backend.MySql.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.Models
{
    public class RXItemModel
    {
        [Key]
        [JsonProperty(PropertyName = "itemId")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = "";
        [JsonProperty(PropertyName = "weight")]
        public int Weight { get; set; } = 0;
        public bool Illegal { get; set; } = false;
        [JsonProperty(PropertyName = "customData")]
        public string Script { get; set; } = "";
        [JsonIgnore]
        public int MaximumStackSize { get; set; } = 16;
        [JsonIgnore]
        public bool RemoveOnUse { get; set; } = true;
        [JsonIgnore]
        public string WeaponHash { get; set; } = string.Empty;
        [JsonIgnore]
        public string ImagePath { get; set; } = "";
        [JsonIgnore]
        public string ItemModel { get; set; } = string.Empty;
        [JsonIgnore]
        public int AttachmentOnlyId { get; set; }

        public RXItemModel(uint id, string name = "", int weight = 0, bool illegal = false, string script = "", int maxstack = 16, bool removeonuse = true, string weaponhash = "", string imagepath = "", string itemmodel = "", int attachmentonlyid = 0)
        {
            Id = id;
            Name = name;
            Weight = weight;
            Illegal = illegal;
            Script = script;
            MaximumStackSize = maxstack;
            RemoveOnUse = removeonuse;
            WeaponHash = weaponhash;
            ImagePath = imagepath;
            ItemModel = itemmodel;
            AttachmentOnlyId = attachmentonlyid;
        }
    }

    public class RXItem : DbItem
    {
        public RXItemModel Model { get; set; }

        public RXItem() { }
    }
}
