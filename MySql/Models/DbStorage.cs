using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace Backend.MySql.Models
{

    [Table("storage")]
    public class DbStorage
    {
        [Key]
        public uint Id { get; set; }
        public int Ausbaustufe { get; set; }
        public uint OwnerId { get; set; }
        public int Price { get; set; }
        public string Position { get; set; }
        public float Heading { get; set; }
        public uint ContainerSlots { get; set; }
        public uint ContainerWeight { get; set; }
        public uint Container1Id { get; set; }
        public uint Container2Id { get; set; }
        public uint Container3Id { get; set; }
        public uint Container4Id { get; set; }
        public uint Container5Id { get; set; }
        public uint Container6Id { get; set; }
        public bool CocainLabor { get; set; }
        public bool MainFlagged { get; set; }

        [NotMapped]
        public bool Locked { get; set; } = true;
        [NotMapped]
        public bool ChestLocked { get; set; } = true;
        [NotMapped]
        public GTANetworkAPI.Object Kiste1 { get; set; }
        [NotMapped]
        public GTANetworkAPI.Object Kiste2 { get; set; }
        [NotMapped]
        public GTANetworkAPI.Object Kiste3 { get; set; }
        [NotMapped]
        public GTANetworkAPI.Object Kiste4 { get; set; }
        [NotMapped]
        public GTANetworkAPI.Object Kiste5 { get; set; }
        [NotMapped]
        public GTANetworkAPI.Object Kiste6 { get; set; }
    }
}
