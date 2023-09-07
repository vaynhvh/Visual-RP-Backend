using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models.Appearance
{
    class HeadBlend
    {
        public byte ShapeFirst { get; set; }
        public byte ShapeSecond { get; set; }
        public byte ShapeThird { get; set; }
        public byte SkinFirst { get; set; }
        public byte SkinSecond { get; set; }
        public byte SkinThird { get; set; }
        public float ShapeMix { get; set; }
        public float SkinMix { get; set; }
        public float ThirdMix { get; set; }
    }
}
