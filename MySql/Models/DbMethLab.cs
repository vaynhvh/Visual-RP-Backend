using Backend.Models;
using Backend.Modules.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using static Backend.Models.RXContainer;

namespace Backend.MySql.Models
{

    public class Parameter
    {
        public string Name { get; set; }
        public string Einheit { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float ActValue { get; set; }

        public Parameter(string name, string einheit, float minValue, float maxValue, float actValue)
        {
            Name = name;
            Einheit = einheit;
            MinValue = minValue;
            MaxValue = maxValue;
            ActValue = actValue;
        }
    }



    public class Production
    {
        public List<uint> NeededItems { get; set; }
        public List<uint> EndProducts { get; set; }
        public uint MinEndProduct { get; set; }
        public uint MaxEndProduct { get; set; }

        public Production(List<uint> neededItems, List<uint> endProducts, uint minEndProduct, uint maxEndProduct, float smellrangePerPlayer = 2.0f, float smellRangeOffset = 0.0f)
        {
            NeededItems = neededItems;
            EndProducts = endProducts;
            MinEndProduct = minEndProduct;
            MaxEndProduct = maxEndProduct;
        }
    }

    [Table("methlab")]
    public class DbMethLab
    {
        [Key]
        public uint Id { get; set; }
        public uint TeamId { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public uint FuelContainerId { get; set; }
        public float Temperature { get; set; }
        public float Druck { get; set; }
        public float Ruehrgeschwindigkeit { get; set; }
        public float Menge { get; set; }
        public int CalculatedValue { get; set; }
        public double Quality { get; set; }
        public bool IsRunning { get; set; }

        [NotMapped]
        public List<RXPlayer> ActivePlayers { get; set; } = new List<RXPlayer>();
        [NotMapped]
        public bool HackInProgress { get; set; } = false;
        [NotMapped]
        public bool FriskInProgress { get; set; } = false;
        [NotMapped]
        public bool Locked { get; set; } = true;
        [NotMapped]
        public bool ImpoundInProgress { get; set; } = false;
        [NotMapped]
        public bool LaborMemberCheckedOnHack { get; set; } = false;
        [NotMapped]
        public bool HasDefended { get; set; } = false;
        [NotMapped]
        public DateTime LastAttacked { get; set; } = DateTime.Now;

        [NotMapped]
        public RXContainerObj LabFuelContainer
        {
            get => ContainerModule.Containers.FirstOrDefault(s => s.Id == this.FuelContainerId);
        }

        [NotMapped]
        public Production LabProduction { get; set; }

        [NotMapped]
        public List<Parameter> Parameters { get; set; }


    }
}
