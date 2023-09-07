using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Backend.Models
{
    public class RXTraining
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Dictionary<Vector4, RXTrainingType> Positions { get; set; }
        public float Range { get; set; }
    }

    public enum RXTrainingType
    {
        Situps,
        Jogging,
        Liegestütze,
        Arme,
    }
}
