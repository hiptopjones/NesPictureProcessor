using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    class PaletteGroup
    {
        public const int IndexCount = 4;

        private int[] PaletteIndexes { get; set; }

        public PaletteGroup(int[] paletteIndexes)
        {
            if (paletteIndexes.Length != IndexCount)
            {
                throw new ArgumentException($"Unexpected number of palette indexes provided: {paletteIndexes.Length} (expected: {IndexCount})");
            }

            PaletteIndexes = paletteIndexes;
        }

        public int GetPaletteIndex(int paletteIndex)
        {
            if (paletteIndex < 0 || paletteIndex >= IndexCount)
            {
                throw new ArgumentException($"Requested palette index is out of bounds: {paletteIndex} (expected: 0 <= i < {IndexCount})");
            }

            return PaletteIndexes[paletteIndex];
        }
    }
}
