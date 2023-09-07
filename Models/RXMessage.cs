using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class RXMessage
    {
        public string Text { get; set; } = "";
        public int Duration { get; set; } = 3500;
        public string Color { get; set; } = "";
        public string Title { get; set; } = "";
        public uint RestrictedToTeam { get; set; } = 0;
        public bool OnlyBadFaction { get; set; } = false;

        public RXMessage() { }
    }
}
