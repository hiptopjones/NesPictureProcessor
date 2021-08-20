using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileEngine
{
    // TODO: Make the size of tiles configurable and enable asymmetric tiles
    // TODO: Make the color depth of tiles configurable
    class Tile
    {
        // Tiles are 8x8 bytes
        public const int PixelCountX = 8;
        public const int PixelCountY = 8;
        public const int BitDepth = 2;
        public const int PixelCount = PixelCountX * PixelCountY;
        public const int PackedPixelByteCount = PixelCount / 8;

        public byte[] PackedLow { get; set; }
        public byte[] PackedHigh { get; set; }

        // NES pattern table format
        public Tile(byte[] packedLow, byte[] packedHigh)
        {
            if (packedLow.Length != PackedPixelByteCount)
            {
                throw new ArgumentException($"Unexpected number of packed low bytes provided: {packedLow.Length} (expected: {PackedPixelByteCount})");
            }

            if (packedHigh.Length != PackedPixelByteCount)
            {
                throw new ArgumentException($"Unexpected number of packed high bytes provided: {packedHigh.Length} (expected: {PackedPixelByteCount})");
            }

            PackedLow = packedLow;
            PackedHigh = packedHigh;
        }

        public int GetPixelValue(int x, int y)
        {
            if (x < 0 || x >= PixelCountX)
            {
                throw new ArgumentException($"Requested X coordinate is out of bounds: {x} (expected: 0 <= x < {PixelCountX})");
            }

            if (y < 0 || y >= PixelCountY)
            {
                throw new ArgumentException($"Requested Y coordinate is out of bounds: {y} (expected: 0 <= y < {PixelCountY})");
            }

            // Broken out to facilitate debugging intermediate values
            int indexedPackedLow = PackedLow[y];
            int shiftedPackedLow = indexedPackedLow >> (7 - x);
            int maskedPackedLow = shiftedPackedLow & 1;
            
            int indexedPackedHigh = PackedHigh[y];
            int shiftedPackedHigh = indexedPackedHigh >> (7 - x);
            int maskedPackedHigh = shiftedPackedHigh & 1;
            
            int pixelValue = maskedPackedLow | (maskedPackedHigh << 1);
            return pixelValue;
        }
    }
}
