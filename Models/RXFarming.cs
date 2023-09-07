using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class RXFarming
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ItemId { get; set; }
        public int RequiredItemId { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public List<Vector3> Positions { get; set; }
        public float Range { get; set; }
        public uint RestrictedToTeam { get; set; } = 0;
        public bool OnlyBadFaction { get; set; } = false;
    }
}
