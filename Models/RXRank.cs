using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class RXRank
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public int Permission { get; set; }
        public int ClothesId { get; set; }

        public ulong DiscordRole { get; set; }

        public uint UprankPoints { get; set; }

        public RXRank() { }
    }
}
