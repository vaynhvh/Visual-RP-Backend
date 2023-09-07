using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("item_models")]
    public class DbItemModel
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; } = "";
        public int Weight { get; set; } = 0;
        public bool Illegal { get; set; } = false;
        public string Script { get; set; } = "";
        public int MaximumStackSize { get; set; } = 16;
        public bool RemoveOnUse { get; set; } = true;
        public string ItemModel { get; set; }   
        public string WeaponHash { get; set; } = string.Empty;
        public string ImagePath { get; set; } = "";
    }

    [Table("container_items")]
    public class DbItem
    {
        [Key]
        public uint Id { get; set; }
        public int Slot { get; set; } = 0;
        public uint InventoryId { get; set; } = 0;
        public uint ItemModelId { get; set; } = 0;
        public int Amount { get; set; } = 0;
    }
}
