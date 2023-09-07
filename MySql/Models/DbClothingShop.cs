using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("clothing_shops")]
    public class DbClothingShop
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }

        [NotMapped]
        public List<DbCloth> Clothes { get; set; }

        public DbClothingShop(uint id, string name, Vector3 position)
        {
            Id = id;
            Name = name;
            Position = position;
        }
    }

    public abstract class DbCloth
    {
        [Key]
        [JsonProperty("i")]    
        public uint Id { get; set; }
        [JsonProperty("c")]
        public int ComponentId { get; set; }
        [JsonProperty("d")]
        public int DrawableId { get; set; }
        [JsonProperty("t")]
        public int TextureId { get; set; }
        [JsonIgnore]
        public string ClothingShops { get; set; } = "[]";
        [JsonProperty("n")]
        public string Name { get; set; }
        [JsonProperty("p")]
        public int Price { get; set; }

        [NotMapped]
        [JsonProperty("prop")]
        public bool Prop { get; set; } = false;

        [NotMapped]
        [JsonIgnore]
        public bool Male { get; set; } = true;

        [NotMapped]
        [JsonIgnore]
        public List<uint> ClothingShopList { get; set; } = new List<uint>();
    }

    [Table("clothes_male")]
    public class DbMaleCloth : DbCloth { }

    [Table("clothes_female")]
    public class DbFemaleCloth : DbCloth { }

    [Table("masks")]
    public class DbMask : DbCloth { }

    [Table("props_male")]
    public class DbMaleProp : DbCloth { }

    [Table("props_female")]
    public class DbFemaleProp : DbCloth { }
}
