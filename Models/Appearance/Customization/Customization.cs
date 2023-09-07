using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models.Appearance
{
    class Customization
    {
		public int Gender { get; set; }

		public Parents Parents { get; set; }

		public float[] Features { get; set; }

		public Hairs Hair { get; set; }

		public List<Appearance> Appearance { get; set; }

		public int EyebrowColor { get; set; }

		public int BeardColor { get; set; }

		public int EyeColor { get; set; }

		public int BlushColor { get; set; }

		public int LipstickColor { get; set; }

		public int ChestHairColor { get; set; }
	}
}
