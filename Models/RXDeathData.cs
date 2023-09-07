using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class RXDeathData
    {
        public bool IsDead { get; set; } = false;
        public DateTime DeathTime { get; set; } = new DateTime(0);

        public RXDeathData() { }
    }
}
