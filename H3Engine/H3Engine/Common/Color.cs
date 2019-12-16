using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Common
{
    public class Color
    {
        public byte R
        {
            get; set;
        }

        public byte G
        {
            get; set;
        }

        public byte B
        {
            get; set;
        }

        public byte A
        {
            get; set;
        }

        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            Color color = new Color();
            color.A = a;
            color.R = r;
            color.G = g;
            color.B = b;

            return color;
        }

    }
}
