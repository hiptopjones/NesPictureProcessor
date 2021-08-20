using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    class SystemPalette
    {
        public const int ColorCount = 64;

        private Color[] Colors { get; set; }

        public SystemPalette(Color[] colors)
        {
            if (colors.Length != ColorCount)
            {
                throw new ArgumentException($"Unexpected number of colors provided: {colors.Length} (expected: {ColorCount})");
            }

            Colors = colors;
        }

        public Color GetColor(int colorIndex)
        {
            if (colorIndex < 0 || colorIndex >= ColorCount)
            {
                throw new ArgumentException($"Requested color index is out of bounds: {colorIndex} (expected: 0 <= i < {ColorCount})");
            }

            return Colors[colorIndex];
        }
    }
}
