using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models.Appearance
{
    class HeadOverlay
    {
        public byte Index { get; set; }
        public byte Color { get; set; }
        public byte SecondaryColor { get; set; }
        public float Opacity { get; set; }

        public HeadOverlay(byte index, byte color, byte secondaryColor, float opacity)
        {
            this.Index = index;
            this.Color = color;
            this.SecondaryColor = secondaryColor;
            this.Opacity = opacity;
        }
    }
}
