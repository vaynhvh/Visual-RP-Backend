using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models.Appearance
{
    public class RXClothesProp
    {
        public int clothid { get; set; }
        public int drawable { get; set; }
        public int texture { get; set; }
        public bool active { get; set; }

        public RXClothesProp() { }
    }
}
