using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class RXInjury
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ScreenEffect { get; set; }
        public int Duration { get; set; }
        public int DamageScale { get; set; }
        public string AnimDict { get; set; }
        public string AnimName { get; set; }

    }
}
